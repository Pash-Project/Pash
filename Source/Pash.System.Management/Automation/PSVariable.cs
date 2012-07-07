using System;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
    public class PSVariable
    {
        public string Name { get; private set; }
        public virtual string Description { get; set; }
        public virtual object Value { get; set; }
        public virtual ScopedItemOptions Options { get; set; }
        public Collection<Attribute> Attributes { get; private set; }

        public PSVariable(string name)
            : this(name, null, ScopedItemOptions.None, null)
        {
        }

        public PSVariable(string name, object value)
            : this(name, value, ScopedItemOptions.None, null)
        {
        }

        public PSVariable(string name, object value, ScopedItemOptions options)
            : this(name, value, options, null)
        {
        }

        public PSVariable(string name, object value, ScopedItemOptions options, Collection<Attribute> attributes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new NullReferenceException("Variable name can't be empty");
            }

            Name = name;
            Description = string.Empty;
            Value = value;
            Options = options;

            // Copy attributes
            Attributes = new Collection<Attribute>();
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    Attributes.Add(attribute);
                }
            }
        }


        public virtual bool IsValidValue(object value)
        {
            throw new NotImplementedException();
        }

        // internals
        //internal static bool IsValidValue(object value, System.Attribute attribute);
        //internal PSVariable(string name, object value, System.Management.Automation.ScopedItemOptions options, System.Collections.ObjectModel.Collection<Attribute> attributes, string description);
        //internal PSVariable(string name, object value, System.Management.Automation.ScopedItemOptions options, string description);
        //internal void SetOptions(System.Management.Automation.ScopedItemOptions newOptions, bool force);
        //internal void SetValueRaw(object newValue, bool preserveValueTypeSemantics);
        //internal object TransformValue(object value);
        //internal bool IsAllScope { get; }
        //internal bool IsConstant { get; }
        internal bool IsPrivate { get; private set; }
        //internal bool IsReadOnly { get; }
        //internal bool WasRemoved { set; get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('$');
            sb.Append(Name);
            sb.Append(" = ");
            sb.Append(Value.ToString());

            return sb.ToString();
        }
    }
}
