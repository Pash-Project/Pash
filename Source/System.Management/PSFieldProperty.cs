using System;
using System.Reflection;

namespace System.Management.Automation
{
    internal class PSFieldProperty : PSProperty
    {
        private FieldInfo _fieldInfo;

        public override object Value
        {
            get
            {
                try
                {
                    return _fieldInfo.GetValue(Owner);
                }
                catch (Exception e)
                {
                    throw new GetValueException("Value cannot be read", e);
                }
            }

            set
            {
                try
                {
                    _fieldInfo.SetValue(Owner, value);
                }
                catch (Exception e)
                {
                    throw new SetValueException("Value cannot be set", e);
                }
            }
        }

        public override PSMemberInfo Copy()
        {
            return new PSFieldProperty(_fieldInfo, Owner);
        }

        public override string TypeNameOfValue {
            get
            {
                return _fieldInfo.FieldType.FullName;
            }
        }

        internal PSFieldProperty(FieldInfo info, object owner)
            : base(owner, true, true)
        {
            Name = info.Name;
            _fieldInfo = info;
        }
    }
}

