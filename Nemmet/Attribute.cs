namespace DeaneBarker.Nemmet
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
