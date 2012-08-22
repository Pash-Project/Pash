using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Pash.ParserIntrinsics.Nodes
{
    public class literal_node : _node
    {
        public literal_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue(context));

            // TODO: extract the value to the pipeline
        }

        internal override object GetValue(ExecutionContext context)
        {
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), PowerShellGrammar.Terminals.literal.Pattern);

            Debug.Assert(matches.Success);

            // TODO: throw if a NYI type of literal
            return matches.Groups[PowerShellGrammar.Terminals.verbatim_string_characters.Name].Value;
        }
    }
}
