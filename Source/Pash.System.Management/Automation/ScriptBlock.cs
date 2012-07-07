using System;
using System.Collections;
using System.Collections.ObjectModel;
using Pash.Implementation;

namespace System.Management.Automation
{
    public class ScriptBlock
    {
        private string _script;
        private ExecutionContext _scope;

        internal ScriptBlock(string script)
        {
            _script = script;
        }

        internal ScriptBlock(ExecutionContext scope, string script)
        {
            _script = script;
            _scope = scope;
        }

        public bool IsFilter { get; set; }

        public Collection<PSObject> Invoke(params object[] args) { throw new NotImplementedException(); }
        public object InvokeReturnAsIs(params object[] args) { throw new NotImplementedException(); }
        public override string ToString() { return _script; }

        // internals
        //internal System.Delegate GetDelegate(System.Type delegateType);
        //internal Collection<PSObject> Invoke(object dollarUnder, object input, params object[] args);
        //internal object InvokeRaw(object dollarUnder, object input, params object[] args);
        //internal object InvokeUsingCmdlet(System.Management.Automation.Cmdlet contextCmdlet, bool UseLocalScope, bool writeErrors, object dollarUnder, object input, object scriptThis, params object[] args);
        //internal void InvokeWithPipe(bool UseLocalScope, bool writeErrors, object dollarUnder, object input, object scriptThis, System.Management.Automation.Internal.Pipe outputPipe, ref System.Collections.ArrayList resultList, params object[] args);
        //internal ScriptBlock(ExecutionContext context, Token scriptBlockToken, ParseTreeNode begin, ParseTreeNode process, ParseTreeNode end);
        //internal ScriptBlock(ExecutionContext context, Token scriptBlockToken, ParseTreeNode ptn);
        //internal ScriptBlock(ExecutionContext context, ParseTreeNode begin, ParseTreeNode process, ParseTreeNode end);
        //internal ScriptBlock(ExecutionContext context, ParseTreeNode ptn);
        //internal ParameterMetadata ParameterMetadata { get; }
        //internal RuntimeDefinedParameterDictionary RuntimeDefinedParameters { get; }
        //internal ParameterDeclarationNode arguments;
        //internal ParseTreeNode begin;
        internal string DefiningFile;
        //internal System.Management.Automation.ParseTreeNode end;
        internal string FunctionName;
        //internal System.Management.Automation.ParseTreeNode process;
    }
}
