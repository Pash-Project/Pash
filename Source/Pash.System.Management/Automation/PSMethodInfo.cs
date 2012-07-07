using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public abstract class PSMethodInfo : PSMemberInfo
    {
        protected PSMethodInfo()
        {
        }

        public override sealed object Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new Exception("Can't change Method Info");
            }
        }

        public abstract Collection<string> OverloadDefinitions { get; }
        public abstract object Invoke(params object[] arguments);
    }
}
