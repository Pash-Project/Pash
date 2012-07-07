using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class FunctionInfo : CommandInfo
    {
        public override string Definition { get { return Name; } }
        public ScopedItemOptions Options { get; set; }
        public ScriptBlock ScriptBlock { get; private set; }

        internal FunctionInfo(string name, ScriptBlock function)
            : this(name, function, ScopedItemOptions.None) { }

        internal FunctionInfo(string name, ScriptBlock function, ScopedItemOptions options)
            : base(name, CommandTypes.Function)
        {
            ScriptBlock = function;
            Options = options;
        }

        // internals
        //internal void SetScriptBlock(ScriptBlock function, bool force);
    }
}
