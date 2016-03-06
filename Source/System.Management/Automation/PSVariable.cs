// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    public class PSVariable : IScopedItem
    {
        public string Name { get; private set; }
        public virtual string Description { get; set; }
        public virtual object Value { get; set; }
        public virtual ScopedItemOptions Options { get; set; }
        public SessionStateEntryVisibility Visibility { get; set; }
        public Collection<Attribute> Attributes { get; private set; }

        public PSModuleInfo Module { get; set; }

        public string  ModuleName
        {
            get
            {
                return Module == null ? "" : this.Module.Name;
            }
        }

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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('$');
            sb.Append(Name);
            sb.Append(" = ");
            if (Value != null)
            {
                sb.Append(Value.ToString());
            }

            return sb.ToString();
        }

        internal object GetBaseObjectValue()
        {
            if (Value is PSObject)
            {
                return ((PSObject)Value).BaseObject;
            }

            return Value;
        }

    
        #region IScopedItem Members

        public string ItemName
        {
            get { return Name; }
        }

        public ScopedItemOptions ItemOptions
        {
            get { return Options; }
            set { Options = value; }
        }

        #endregion

    }
}
