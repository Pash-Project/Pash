using System;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal class ObjectInfo
    {
        internal ObjectInfo(object obj)
        {
            this.Object = GetObject(obj);
            this.Type = GetObjectType();
        }

        private object GetObject(object obj)
        {
            if (obj is PSObject)
                return ((PSObject)obj).BaseObject;

            return obj;
        }

        private Type GetObjectType()
        {
            if (Object is Type)
                return (Type)Object;

            return Object.GetType();
        }

        internal Object Object { get; private set; }
        internal Type Type { get; private set; }
    }
}
