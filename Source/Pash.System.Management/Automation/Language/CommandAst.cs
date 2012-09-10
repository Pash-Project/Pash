using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Extensions.Enumerable;

namespace System.Management.Automation.Language
{
    public class CommandAst : CommandBaseAst
    {
        public CommandAst(IScriptExtent extent, IEnumerable<CommandElementAst> commandElements, TokenKind invocationOperator, IEnumerable<RedirectionAst> redirections)
            : base(extent, redirections)
        {
            this.CommandElements = commandElements.ToReadOnlyCollection();
            this.InvocationOperator = invocationOperator;
        }

        public ReadOnlyCollection<CommandElementAst> CommandElements { get; private set; }
        public TokenKind InvocationOperator { get; private set; }

        public string GetCommandName()
        {
            var firstCommandElement = this.CommandElements.First();

            if (firstCommandElement is StringConstantExpressionAst)
            {
                return ((StringConstantExpressionAst)firstCommandElement).Value;
            }

            throw new NotImplementedException(this.ToString());
        }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.CommandElements) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }

    }
}
