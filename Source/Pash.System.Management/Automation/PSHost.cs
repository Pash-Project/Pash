using System;
using System.Globalization;
using System.Management.Automation;

namespace System.Management.Automation.Host
{
    public abstract class PSHost
    {
        protected PSHost() { }

        public abstract CultureInfo CurrentCulture { get; }
        public abstract CultureInfo CurrentUICulture { get; }
        public abstract Guid InstanceId { get; }
        public abstract string Name { get; }
        public virtual PSObject PrivateData { get; private set; }
        public abstract PSHostUserInterface UI { get; }
        public abstract Version Version { get; }

        public abstract void EnterNestedPrompt();
        public abstract void ExitNestedPrompt();
        public abstract void NotifyBeginApplication();
        public abstract void NotifyEndApplication();
        public abstract void SetShouldExit(int exitCode);
    }
}
