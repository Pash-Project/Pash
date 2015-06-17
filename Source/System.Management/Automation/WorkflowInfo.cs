// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    public class WorkflowInfo : FunctionInfo
    {
        public WorkflowInfo(
            string name,
            string definition,
            ScriptBlock workflow,
            string xamlDefinition,
            WorkflowInfo[] workflowsCalled)
            : base(name, workflow, null)
        {
        }

        public WorkflowInfo(
            string name,
            string definition,
            ScriptBlock workflow,
            string xamlDefinition,
            WorkflowInfo[] workflowsCalled,
            PSModuleInfo module)
            : base (name, workflow, null)
        {
        }
    }
}
