namespace Pash.Configuration
{
    public class AliasElement
    {
        private string name1;
        private string definition1;
        private string scope1;

        // Required, key, R/O
        public string name { get; private set; }
        // Required
        public string definition { get; set; }
        // Default value "global"
        public string scope { get; set; }

        public AliasElement()
        {
            scope = "global";
        }

        public AliasElement(string name, string definition, string scope) : this()
        {
            this.name = name;
            this.definition = definition;
            if (scope != null)
                this.scope = scope;
        }
    }
}