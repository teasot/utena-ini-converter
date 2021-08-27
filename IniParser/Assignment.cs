using System;
using System.Collections.Generic;
using System.Text;

namespace IniParser
{
    public class Assignment : IniLineItem
    {
        public override string Type { get; } = nameof(Assignment);
        public string LHS { get; set; }
        /// <summary>
        /// RHS property. Can be either the string in the case of string assignment, or 
        /// </summary>
        public string RHS { get; set; }
        public bool IsString { get; set; }
        public string GetStringValue()
        {
            if (IsString)
            {
                //Unquote RHS
                string ReturnValue = RHS.Substring(1, (RHS.Length - 2));
                //Replace \" by "
                ReturnValue.Replace(@"\""", @"""");
                return ReturnValue;
            }
            else
                return null;
        }

        public override string ToString()
        {
            return LHS + " = " + RHS + (HasComment ? (";" + Comment) : ""); ;
        }
    }
}
