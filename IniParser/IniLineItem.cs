using System;
using System.Collections.Generic;
using System.Text;

namespace IniParser
{
    public abstract class IniLineItem
    {
        public abstract string Type { get; }

        public string Comment { get; set; }
        public bool HasComment { get { return !string.IsNullOrWhiteSpace(Comment); } }

        public static IniLineItem Parse(string Text)
        {
            Text = Text.Trim();

            // A literal empty line
            if (string.IsNullOrWhiteSpace(Text))
                return new EmptyLine();

            // And empty line, with a comment (the line is "empty" in the sense it does not do anything)
            if (Text[0] == ';')
                return new EmptyLine { Comment = Text.Substring(1) };

            // A section.
            if (Text[0] == '[')
            {
                Section ReturnSection = new Section();
                int IndexOfClosingBracket = Text.IndexOf(']');
                ReturnSection.Name = Text.Substring(1, IndexOfClosingBracket - 1);
                if (Text.Length > IndexOfClosingBracket + 1)
                {
                    string AfterBracket = Text.Substring(IndexOfClosingBracket + 1);
                    AfterBracket = AfterBracket.Trim();

                    // Dangerously, assume first character IS ";"
                    ReturnSection.Comment = AfterBracket.Substring(1);
                }

                return ReturnSection;
            }

            // Finally, it MUST be an assignment
            Assignment ReturnAssignment = new Assignment();
            string[] EqualsSplit = Text.Split("=");
            ReturnAssignment.LHS = EqualsSplit[0].Trim();
            string RawRHS = "";

            // Ignore LHS (first string in index)
            for(int i = 1; i < EqualsSplit.Length; i++)
                RawRHS += EqualsSplit[i];

            RawRHS = RawRHS.Trim();
            if (RawRHS[0] == '\"')
            {
                ReturnAssignment.IsString = true;

                string StringVal = "";
                int Index = 1;
                char PriorChar = '\"';

                // Loop through until string exited OR end of RawRHS
                while(Index < RawRHS.Length)
                {
                    char CurChar = RawRHS[Index];

                    // First handle double quotes
                    if (CurChar == '\"')
                    {
                        if (PriorChar == '\\')
                            StringVal += CurChar;
                        else
                            break;
                    }
                    else if (CurChar == 'n')
                    {
                        if (PriorChar == '\\')
                            StringVal += "\n";
                        else
                            StringVal += 'n';
                    }
                    else if (CurChar != '\\')
                    {
                        StringVal += CurChar;
                    }
                    // Finally save CurChar as PriorChar;
                    PriorChar = CurChar;
                    Index++;
                }

                // Now, Index is end of string
                ReturnAssignment.RHS = StringVal;

                // Finally, detect if there is a comment
                string PotentialComment = RawRHS.Substring(Index);
                if (PotentialComment.Contains(";"))
                {
                    ReturnAssignment.Comment = PotentialComment.Substring(PotentialComment.IndexOf(";") + 1).Trim();
                }
            }
            else
            {
                ReturnAssignment.IsString = false;
                if (RawRHS.IndexOf(";") > -1)
                {
                    ReturnAssignment.RHS = RawRHS.Substring(0, RawRHS.IndexOf(";")).Trim();
                    ReturnAssignment.Comment = RawRHS.Substring(RawRHS.IndexOf(";") + 1).Trim();
                }
                else
                    ReturnAssignment.RHS = RawRHS;
            }

            return ReturnAssignment;
        }
    }
}
