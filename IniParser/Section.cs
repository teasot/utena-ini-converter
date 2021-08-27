using System;
using System.Collections.Generic;
using System.Text;

namespace IniParser
{
    public class Section : IniLineItem
    {
        public override string Type { get; } = nameof(Section);
        public string Name { get; set; }
        public override string ToString()
        {
            return "[" + Name + "]";
        }
    }
}
