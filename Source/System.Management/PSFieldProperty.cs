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
                    object convertedValue = LanguagePrimitives.ConvertTo(value, _fieldInfo.FieldType);
                    _fieldInfo.SetValue(Owner, convertedValue);
                }
                catch (Exception e)
                {
                    throw new SetValueException("Value cannot be set", e);
                }
            }
        }

        public override PSMemberInfo Copy()
        {
            return new PSFieldProperty(_fieldInfo, Owner, IsInstance);
        }

        public override string TypeNameOfValue {
            get
            {
                return _fieldInfo.FieldType.FullName;
            }
        }

        internal PSFieldProperty(FieldInfo info, object owner, bool isInstance)
            : base(owner, info.Name, true, true, isInstance)
        {
            _fieldInfo = info;
        }
    }
}

