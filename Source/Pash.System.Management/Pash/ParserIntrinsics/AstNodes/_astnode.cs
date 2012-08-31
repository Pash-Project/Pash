using System;
using System.Linq;
using Extensions.String;
using System.Management.Automation;
using Pash.Implementation;
using Irony.Ast;
using Irony.Parsing;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Pash.ParserIntrinsics.AstNodes
{
    // TODO: make it an interface, or add a field and remove this comment.
    public class _astnode
    {
        protected readonly AstContext astContext;
        protected readonly ParseTreeNode parseTreeNode;

        public _astnode(AstContext astContext, ParseTreeNode parseTreeNode)
        {
            this.astContext = astContext;
            this.parseTreeNode = parseTreeNode;

            ChildAstNodes = new ReadOnlyCollection<_astnode>(this.parseTreeNode.ChildNodes.Select(childNode => childNode.AstNode).Cast<_astnode>().ToList());
        }

        public PowerShellGrammar Grammar { get { return (PowerShellGrammar)this.astContext.Language.Grammar; } }

        public ReadOnlyCollection<_astnode> ChildAstNodes;

        [DebuggerStepThrough]
        public T As<T>() where T : _astnode { return (T)this; }

        public string Text { get { return parseTreeNode.Token.Text; } }
    }
}
