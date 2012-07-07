using System.Text;

namespace System.Management.Automation
{
    public class PSNoteProperty : PSPropertyInfo
    {
        static PSNoteProperty()
        {
            
        }

        public PSNoteProperty(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Name can't be empty");

            Name = name;
            _value = value;
        }

        private object _value;
        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!IsInstance)
                    // TODO: throw the SetValueException instead
                    throw new Exception("Can't change value of a static note.");

                _value = value;
            }
        }
        
        public override bool IsGettable
        {
            get
            {
                return true;
            }
        }

        public override bool IsSettable
        {
            get
            {
                return IsInstance;
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.NoteProperty;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                if (Value == null)
                    return string.Empty;

                return Value.GetType().FullName;
            }
        }

        public override PSMemberInfo Copy()
        {
            PSNoteProperty outVal = new PSNoteProperty(Name, Value);
            CopyProperties(outVal);
            return outVal;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(TypeNameOfValue);
            str.Append(" ");
            str.Append(Name);
            str.Append("=");
            str.Append((Value == null) ? "null" : Value.ToString());

            return str.ToString();
        }
    }
}