using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace System.Management.Automation.Language
{
    public class TypeName : ITypeName
    {
//        readonly Type Type;

        public TypeName(Type type)
        {
//            this.Type = type;
        }

        public TypeName(string name)
        {
            this.Name = name;
        }

        public string AssemblyName
        {
            get;
            private set;
        }

        public IScriptExtent Extent
        {
            get;
            private set;
        }

        public string FullName
        {
            get;
            private set;
        }

        public bool IsArray
        {
            get;
            private set;
        }

        public bool IsGeneric
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public Type GetReflectionAttributeType()
        {
            throw new NotImplementedException();
        }

        public Type GetReflectionType()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", this.Name);
        }
    }
}
