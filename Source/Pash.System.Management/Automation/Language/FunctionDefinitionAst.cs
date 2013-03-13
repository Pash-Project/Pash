using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class FunctionDefinitionAst : StatementAst
    {
        public FunctionDefinitionAst(IScriptExtent extent, bool isFilter, bool isWorkflow, string name, IEnumerable<ParameterAst> parameters, ScriptBlockAst body)
            : base(extent)
        {
            this.IsFilter = isFilter;
            this.IsWorkflow = isWorkflow;
            this.Name = name;
            this.Parameters = parameters.ToReadOnlyCollection();
            this.Body = body;
        }

        public ScriptBlockAst Body { get; private set; }
        public bool IsFilter { get; private set; }
        public bool IsWorkflow { get; private set; }
        public string Name { get; private set; }
        public ReadOnlyCollection<ParameterAst> Parameters { get; private set; }

        //public CommentHelpInfo GetHelpContent();

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Body;
                foreach (var item in this.Parameters) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("function {0} {{ ... }}", this.Name);
        }
    }
}
