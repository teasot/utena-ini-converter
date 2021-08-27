using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using IniParser;

namespace ParseUtenaScript
{
    class Program
    {
        static void Main(string[] args)
        {
            // Location of translation INI (english for now)
            string InputFile;
            string OutputFolder;
            if (args.Length == 2)
            {
                InputFile = args[0];
                OutputFolder = args[1];
            }
            else {
                InputFile = @"D:\Emulation\Sega Saturn\ROMS\Utena\utena translator 104\utena_eng.ini";
                OutputFolder = @"D:\Emulation\Sega Saturn\ROMS\Utena\";
            }

            ParseUtena(InputFile, OutputFolder);

        }

        static void ParseUtena(string Location, string OutputFolder)
        {
            List<IniLineItem> LineItems = new List<IniLineItem>();

            foreach (string Line in File.ReadAllLines(Location))
                LineItems.Add(IniLineItem.Parse(Line));

            ActorPhrases CurrentPhrase = null;
            OptionGroup CurrentOptionGroup = null;

            List<ActorPhrases> Phrases = new List<ActorPhrases>();
            List<OptionGroup> OptionGroups = new List<OptionGroup>();

            int OptionIndex = 1;

            Dictionary<string, VoicedLine> VoicedLineMapping = new Dictionary<string, VoicedLine>();
            Dictionary<string, OptionGroup> OptionGroupMapping = new Dictionary<string, OptionGroup>();

            bool IsActor = false;
            bool IsOptions = false;

            List<string> InvalidLHS = new List<string> { "fix_duplicate_names", "user_linking", "machine_linking", "rewrite_all_va" }; 

            foreach (IniLineItem LineItem in LineItems)
            {
                switch (LineItem.Type)
                {
                    case nameof(EmptyLine):
                        if (LineItem.HasComment)
                        {
                            if (LineItem.Comment.Trim().StartsWith("Actor:"))
                            {
                                if (IsOptions)
                                {
                                    CurrentOptionGroup = null;
                                }
                                IsActor = true;
                                IsOptions = false;

                                CurrentPhrase = new ActorPhrases { Actor = LineItem.Comment.Split("Actor:")[1].Trim() };
                                Phrases.Add(CurrentPhrase);
                            }
                            else if (LineItem.Comment.Trim().StartsWith("Options"))
                            {

                                // Dump existing actor
                                if (IsActor)
                                {
                                    Phrases.Add(CurrentPhrase);
                                    CurrentPhrase = null;
                                }
                                IsActor = false;
                                IsOptions = true;
                            }
                        }
                        break;
                    case nameof(Assignment):
                        Assignment AssignmentLine = (Assignment)LineItem;
                        if (!InvalidLHS.Contains(AssignmentLine.LHS))
                        { 
                            if (IsActor)
                            {
                                // Original voiced line 
                                VoicedLine VoicedLine = new VoicedLine();
                                VoicedLine.VoiceLine = AssignmentLine.LHS;

                                if (AssignmentLine.IsString)
                                {
                                    // Is a voice line
                                    VoicedLine.Text = AssignmentLine.RHS;
                                    CurrentPhrase.Lines.Add(VoicedLine);
                                }
                                else
                                {
                                    // Is a mapping
                                    VoicedLine.Text = VoicedLineMapping[AssignmentLine.RHS].Text;
                                    CurrentPhrase.Lines.Add(VoicedLine);
                                }
                                VoicedLineMapping[VoicedLine.VoiceLine] = VoicedLine;
                            }
                            else if (IsOptions)
                            {
                                string[] LHSSplit = AssignmentLine.LHS.Split(" ");
                                switch (LHSSplit.Length)
                                {
                                    case 1:
                                        // New option group, so push the old one and create a new one

                                        CurrentOptionGroup = new OptionGroup {
                                            ID = "OptionGroup" + OptionIndex++.ToString()
                                        };
                                        OptionGroups.Add(CurrentOptionGroup);
                                        OptionGroupMapping[CurrentOptionGroup.ID] = CurrentOptionGroup;

                                        foreach (string Value in AssignmentLine.RHS.Split('/'))
                                            CurrentOptionGroup.Options.Add(Value);
                                        break;
                                    default:
                                        // Generate description
                                        string Description = "";
                                        if (LHSSplit.Length == 2)
                                            Description = LHSSplit[0];
                                        else
                                            for (int i = 0; i < LHSSplit.Length - 1; i++)
                                                Description += LHSSplit[i];

                                        // Get Choice Index
                                        string ChoiceName = LHSSplit[LHSSplit.Length - 1];
                                        int ChoiceIndex = int.Parse(ChoiceName.Replace("OPTION", ""));

                                        // Create new option group if description inconsistent
                                        // This ONLY works cos 
                                        if (CurrentOptionGroup == null || CurrentOptionGroup.Options.Count >= ChoiceIndex)
                                        {
                                            CurrentOptionGroup = new OptionGroup
                                            {
                                                ID = "OptionGroup" + OptionIndex++.ToString(),
                                                Description = Description
                                            };
                                            OptionGroups.Add(CurrentOptionGroup);
                                            OptionGroupMapping[CurrentOptionGroup.ID] = CurrentOptionGroup;
                                        }

                                        CurrentOptionGroup.Options.Add(AssignmentLine.RHS);
                                        break;
                                }
                            }
                        }
                        break;
                    case nameof(Section):
                        break;
                }
            }

            // Write JSON files
            // Initialise file location
            string PhrasesFile = Path.Combine(OutputFolder, "EN_phrases.json");
            string OptionsFile = Path.Combine(OutputFolder, "EN_options.json");

            // Write phrases
            using (StreamWriter Writer = File.CreateText(PhrasesFile))
            {
                JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented});
                Serializer.Serialize(Writer, VoicedLineMapping);
            }
            // Write options
            using (StreamWriter Writer = File.CreateText(OptionsFile))
            {
                JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });
                Serializer.Serialize(Writer, OptionGroupMapping);
            }
        }
    }
}
