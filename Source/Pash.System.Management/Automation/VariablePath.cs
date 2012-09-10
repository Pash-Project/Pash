using System;

namespace System.Management.Automation
{
    public class VariablePath
    {
        public VariablePath(string path)
        {
            this.UserPath = path;
        }

        public string DriveName { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsDriveQualified { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsGlobal { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsLocal { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsPrivate { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsScript { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsUnqualified { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsUnscopedVariable { get { throw new NotImplementedException(this.ToString()); } }
        public bool IsVariable { get { throw new NotImplementedException(this.ToString()); } }
        public string UserPath { get; private set; }

        public override string ToString() { return this.UserPath; }
    }
}
