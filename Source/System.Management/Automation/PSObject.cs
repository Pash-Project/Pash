// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;

namespace System.Management.Automation
{
    [TypeDescriptionProvider(typeof(PSObjectTypeDescriptionProvider))]
    public class PSObject : IFormattable, IComparable
    {
        public PSObject(object obj)
        {
            Initialize(obj);
        }

        public PSObject()
        {
            Initialize(PSCustomObject.Instance);
        }

        public object ImmediateBaseObject { get; private set; }

        private PSMemberInfoCollection<PSMemberInfo> _members;
        public PSMemberInfoCollection<PSMemberInfo> Members
        {
            get
            {
                if (_properties == null)
                {
                    InitProperties();
                }
                if (_methods == null)
                {
                    InitMethods();
                }
                return _members;
            }
        }

        private PSMemberInfoCollection<PSMethodInfo> _methods;
        public PSMemberInfoCollection<PSMethodInfo> Methods
        {
            get
            {
                if (_methods == null)
                {
                    InitMethods();
                }
                return _methods;
            }
        }

        private PSMemberInfoCollection<PSPropertyInfo> _properties;
        public PSMemberInfoCollection<PSPropertyInfo> Properties
        {
            get
            {
                if (_properties == null)
                {
                    InitProperties();
                }
                return _properties;
            }
        }

        private Collection<string> _typeNames;
        public Collection<string> TypeNames
        {
            get
            {
                if (_typeNames == null)
                {
                    _typeNames = new Collection<string>();
                    var type = BaseObject.GetType();
                    while (type != null)
                    {
                        _typeNames.Add(type.FullName);
                        type = type.BaseType;
                    }
                }
                return _typeNames;
            }
        }

        public object BaseObject
        {
            get
            {
                object objParent = null;
                PSObject obj = this;
                do
                {
                    objParent = obj.ImmediateBaseObject;
                    obj = objParent as PSObject;
                }
                while (obj != null);
                return objParent;
            }
        }

        private void InitMethods()
        {
            _methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            var baseObject = ImmediateBaseObject;
            var methods = (from method
                              in baseObject.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                           select new PSMethodInfo(method, baseObject)).ToList();
            methods.ForEach(_methods.Add);
            methods.ForEach(_members.Add);
        }

        private void InitProperties()
        {
            _properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
            var baseObject = ImmediateBaseObject;
            var properties = (from property
                              in baseObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              select new PSProperty(property, baseObject)).ToList();
            properties.ForEach(_properties.Add);
            properties.ForEach(_members.Add);
            var fields = (from field
             in baseObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                          select new PSFieldProperty(field, baseObject)).ToList();
            fields.ForEach(_properties.Add);
            fields.ForEach(_members.Add);
        }

        protected void Initialize(object obj)
        {
            _members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            ImmediateBaseObject = obj;
        }

        public override bool Equals(object obj)
        {
            if (obj is PSObject)
            {
                obj = ((PSObject)obj).ImmediateBaseObject;
            }
            if (ImmediateBaseObject == null)
            {
                return obj == null;
            }
            return ImmediateBaseObject.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (ImmediateBaseObject == null)
            {
                return 0;
            }
            return ImmediateBaseObject.GetHashCode();
        }

        public override string ToString()
        {
            if (ImmediateBaseObject == null)
            {
                return "";
            }
            return ImmediateBaseObject.ToString();
        }

        public static PSObject AsPSObject(object obj)
        {
            if (obj is PSObject)
            {
                return (PSObject) obj;
            }

            return new PSObject(obj);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            // TODO: a better implementation with format and formatProvider
            return ImmediateBaseObject.ToString();
        }

        public int CompareTo(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return 0;

            else
                return LanguagePrimitives.Compare(this.BaseObject, obj);
        }
    }
}
