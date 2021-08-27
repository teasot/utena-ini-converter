using System;
using System.Collections.Generic;
using System.Text;

namespace ParseUtenaScript
{
    class ActorPhrases
    {
        public string Actor { get; set; }
        public List<VoicedLine> Lines { get; } = new List<VoicedLine>();
    }
}
