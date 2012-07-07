using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;
using System.Globalization;
using System.Management.Automation;

namespace Pash.Implementation
{
    public class LocalHost : PSHost
    {
        private CultureInfo originalCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
        private CultureInfo originalUICultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return originalCultureInfo; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return originalUICultureInfo; }
        }

        public override void EnterNestedPrompt()
        {
            // $host.EnterNestedPrompt()
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        Guid instanceId = Guid.NewGuid();

        public override Guid InstanceId
        {
            get { return instanceId; }
        }

        public override string Name
        {
            get { return "LocalHost"; }
        }

        public override void NotifyBeginApplication()
        {
            return;  // Do nothing...
        }

        public override void NotifyEndApplication()
        {
            return;  // Do nothing...
        }

        public int ExitCode { get; private set; }
        public bool ShouldExit { get; private set; }

        public override void SetShouldExit(int exitCode)
        {
            ExitCode = exitCode;
            ShouldExit = true;
        }

        public override PSHostUserInterface UI
        {
            get { return new LocalHostUserInterface(); }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        // TODO: what options are needed?
        object Options = new object();

        public override PSObject PrivateData
        {
            get
            {
                return PSObject.AsPSObject(Options);
            }
        }
    }
}
