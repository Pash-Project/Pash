namespace Pash.Configuration
{
    public class FunctionElement
    {
        private string name1;
        private string type1;
        private string value1;
        private string scope1;

        // Required, key, R/O
        public string name { get; private set; }
        // Default value "inline"
        public string type { get; set; }
        // Required
        public string value { get; set; }
        // Default value "global", R/O
        public string scope { get; private set; }

        public FunctionElement()
        {
            type = "inline";
            scope = "global";
        }

        public FunctionElement(string name, string type, string value, string scope) : this()
        {
            this.name = name;
            if (type != null)
                this.type = type;
            this.value = value;
            if (scope != null)
                this.scope = scope;
        }
    }
}