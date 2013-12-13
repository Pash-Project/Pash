// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;

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

        public PSMemberInfoCollection<PSMemberInfo> Members { get; private set; }

        public PSMemberInfoCollection<PSMethodInfo> Methods { get; private set; }

        public PSMemberInfoCollection<PSPropertyInfo> Properties { get; private set; }

        public Collection<string> TypeNames { get; private set; }

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

        protected void Initialize(object obj)
        {
            Members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            Properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
            Methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            ImmediateBaseObject = obj;
        }

        public override bool Equals(object obj)
        {
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
                return "<null>";
            }
            return ImmediateBaseObject.ToString();
        }

        public static PSObject AsPSObject(object obj)
        {
            PSObject _psobj = obj as PSObject;

            if (_psobj != null)
                return _psobj;

            return new PSObject(obj);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
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
