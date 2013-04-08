using System;
namespace System.Management.Automation.Runspaces
{
    public sealed class AssemblyConfigurationEntry : RunspaceConfigurationEntry
    {
        private string filename;

        public string FileName
        {
            get
            {
                return this.filename;
            }
        }

        public AssemblyConfigurationEntry(string name, string fileName)
            : base(name)
        {
            this.filename = fileName;
        }
    }
}
