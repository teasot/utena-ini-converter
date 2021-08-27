using System;
using System.Collections.Generic;
using System.Text;

namespace ParseUtenaScript
{
    class Day
    {
        string DayName { get; set; }
        List<ActorPhrases> Phrases { get; } = new List<ActorPhrases>();
        List<OptionGroup> Options { get; } = new List<OptionGroup>();
    }
}
