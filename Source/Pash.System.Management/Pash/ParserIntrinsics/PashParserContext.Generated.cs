using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GoldParser;
using Pash.Implementation;
using System.Linq;

namespace Pash.ParserIntrinsics
{
    public partial class PashParserContext
    {
        private class ASTNodeContainer
        {
            public ASTNode Node = null;
        }

        // instance fields
        private Parser m_parser;

        //private TextReader m_inputReader;

        // constructor
        public PashParserContext(Parser parser)
        {
            m_parser = parser;
        }

        private ASTNode Node(int index)
        {
            return (ASTNode)m_parser.GetReductionSyntaxNode(index);
        }

        private object Token(int index)
        {
            return m_parser.GetReductionSyntaxNode(index);
        }

        public string GetTokenText()
        {
            if (m_parser.TokenSymbol.SymbolType == SymbolType.Terminal) 
                return m_parser.TokenString;
            else
                return null;
        }

        public ASTNode CreateASTNode()
        {
            var astNode = new ASTNodeContainer();

            System.Diagnostics.Debug.WriteLine(Enum.GetName(typeof(RuleConstants), m_parser.ReductionRule.Index));

            switch ((RuleConstants)m_parser.ReductionRule.Index)
            {
                case RuleConstants.Rule_Statementseparatortoken_Newline:
                    // <statementSeparatorToken> ::= NewLine <statementSeparatorToken>
                    CreateRule_Rule_Statementseparatortoken_Newline(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Statementseparatortoken:
                    // <statementSeparatorToken> ::= 
                    CreateRule_Rule_Statementseparatortoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Statementlistrule:
                    // <statementListRule> ::= <statementRule>
                    CreateRule_Rule_Statementlistrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Statementlistrule2:
                    // <statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
                    CreateRule_Rule_Statementlistrule2(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Statementrule:
                    // <statementRule> ::= <pipelineRule>
                    CreateRule_Rule_Statementrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Statementrule_Commenttoken:
                    // <statementRule> ::= CommentToken
                    CreateRule_Rule_Statementrule_Commenttoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Pipelinerule:
                    // <pipelineRule> ::= <cmdletCall>
                    CreateRule_Rule_Pipelinerule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Pipelinerule_Pipe:
                    // <pipelineRule> ::= <cmdletCall> | <pipelineRule>
                    CreateRule_Rule_Pipelinerule_Pipe(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Pipelinerule2:
                    // <pipelineRule> ::= <assignmentStatementRule>
                    CreateRule_Rule_Pipelinerule2(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Pipelinerule_Pipe2:
                    // <pipelineRule> ::= <assignmentStatementRule> | <pipelineRule>
                    CreateRule_Rule_Pipelinerule_Pipe2(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Assignmentstatementrule_Assignmentoperatortoken:
                    // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
                    CreateRule_Rule_Assignmentstatementrule_Assignmentoperatortoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Lvalueexpression:
                    // <lvalueExpression> ::= <lvalue>
                    CreateRule_Rule_Lvalueexpression(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Lvalue:
                    // <lvalue> ::= <simpleLvalue>
                    CreateRule_Rule_Lvalue(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Simplelvalue_Variabletoken:
                    // <simpleLvalue> ::= VariableToken
                    CreateRule_Rule_Simplelvalue_Variabletoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Parameterargumenttoken:
                    // <ParameterArgumentToken> ::= <valueRule>
                    CreateRule_Rule_Parameterargumenttoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Parameterargumenttoken_Anywordtoken:
                    // <ParameterArgumentToken> ::= AnyWordToken
                    CreateRule_Rule_Parameterargumenttoken_Anywordtoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Parameterargumenttoken_Parametertoken:
                    // <ParameterArgumentToken> ::= ParameterToken
                    CreateRule_Rule_Parameterargumenttoken_Parametertoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmletparamslist:
                    // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
                    CreateRule_Rule_Cmletparamslist(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmletparamslist2:
                    // <cmletParamsList> ::= <ParameterArgumentToken>
                    CreateRule_Rule_Cmletparamslist2(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmdletname_Anywordtoken:
                    // <cmdletName> ::= AnyWordToken
                    CreateRule_Rule_Cmdletname_Anywordtoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmdletcall_Execcall:
                    // <cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
                    CreateRule_Rule_Cmdletcall_Execcall(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmdletcall_Execcall2:
                    // <cmdletCall> ::= ExecCall <cmdletName>
                    CreateRule_Rule_Cmdletcall_Execcall2(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmdletcall:
                    // <cmdletCall> ::= <cmdletName> <cmletParamsList>
                    CreateRule_Rule_Cmdletcall(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmdletcall2:
                    // <cmdletCall> ::= <cmdletName>
                    CreateRule_Rule_Cmdletcall2(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Cmdletcall3:
                    // <cmdletCall> ::= <expressionRule>
                    CreateRule_Rule_Cmdletcall3(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Expressionrule:
                    // <expressionRule> ::= <logicalExpressionRule>
                    CreateRule_Rule_Expressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Logicalexpressionrule:
                    // <logicalExpressionRule> ::= <bitwiseExpressionRule>
                    CreateRule_Rule_Logicalexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Bitwiseexpressionrule:
                    // <bitwiseExpressionRule> ::= <comparisonExpressionRule>
                    CreateRule_Rule_Bitwiseexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Comparisonexpressionrule:
                    // <comparisonExpressionRule> ::= <addExpressionRule>
                    CreateRule_Rule_Comparisonexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Addexpressionrule:
                    // <addExpressionRule> ::= <multiplyExpressionRule>
                    CreateRule_Rule_Addexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Addexpressionrule_Additionoperatortoken:
                    // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
                    CreateRule_Rule_Addexpressionrule_Additionoperatortoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Multiplyexpressionrule:
                    // <multiplyExpressionRule> ::= <formatExpressionRule>
                    CreateRule_Rule_Multiplyexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Formatexpressionrule:
                    // <formatExpressionRule> ::= <rangeExpressionRule>
                    CreateRule_Rule_Formatexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Rangeexpressionrule:
                    // <rangeExpressionRule> ::= <arrayLiteralRule>
                    CreateRule_Rule_Rangeexpressionrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Rangeexpressionrule_Rangeoperatortoken:
                    // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
                    CreateRule_Rule_Rangeexpressionrule_Rangeoperatortoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Arrayliteralrule:
                    // <arrayLiteralRule> ::= <postfixOperatorRule>
                    CreateRule_Rule_Arrayliteralrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Arrayliteralrule_Commatoken:
                    // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
                    CreateRule_Rule_Arrayliteralrule_Commatoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Postfixoperatorrule:
                    // <postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
                    CreateRule_Rule_Postfixoperatorrule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Propertyorarrayreferencerule:
                    // <propertyOrArrayReferenceRule> ::= <valueRule>
                    CreateRule_Rule_Propertyorarrayreferencerule(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Valuerule_Stringtoken:
                    // <valueRule> ::= StringToken
                    CreateRule_Rule_Valuerule_Stringtoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Valuerule_Variabletoken:
                    // <valueRule> ::= VariableToken
                    CreateRule_Rule_Valuerule_Variabletoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Valuerule_Numbertoken:
                    // <valueRule> ::= NumberToken
                    CreateRule_Rule_Valuerule_Numbertoken(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Valuerule_Dollarlparan_Rparan:
                    // <valueRule> ::= $( <statementRule> )
                    CreateRule_Rule_Valuerule_Dollarlparan_Rparan(m_parser, astNode);
                    break;

                case RuleConstants.Rule_Valuerule_Lparan_Rparan:
                    // <valueRule> ::= ( <assignmentStatementRule> )
                    CreateRule_Rule_Valuerule_Lparan_Rparan(m_parser, astNode);
                    break;

                default:
                    throw new RuleException("Unknown rule: Does your CGT Match your Code Revision?");
            }
            return astNode.Node;
        }

        #region Rules
        // <statementSeparatorToken> ::= NewLine <statementSeparatorToken>
        partial void CreateRule_Rule_Statementseparatortoken_Newline(Parser theParser, ASTNodeContainer astNode);

        // <statementSeparatorToken> ::= 
        partial void CreateRule_Rule_Statementseparatortoken(Parser theParser, ASTNodeContainer astNode);

        // <statementListRule> ::= <statementRule>
        partial void CreateRule_Rule_Statementlistrule(Parser theParser, ASTNodeContainer astNode);

        // <statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
        partial void CreateRule_Rule_Statementlistrule2(Parser theParser, ASTNodeContainer astNode);

        // <statementRule> ::= <pipelineRule>
        partial void CreateRule_Rule_Statementrule(Parser theParser, ASTNodeContainer astNode);

        // <statementRule> ::= CommentToken
        partial void CreateRule_Rule_Statementrule_Commenttoken(Parser theParser, ASTNodeContainer astNode);

        // <pipelineRule> ::= <cmdletCall>
        partial void CreateRule_Rule_Pipelinerule(Parser theParser, ASTNodeContainer astNode);

        // <pipelineRule> ::= <cmdletCall> | <pipelineRule>
        partial void CreateRule_Rule_Pipelinerule_Pipe(Parser theParser, ASTNodeContainer astNode);

        // <pipelineRule> ::= <assignmentStatementRule>
        partial void CreateRule_Rule_Pipelinerule2(Parser theParser, ASTNodeContainer astNode);

        // <pipelineRule> ::= <assignmentStatementRule> | <pipelineRule>
        partial void CreateRule_Rule_Pipelinerule_Pipe2(Parser theParser, ASTNodeContainer astNode);

        // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
        partial void CreateRule_Rule_Assignmentstatementrule_Assignmentoperatortoken(Parser theParser, ASTNodeContainer astNode);

        // <lvalueExpression> ::= <lvalue>
        partial void CreateRule_Rule_Lvalueexpression(Parser theParser, ASTNodeContainer astNode);

        // <lvalue> ::= <simpleLvalue>
        partial void CreateRule_Rule_Lvalue(Parser theParser, ASTNodeContainer astNode);

        // <simpleLvalue> ::= VariableToken
        partial void CreateRule_Rule_Simplelvalue_Variabletoken(Parser theParser, ASTNodeContainer astNode);

        // <ParameterArgumentToken> ::= <valueRule>
        partial void CreateRule_Rule_Parameterargumenttoken(Parser theParser, ASTNodeContainer astNode);

        // <ParameterArgumentToken> ::= AnyWordToken
        partial void CreateRule_Rule_Parameterargumenttoken_Anywordtoken(Parser theParser, ASTNodeContainer astNode);

        // <ParameterArgumentToken> ::= ParameterToken
        partial void CreateRule_Rule_Parameterargumenttoken_Parametertoken(Parser theParser, ASTNodeContainer astNode);

        // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
        partial void CreateRule_Rule_Cmletparamslist(Parser theParser, ASTNodeContainer astNode);

        // <cmletParamsList> ::= <ParameterArgumentToken>
        partial void CreateRule_Rule_Cmletparamslist2(Parser theParser, ASTNodeContainer astNode);

        // <cmdletName> ::= AnyWordToken
        partial void CreateRule_Rule_Cmdletname_Anywordtoken(Parser theParser, ASTNodeContainer astNode);

        // <cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
        partial void CreateRule_Rule_Cmdletcall_Execcall(Parser theParser, ASTNodeContainer astNode);

        // <cmdletCall> ::= ExecCall <cmdletName>
        partial void CreateRule_Rule_Cmdletcall_Execcall2(Parser theParser, ASTNodeContainer astNode);

        // <cmdletCall> ::= <cmdletName> <cmletParamsList>
        partial void CreateRule_Rule_Cmdletcall(Parser theParser, ASTNodeContainer astNode);

        // <cmdletCall> ::= <cmdletName>
        partial void CreateRule_Rule_Cmdletcall2(Parser theParser, ASTNodeContainer astNode);

        // <cmdletCall> ::= <expressionRule>
        partial void CreateRule_Rule_Cmdletcall3(Parser theParser, ASTNodeContainer astNode);

        // <expressionRule> ::= <logicalExpressionRule>
        partial void CreateRule_Rule_Expressionrule(Parser theParser, ASTNodeContainer astNode);

        // <logicalExpressionRule> ::= <bitwiseExpressionRule>
        partial void CreateRule_Rule_Logicalexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <bitwiseExpressionRule> ::= <comparisonExpressionRule>
        partial void CreateRule_Rule_Bitwiseexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <comparisonExpressionRule> ::= <addExpressionRule>
        partial void CreateRule_Rule_Comparisonexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <addExpressionRule> ::= <multiplyExpressionRule>
        partial void CreateRule_Rule_Addexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
        partial void CreateRule_Rule_Addexpressionrule_Additionoperatortoken(Parser theParser, ASTNodeContainer astNode);

        // <multiplyExpressionRule> ::= <formatExpressionRule>
        partial void CreateRule_Rule_Multiplyexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <formatExpressionRule> ::= <rangeExpressionRule>
        partial void CreateRule_Rule_Formatexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <rangeExpressionRule> ::= <arrayLiteralRule>
        partial void CreateRule_Rule_Rangeexpressionrule(Parser theParser, ASTNodeContainer astNode);

        // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
        partial void CreateRule_Rule_Rangeexpressionrule_Rangeoperatortoken(Parser theParser, ASTNodeContainer astNode);

        // <arrayLiteralRule> ::= <postfixOperatorRule>
        partial void CreateRule_Rule_Arrayliteralrule(Parser theParser, ASTNodeContainer astNode);

        // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
        partial void CreateRule_Rule_Arrayliteralrule_Commatoken(Parser theParser, ASTNodeContainer astNode);

        // <postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
        partial void CreateRule_Rule_Postfixoperatorrule(Parser theParser, ASTNodeContainer astNode);

        // <propertyOrArrayReferenceRule> ::= <valueRule>
        partial void CreateRule_Rule_Propertyorarrayreferencerule(Parser theParser, ASTNodeContainer astNode);

        // <valueRule> ::= StringToken
        partial void CreateRule_Rule_Valuerule_Stringtoken(Parser theParser, ASTNodeContainer astNode);

        // <valueRule> ::= VariableToken
        partial void CreateRule_Rule_Valuerule_Variabletoken(Parser theParser, ASTNodeContainer astNode);

        // <valueRule> ::= NumberToken
        partial void CreateRule_Rule_Valuerule_Numbertoken(Parser theParser, ASTNodeContainer astNode);

        // <valueRule> ::= $( <statementRule> )
        partial void CreateRule_Rule_Valuerule_Dollarlparan_Rparan(Parser theParser, ASTNodeContainer astNode);

        // <valueRule> ::= ( <assignmentStatementRule> )
        partial void CreateRule_Rule_Valuerule_Lparan_Rparan(Parser theParser, ASTNodeContainer astNode);

        #endregion
    }
}
