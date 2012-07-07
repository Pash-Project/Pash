using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using GoldParser;
using Pash.ParserIntrinsics;

namespace Pash.ParserIntrinsics
{
    public partial class PashParser
    {
        #region Static Initialization
        private static object SyncRoot = new object();
        private static ParserFactory _parserFactory;

        public static PashParser Initialize()
        {
            return new PashParser(ParserFactory);
        }

        private static ParserFactory ParserFactory
        {
            get
            {
                if (_parserFactory == null)
                {
                    lock (SyncRoot)
                    {
                        if (_parserFactory == null)
                        {
                            _parserFactory = new ParserFactory();

                            // TODO: make sure to initialize from an XML
                            // TODO: don't use strings - use constants
                            _parserFactory.InitializeFactoryFromResource("System.Management.Automation.Pash.ParserIntrinsics.PashGrammar.cgt");
                        }
                    }
                }

                return _parserFactory;
            }
        }
        #endregion

        internal PipelineNode NormilizeToPipeline()
        {
            if (ErrorString != null)
                return null;

            if (SyntaxTree == null)
                return PipelineNode.GetPipeline(m_parser);

            ASTNode rootNode = SyntaxTree;

            if (rootNode is PipelineNode)
                return rootNode as PipelineNode;

            PipelineNode pipeline = PipelineNode.GetPipeline(null);

            if (rootNode is AssignmentNode)
            {
                pipeline.AddItem(rootNode);
            }
            else
            {
                CmdletNode cmdlet = rootNode as CmdletNode;

                if (cmdlet == null)
                {
                    cmdlet = new CmdletNode(null, rootNode as AnyWordNode);
                }

                pipeline.AddItem(cmdlet);
            }
            
            return pipeline;

            //parser.SyntaxTree.Execute(runtime);
        }
    }

    public partial class PashParserContext
    {
        // <statementSeparatorToken> ::= NewLine <statementSeparatorToken>
        partial void CreateRule_Rule_Statementseparatortoken_Newline(Parser theParser, ASTNodeContainer astNode)
        {
        }

        // <statementSeparatorToken> ::= 
        partial void CreateRule_Rule_Statementseparatortoken(Parser theParser, ASTNodeContainer astNode)
        {
        }

        // <statementListRule> ::= <statementRule>
        partial void CreateRule_Rule_Statementlistrule(Parser theParser, ASTNodeContainer astNode)
        {
        }

        // <statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
        partial void CreateRule_Rule_Statementlistrule2(Parser theParser, ASTNodeContainer astNode)
        {
        }

        // <statementRule> ::= <pipelineRule>
        partial void CreateRule_Rule_Statementrule(Parser theParser, ASTNodeContainer astNode)
        {
        }

        // <statementRule> ::= CommentToken
        partial void CreateRule_Rule_Statementrule_Commenttoken(Parser theParser, ASTNodeContainer astNode)
        {
        }

        // <pipelineRule> ::= <cmdletCall>
        partial void CreateRule_Rule_Pipelinerule(Parser theParser, ASTNodeContainer astNode)
	    {
	    }

        // <pipelineRule> ::= <cmdletCall> | <pipelineRule>
        partial void CreateRule_Rule_Pipelinerule_Pipe(Parser theParser, ASTNodeContainer astNode)
		{
            astNode.Node = PipelineNode.GetPipeline(theParser);
	    }

        // <pipelineRule> ::= <assignmentStatementRule>
        partial void CreateRule_Rule_Pipelinerule2(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <pipelineRule> ::= <assignmentStatementRule> | <pipelineRule>
        partial void CreateRule_Rule_Pipelinerule_Pipe2(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
        partial void CreateRule_Rule_Assignmentstatementrule_Assignmentoperatortoken(Parser theParser, ASTNodeContainer astNode)
		{
            astNode.Node = new AssignmentNode(theParser);
		}

        // <lvalueExpression> ::= <lvalue>
        partial void CreateRule_Rule_Lvalueexpression(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <lvalue> ::= <simpleLvalue>
        partial void CreateRule_Rule_Lvalue(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <simpleLvalue> ::= VariableToken
        partial void CreateRule_Rule_Simplelvalue_Variabletoken(Parser theParser, ASTNodeContainer astNode)
		{
            astNode.Node = new VariableNode(theParser);
		}

        // <ParameterArgumentToken> ::= <valueRule>
        partial void CreateRule_Rule_Parameterargumenttoken(Parser theParser, ASTNodeContainer astNode)
		{
            astNode.Node = ParamsListNode.GetParamsList(theParser);
		}

        // <ParameterArgumentToken> ::= AnyWordToken
        partial void CreateRule_Rule_Parameterargumenttoken_Anywordtoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new AnyWordNode(theParser);
        }

        // <ParameterArgumentToken> ::= ParameterToken
        partial void CreateRule_Rule_Parameterargumenttoken_Parametertoken(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
        partial void CreateRule_Rule_Cmletparamslist(Parser theParser, ASTNodeContainer astNode)
		{
            astNode.Node = ParamsListNode.GetParamsList(theParser);
		}

        // <cmletParamsList> ::= <ParameterArgumentToken>
        partial void CreateRule_Rule_Cmletparamslist2(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <cmdletName> ::= AnyWordToken
        partial void CreateRule_Rule_Cmdletname_Anywordtoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new AnyWordNode(theParser);
        }

        // <cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
        partial void CreateRule_Rule_Cmdletcall_Execcall(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <cmdletCall> ::= ExecCall <cmdletName>
        partial void CreateRule_Rule_Cmdletcall_Execcall2(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <cmdletCall> ::= <cmdletName> <cmletParamsList>
        partial void CreateRule_Rule_Cmdletcall(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new CmdletNode(theParser);
        }

        // <cmdletCall> ::= <cmdletName>
        partial void CreateRule_Rule_Cmdletcall2(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <cmdletCall> ::= <expressionRule>
        partial void CreateRule_Rule_Cmdletcall3(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <expressionRule> ::= <logicalExpressionRule>
        partial void CreateRule_Rule_Expressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <logicalExpressionRule> ::= <bitwiseExpressionRule>
        partial void CreateRule_Rule_Logicalexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <bitwiseExpressionRule> ::= <comparisonExpressionRule>
        partial void CreateRule_Rule_Bitwiseexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <comparisonExpressionRule> ::= <addExpressionRule>
        partial void CreateRule_Rule_Comparisonexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <addExpressionRule> ::= <multiplyExpressionRule>
        partial void CreateRule_Rule_Addexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
        partial void CreateRule_Rule_Addexpressionrule_Additionoperatortoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new AdditionExpressionNode(theParser);
        }

        // <multiplyExpressionRule> ::= <formatExpressionRule>
        partial void CreateRule_Rule_Multiplyexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <formatExpressionRule> ::= <rangeExpressionRule>
        partial void CreateRule_Rule_Formatexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <rangeExpressionRule> ::= <arrayLiteralRule>
        partial void CreateRule_Rule_Rangeexpressionrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
        partial void CreateRule_Rule_Rangeexpressionrule_Rangeoperatortoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new RangeNode(theParser);
        }

        // <arrayLiteralRule> ::= <postfixOperatorRule>
        partial void CreateRule_Rule_Arrayliteralrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
        partial void CreateRule_Rule_Arrayliteralrule_Commatoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new ArrayNode(theParser);
        }

        // <postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
        partial void CreateRule_Rule_Postfixoperatorrule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <propertyOrArrayReferenceRule> ::= <valueRule>
        partial void CreateRule_Rule_Propertyorarrayreferencerule(Parser theParser, ASTNodeContainer astNode)
		{
		}

        // <valueRule> ::= StringToken
        partial void CreateRule_Rule_Valuerule_Stringtoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new StringNode(theParser);
        }

        // <valueRule> ::= VariableToken
        partial void CreateRule_Rule_Valuerule_Variabletoken(Parser theParser, ASTNodeContainer astNode)
        {
            astNode.Node = new VariableNode(theParser);
        }

        // <valueRule> ::= NumberToken
        partial void CreateRule_Rule_Valuerule_Numbertoken(Parser theParser, ASTNodeContainer astNode)
		{
            astNode.Node = new NumberNode(theParser);
		}

        // <valueRule> ::= $( <statementRule> )
        partial void CreateRule_Rule_Valuerule_Dollarlparan_Rparan(Parser theParser, ASTNodeContainer astNode)
        {
            // Value-of
            astNode.Node = new ValueOfNode(theParser);
        }

        // <valueRule> ::= ( <assignmentStatementRule> )
        partial void CreateRule_Rule_Valuerule_Lparan_Rparan(Parser theParser, ASTNodeContainer astNode)
        {
        }
    }
}
