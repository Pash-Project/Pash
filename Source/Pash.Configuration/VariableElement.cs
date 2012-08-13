namespace Pash.Configuration
{
    public class VariableElement
    {
        private string name1;
        private string type1;
        private string value1;
        private string scope1;

        // Required, key, R/O
        public string name { get; private set; }
        // Default value "System.String"
        public string type { get; set; }
        // Default value "null"
        public string value { get; set; }
        // Default value "global", R/O
        public string scope { get; private set; }

        public VariableElement()
        {
            type = "System.String";
            value = null;
            scope = "global";
        }

        public VariableElement(string name, string type, string value, string scope) : this()
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