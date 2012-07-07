using System;

namespace System.Management.Automation
{
    public class ScriptInfo : CommandInfo
    {
        public override string Definition { get { return ScriptBlock.ToString(); } }
        public ScriptBlock ScriptBlock { get; private set; }

        public override string ToString() { return Definition; }

        // internals
        internal ScriptInfo(string name, ScriptBlock script) : base(name, CommandTypes.Script)
        {
            ScriptBlock = script;
        }
    }
}
