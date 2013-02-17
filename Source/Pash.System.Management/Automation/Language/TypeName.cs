// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace System.Management.Automation.Language
{
    public class TypeName : ITypeName
    {
        private static readonly Dictionary<string, Type> TypeAccelerators = new Dictionary<string, Type>
        {
            { "int", typeof(int) },
            { "long", typeof(long) },
            { "string", typeof(string) },
            { "char", typeof(char) },
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            { "float", typeof(float) },
            { "single", typeof(float) },
            { "regex", typeof(Text.RegularExpressions.Regex) },
            { "array", typeof(Array) },
            { "xml", typeof(Xml.XmlDocument) },
            { "scriptblock", typeof(ScriptBlock) },
            { "switch", typeof(SwitchParameter)  },
            { "hashtable", typeof(Collections.Hashtable) },
            { "type", typeof(Type) },
            { "ipaddress", typeof(Net.IPAddress) }
            // TODO: Next accelerators seems to be PowerShell and Windows-specific. Sort them out.
            //ref System.Management.Automation.PSReference
            //psobject	System.Management.Automation.PSObject
            //pscustomobject	System.Management.Automation.PSObject
            //psmoduleinfo	System.Management.Automation.PSModuleInfo
            //powershell	System.Management.Automation.PowerShell
            //runspacefactory	System.Management.Automation.Runspaces.RunspaceFactory
            //runspace	System.Management.Automation.Runspaces.Runspace
            //wmi	System.Management.ManagementObject
            //wmisearcher	System.Management.ManagementObjectSearcher
            //wmiclass	System.Management.ManagementClass
            //adsi	System.DirectoryServices.DirectoryEntry
            //adsisearcher	System.DirectoryServices.DirectorySearcher
            //accelerators	System.Management.Automation.TypeAccelerators
        };

        readonly Type Type;

        public TypeName(Type type)
        {
            this.Type = type;
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
            // We act correspondingly to the notes in §3.9 of PowerShell Language Specification.
            Type type;
            if (TypeAccelerators.TryGetValue(Name, out type))
            {
                return type;
            }

            // TODO: Try parse type name with "System." prefix.
            // TODO: Parse generic types.

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", this.Name);
        }
    }
}
