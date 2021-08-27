using System;
using System.Collections.Generic;
using System.Text;

namespace ParseUtenaScript
{
    public class OptionGroup
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public List<string> Options { get; } = new List<string>(); 
    }
}
