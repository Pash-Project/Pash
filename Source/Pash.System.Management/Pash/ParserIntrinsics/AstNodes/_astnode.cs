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
    public abstract class _astnode
    {
        protected readonly AstContext astContext;
        protected readonly ParseTreeNode parseTreeNode;

        public _astnode(AstContext astContext, ParseTreeNode parseTreeNode)
        {
            this.astContext = astContext;
            this.parseTreeNode = parseTreeNode;

            ChildAstNodes = new ReadOnlyCollection<_astnode>(this.parseTreeNode.ChildNodes.Select(childNode => childNode.AstNode).Cast<_astnode>().ToList());
        }

        protected PowerShellGrammar Grammar { get { return (PowerShellGrammar)this.astContext.Language.Grammar; } }

        protected ReadOnlyCollection<_astnode> ChildAstNodes;

        [DebuggerStepThrough]
        internal T Cast<T>() where T : _astnode { return (T)this; }

        protected string Text { get { return parseTreeNode.FindTokenAndGetText(); } }

        public override string ToString()
        {
            return "'{0}' ({1})".FormatString(this.Text, this.GetType().Name);
        }
    }
}
