// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace Pash.Implementation
{
    class PSObjectProperties
    {
        public PSObjectProperties(object baseObject, bool isInstance)
        {
            this._baseObject = baseObject;
            this._isInstance = isInstance;

            _type = (_baseObject is Type && !_isInstance) ? (Type)_baseObject : _baseObject.GetType();
            _instanceObject = _isInstance ? _baseObject : null;
            _flags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
            _flags |= _isInstance ? BindingFlags.Instance : BindingFlags.Static;
        }

        private object _baseObject;
        private bool _isInstance;
        private Type _type;
        private object _instanceObject;
        private BindingFlags _flags;

        private bool _includeParameterizedProperties;

        public List<PSProperty> GetProperties()
        {
            _includeParameterizedProperties = false;

            var properties = (
                from propertyInfo in GetAllPropertyInfo()
                select new PSProperty(propertyInfo, _instanceObject, _isInstance)).ToList();

            properties.AddRange(
                from field
                in _type.GetFields(_flags)
                select new PSFieldProperty(field, _instanceObject, _isInstance)
            );
            return properties;
        }

        public List<PSParameterizedProperty> GetParameterizedProperties()
        {
            _includeParameterizedProperties = true;

            return (from propertyInfo in GetAllPropertyInfo()
                    select new PSParameterizedProperty(propertyInfo, _type, _instanceObject, _isInstance)).ToList();
        }

        private List<PropertyInfo> GetAllPropertyInfo()
        {
            // get all properties
            var propertyInfos = _type.GetProperties(_flags).Where(p => FilterProperty(p)).ToList();

            // TODO: maybe the following isn't necessary. I investigated this, because I saw that in PS you can access
            // an array's .Count property. However, I just read that they do this by adding this property through
            // Types.ps1xml. So although the following code did the same (as it indeed implemnts/inherits a Count property)
            // this did not seem to be the actual intention

            // get properties of all interfaces explicitly, as properties of interfaces implemented by interfaces aren't
            // included in the normal GetProperties() call
            _type.GetInterfaces().ToList().ForEach(i => propertyInfos.AddRange(i.GetProperties(_flags)
                .Where(propertyInfo => FilterProperty(propertyInfo))));

            propertyInfos = (from propertyInfo in propertyInfos
                             where FilterProperty(propertyInfo)
                             select propertyInfo).ToList();

            return (from propertyInfo
                    in propertyInfos.GroupBy(prop => prop.Name).Select(grp => grp.First())
                    select propertyInfo).ToList();
        }

        bool FilterProperty(PropertyInfo propertyInfo)
        {
            bool isParameterizedProperty = PSParameterizedProperty.IsParameterizedProperty(propertyInfo);
            if (_includeParameterizedProperties)
            {
                return isParameterizedProperty;
            }
            return !isParameterizedProperty;
        }
    }
}
