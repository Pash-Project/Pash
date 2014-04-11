using System;
using System.Reflection;

namespace System.Management.Automation
{
    public class PSProperty : PSPropertyInfo
    {
        private PropertyInfo _propertyInfo;

        internal object Owner { get; private set; }

        private bool _isGettable;
        public override bool IsGettable {
            get
            {
                return _isGettable;
            }
        }

        private bool _isSettable;
        public override bool IsSettable {
            get
            {
                return _isSettable;
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.Property;
            }
        }

        public override object Value
        {
            get
            {
                try
                {
                    return _propertyInfo.GetValue(Owner, null);
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
                    _propertyInfo.SetValue(Owner, value, null);
                }
                catch (Exception e)
                {
                    throw new SetValueException("Value cannot be set", e);
                }
            }
        }

        public override string TypeNameOfValue {
            get
            {
                return _propertyInfo.PropertyType.FullName;
            }
        }

        public override PSMemberInfo Copy()
        {
            return new PSProperty(_propertyInfo, Owner);
        }

        internal PSProperty(object owner, bool gettable, bool settable)
        {
            Owner = owner;
            _isGettable = gettable;
            _isSettable = settable;
        }

        internal PSProperty(PropertyInfo info, object owner)
            : this(owner, info.CanRead, info.CanWrite)
        {
            Name = info.Name;
            _propertyInfo = info;
        }
    }
}
