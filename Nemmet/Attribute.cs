using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeaneBarker
{ 
    public class Attribute
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Attribute(string key, string value)
        {
            Key = key.ToLower();
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key.ToLower()}=\"{Value.Replace("\"", "&quot;")}\"";
        }
    }
}
