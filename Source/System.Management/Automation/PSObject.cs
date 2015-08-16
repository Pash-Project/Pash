// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using Pash.Implementation;

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
                    InitParameterizedProperties();
                }
                if (_methods == null)
                {
                    InitMethods();
                }
                return _members;
            }
        }

        private PSMemberInfoCollection<PSMemberInfo> _staticMembers;
        internal PSMemberInfoCollection<PSMemberInfo> StaticMembers
        {
            get
            {
                if (_staticMembers == null)
                {
                    InitStaticMembers();
                }
                return _staticMembers;
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

        internal bool WriteToErrorStream
        {
            get
            {
                var prop = Properties["writeToErrorStream"];
                return (prop == null) ? false : prop.Value is bool && (bool)prop.Value;
            }
            set
            {
                var prop = Properties["writeToErrorStream"];
                if (prop != null)
                {
                    prop.Value = value;
                }
                else
                {
                    Properties.Add(new PSNoteProperty("writeToErrorStream", value));
                }
            }
        }

        private List<PSMethod> GetMethods(bool isInstance)
        {
            var baseObject = ImmediateBaseObject;
            var type = (baseObject is Type && !isInstance) ? (Type) baseObject : baseObject.GetType();
            var instanceObject = isInstance ? baseObject : null;
            BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
            flags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var methodNames = (from method in type.GetMethods(flags) select method.Name).Distinct();
            return (from name in methodNames select new PSMethod(name, type, instanceObject, isInstance)).ToList();
        }

        private List<PSProperty> GetProperties(bool isInstance)
        {
           return new PSObjectProperties(ImmediateBaseObject, isInstance).GetProperties();
        }

        private void InitMethods()
        {
            _methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            var methods = GetMethods(true);
            methods.ForEach(_methods.Add);
            methods.ForEach(_members.Add);
        }

        private void InitProperties()
        {
            _properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
            var properties = GetProperties(true);
            properties.ForEach(_properties.Add);
            properties.ForEach(_members.Add);
        }

        private void InitStaticMembers()
        {
            _staticMembers = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            GetMethods(false).ForEach(_staticMembers.Add);
            GetProperties(false).ForEach(_staticMembers.Add);
        }

        protected void Initialize(object obj)
        {
            if (obj == null)
            {
                throw new PSArgumentNullException("Argument \"obj\" is null"); 
            }
            _members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            ImmediateBaseObject = obj;
        }

        internal Collection<PSPropertyInfo> GetDefaultDisplayPropertySet()
        {
            // TODO: As soon as the extended type system is supported, we can check the types TypeData on initialization
            // and then get the DefaultDisplayPropertySet, a set that defines all properties of the object that should
            // be printed by default
            // For now we just return all properties
            var collection = new Collection<PSPropertyInfo>();
            foreach (var info in Properties)
            {
                if (info.IsGettable)
                {
                    collection.Add(info);
                }
            }
            return collection;
        }

        public override bool Equals(object obj)
        {
            if (obj is PSObject)
            {
                obj = ((PSObject)obj).ImmediateBaseObject;
            }
            return ImmediateBaseObject.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ImmediateBaseObject.GetHashCode();
        }

        public override string ToString()
        {
            return (string)LanguagePrimitives.ConvertTo(ImmediateBaseObject, typeof(string), Thread.CurrentThread.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return (string)LanguagePrimitives.ConvertTo(ImmediateBaseObject, typeof(string), formatProvider);
        }

        public static PSObject AsPSObject(object obj)
        {
            if (obj is PSObject)
            {
                return (PSObject) obj;
            }

            return new PSObject(obj);
        }

        internal static PSObject WrapOrNull(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return AsPSObject(obj);
        }

        internal static object Unwrap(object obj)
        {
            var psobj = obj as PSObject;
            return psobj == null ? obj : psobj.BaseObject;
        }

        public int CompareTo(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return 0;

            else
                return LanguagePrimitives.Compare(this.BaseObject, obj);
        }

        internal static PSMemberInfo GetMemberInfoSafe(PSObject psobj, object memberNameObj, bool isStatic)
        {
            if (memberNameObj == null)
            {
                throw new PSArgumentNullException("Member name is null");
            }
            var memberName = memberNameObj.ToString();
            if (psobj == null)
            {
                return null;
            }
            if (isStatic)
            {
                return psobj.StaticMembers[memberName];
            }
            return psobj.Members[memberName];
        }

        private void InitParameterizedProperties()
        {
            var properties = GetParameterizedProperties(true);
            properties.ForEach(_members.Add);
        }

        private List<PSParameterizedProperty> GetParameterizedProperties(bool isInstance)
        {
            return new PSObjectProperties(ImmediateBaseObject, isInstance).GetParameterizedProperties();
        }
    }
}
