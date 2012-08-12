using System;
using GoldParser;
using System.Collections.Generic;

namespace Pash.ParserIntrinsics
{
    /// <summary>
    /// Derive this class for NonTerminal AST Nodes
    /// </summary>
    public abstract partial class NonTerminalNode : ASTNode
    {
        public Rule Rule { get; private set; }
        public int ReductionNumber { get; set; }

        private List<ASTNode> m_array = new List<ASTNode>();

        public NonTerminalNode(Parser theParser)
        {
            if (theParser != null)
                Rule = theParser.ReductionRule;
        }

        public int Count
        {
            get { return m_array.Count; }
        }

        public ASTNode this[int index]
        {
            get { return m_array[index]; }
        }

        public void AppendChildNode(ASTNode node)
        {
            if (node == null)
            {
                return;
            }
            m_array.Add(node);
        }
    }
}

