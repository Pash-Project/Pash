using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Collections.Generic;

namespace Pash.Implementation
{
    internal class ObjectInfo
    {
        private static readonly BindingFlags _bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        internal ObjectInfo(object obj)
        {
            this.Object = GetObject(obj);
            this.Type = Object.GetType();
        }

        private object GetObject(object obj)
        {
            if (obj is PSObject)
                return ((PSObject)obj).BaseObject;

            return obj;
        }

        internal Object Object { get; private set; }
        internal Type Type { get; private set; }

        internal MemberInfo GetMember(string name, bool @static)
        {
            Type type = GetType(@static);

            // TODO: Single() is a problem for overloaded methods
            return type.GetMember(name, _bindingFlags).Single();
        }

        private Type GetType(bool @static)
        {
            if (@static)
            {
                return (Type)Object;
            }
            return Type;
        }

        internal MethodInfo GetMethod(string name, IEnumerable<object> arguments, bool @static)
        {
            Type type = GetType(@static);
            return type.GetMethod(name, _bindingFlags, null, arguments.Select(a => a.GetType()).ToArray(), null);
        }
    }
}
