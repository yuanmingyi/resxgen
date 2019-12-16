namespace resxgen
{
    struct Data
    {
        public string Name;
        public string Value;
        public string Comment;

        public Data(string name, string value, string comment = "")
        {
            Name = name;
            Value = value;
            Comment = comment;
        }
    }
}
