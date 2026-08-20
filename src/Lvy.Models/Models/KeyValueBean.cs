using System;

namespace Lvy.Models
{
    [Serializable]
    public class KeyValueBean
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public string Help1 { get; set; }
        public string Help2 { get; set; }
    }
}

