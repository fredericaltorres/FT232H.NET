using Newtonsoft.Json;
using System.Collections.Generic;

namespace fAI.Tests
{
    public class LedSequence
    {
        [JsonProperty("sequences")]
        public List<Sequence> Sequences { get; set; }
    }
    
    public class Sequence
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("actions")]
        public List<Action> Actions { get; set; }
    }

    public class Action
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public bool On => this.Value == "ON";

        public int ValueAsInt => int.Parse(Value);

        public int GetLedIndex()
        {
            return int.Parse(Command.Replace("LED_", ""));
        }
    }
}

