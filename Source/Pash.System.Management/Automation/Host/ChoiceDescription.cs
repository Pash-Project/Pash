using System;

namespace System.Management.Automation.Host
{
    public sealed class ChoiceDescription
    {
        public ChoiceDescription(string label) { throw new NotImplementedException(); }
        public ChoiceDescription(string label, string helpMessage) { throw new NotImplementedException(); }

        public string HelpMessage { get; set; }
        public string Label { get; private set; }

        // internals
        //internal ChoiceDescription(string resStringBaseName, string labelResourceId, string helpResourceId);
    }
}
