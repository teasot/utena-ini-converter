using System;
using System.Collections.Generic;
using System.Text;

namespace IniParser
{
    public class EmptyLine : IniLineItem
    {
        public override string Type { get; } = nameof(EmptyLine);
    }
}
