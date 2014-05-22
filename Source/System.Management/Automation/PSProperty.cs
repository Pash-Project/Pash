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
                    var msg = String.Format("Value '{0}' cannot be read", _propertyInfo.Name);
                    throw new GetValueException(msg, e);
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
            return new PSProperty(_propertyInfo, Owner, IsInstance);
        }

        internal PSProperty(object owner, string name, bool gettable, bool settable, bool isInstance)
        {
            Name = name;
            IsInstance = isInstance;
            Owner = owner;
            _isGettable = gettable;
            _isSettable = settable;
        }

        internal PSProperty(PropertyInfo info, object owner, bool isInstance)
            : this(owner, info.Name, info.CanRead, info.CanWrite, isInstance)
        {
            _propertyInfo = info;
        }
    }
}

