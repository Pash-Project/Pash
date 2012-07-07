using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    public class CommandInvocationIntrinsics
    {
        public string ExpandString(string source) { throw new NotImplementedException(); }
        public Collection<PSObject> InvokeScript(string script) { throw new NotImplementedException(); }
        public Collection<PSObject> InvokeScript(string script, bool useNewScope, PipelineResultTypes writeToPipeline, IList input, params object[] args) { throw new NotImplementedException(); }
        public ScriptBlock NewScriptBlock(string scriptText) { throw new NotImplementedException(); }

        // internals
        //internal CommandInvocationIntrinsics(ExecutionContext context) { throw new NotImplementedException(); }
        //internal CommandInvocationIntrinsics(ExecutionContext context, PSCmdlet cmdlet) { throw new NotImplementedException(); }
    }
}
