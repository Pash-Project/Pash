// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Language;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    [Serializable]
    public class ScriptBlock// : ISerializable
    {
        private ScriptBlockAst _scriptBlockAst;

        internal ScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            _scriptBlockAst = scriptBlockAst;
        }

        //protected ScriptBlock(SerializationInfo info, StreamingContext context);

        public Ast Ast { get { return _scriptBlockAst; } }
        //public List<Attribute> Attributes { get; }
        //public string File { get; }
        //public bool IsFilter { get; set; }
        //public PSModuleInfo Module { get; }
        //public PSToken StartPosition { get; }

        //public void CheckRestrictedLanguage(IEnumerable<string> allowedCommands, IEnumerable<string> allowedVariables, bool allowEnvironmentVariables);
        //public static ScriptBlock Create(string script);
        //public ScriptBlock GetNewClosure();
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context);
        //public PowerShell GetPowerShell(params object[] args);
        //public PowerShell GetPowerShell(Dictionary<string, object> variables, params object[] args);
        //public PowerShell GetPowerShell(Dictionary<string, object> variables, out Dictionary<string, object> usingVariables, params object[] args);
        //public SteppablePipeline GetSteppablePipeline();
        //public SteppablePipeline GetSteppablePipeline(CommandOrigin commandOrigin);

        //public

        public Collection<PSObject> Invoke(params object[] args)
        {
            var pipeline = Runspace.DefaultRunspace.CreateNestedPipeline();
            pipeline.Commands.Add(new Command(_scriptBlockAst));
            return pipeline.Invoke(args);
        }

        //public object InvokeReturnAsIs(params object[] args);
        public override string ToString() { return this.Ast.ToString(); }
    }
}
