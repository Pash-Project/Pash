using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using GoldParser;
using Pash.ParserIntrinsics;

namespace Pash.ParserIntrinsics
{
    using Nodes;

    public partial class PashParserContext
    {
        // <pipelineRule> ::= <cmdletCall> | <pipelineRule>
        partial void CreateRule_Rule_Pipelinerule_Pipe(Parser theParser, ref ASTNode node)
        {
            node = PipelineNode.GetPipeline(theParser);
        }
        // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
        partial void CreateRule_Rule_Assignmentstatementrule_Assignmentoperatortoken(Parser theParser, ref ASTNode node)
        {
            node = new AssignmentNode(theParser);
        }

        // <simpleLvalue> ::= VariableToken
        partial void CreateRule_Rule_Simplelvalue_Variabletoken(Parser theParser, ref ASTNode node)
        {
            node = new VariableNode(theParser);
        }

        // <ParameterArgumentToken> ::= <valueRule>
        partial void CreateRule_Rule_Parameterargumenttoken(Parser theParser, ref ASTNode node)
        {
            node = ParamsListNode.GetParamsList(theParser);
        }

        // <ParameterArgumentToken> ::= AnyWordToken
        partial void CreateRule_Rule_Parameterargumenttoken_Anywordtoken(Parser theParser, ref ASTNode node)
        {
            node = new AnyWordNode(theParser);
        }

        // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
        partial void CreateRule_Rule_Cmletparamslist(Parser theParser, ref ASTNode node)
        {
            node = ParamsListNode.GetParamsList(theParser);
        }

        // <cmdletName> ::= AnyWordToken
        partial void CreateRule_Rule_Cmdletname_Anywordtoken(Parser theParser, ref ASTNode node)
        {
            node = new AnyWordNode(theParser);
        }

        // <cmdletCall> ::= <cmdletName> <cmletParamsList>
        partial void CreateRule_Rule_Cmdletcall(Parser theParser, ref ASTNode node)
        {
            node = new CmdletNode(theParser);
        }

        // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
        partial void CreateRule_Rule_Addexpressionrule_Additionoperatortoken(Parser theParser, ref ASTNode node)
        {
            node = new AdditionExpressionNode(theParser);
        }

        // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
        partial void CreateRule_Rule_Rangeexpressionrule_Rangeoperatortoken(Parser theParser, ref ASTNode node)
        {
            node = new RangeNode(theParser);
        }

        // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
        partial void CreateRule_Rule_Arrayliteralrule_Commatoken(Parser theParser, ref ASTNode node)
        {
            node = new ArrayNode(theParser);
        }

        // <valueRule> ::= StringToken
        partial void CreateRule_Rule_Valuerule_Stringtoken(Parser theParser, ref ASTNode node)
        {
            node = new StringNode(theParser);
        }

        // <valueRule> ::= VariableToken
        partial void CreateRule_Rule_Valuerule_Variabletoken(Parser theParser, ref ASTNode node)
        {
            node = new VariableNode(theParser);
        }

        // <valueRule> ::= NumberToken
        partial void CreateRule_Rule_Valuerule_Numbertoken(Parser theParser, ref ASTNode node)
        {
            node = new NumberNode(theParser);
        }

        // <valueRule> ::= $( <statementRule> )
        partial void CreateRule_Rule_Valuerule_Dollarlparan_Rparan(Parser theParser, ref ASTNode node)
        {
            // Value-of
            node = new ValueOfNode(theParser);
        }
    }
}
