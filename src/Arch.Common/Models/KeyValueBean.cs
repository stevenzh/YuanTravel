using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arch.Common.Models
{
    [Serializable]
    public class KeyValueBean
    {
        public KeyValueBean()
        {
        }

        public KeyValueBean(string value, string key)
        {
            this.Value = value;
            this.Key = key;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
