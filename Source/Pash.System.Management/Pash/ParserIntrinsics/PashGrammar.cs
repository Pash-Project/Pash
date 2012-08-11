
using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;

using GoldParser;

namespace Morozov.Parsing
{
		
    [Serializable()]
    public class SymbolException : System.Exception
    {
        public SymbolException(string message) : base(message)
        {
        }

        public SymbolException(string message,
            Exception inner) : base(message, inner)
        {
        }

        protected SymbolException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

    }

    [Serializable()]
    public class RuleException : System.Exception
    {

        public RuleException(string message) : base(message)
        {
        }

        public RuleException(string message,
                             Exception inner) : base(message, inner)
        {
        }

        protected RuleException(SerializationInfo info,
                                StreamingContext context) : base(info, context)
        {
        }

    }

    enum SymbolConstants : int
    {
        SYMBOL_EOF                          =  0, // (EOF)
        SYMBOL_ERROR                        =  1, // (Error)
        SYMBOL_COMMENT                      =  2, // Comment
        SYMBOL_WHITESPACE                   =  3, // Whitespace
        SYMBOL_NUM                          =  4, // '#'
        SYMBOL_TIMESDIV                     =  5, // '*/'
        SYMBOL_DIVTIMES                     =  6, // '/*'
        SYMBOL_DOLLARLPARAN                 =  7, // '$('
        SYMBOL_LPARAN                       =  8, // '('
        SYMBOL_RPARAN                       =  9, // ')'
        SYMBOL_ADDITIONOPERATORTOKEN        = 10, // AdditionOperatorToken
        SYMBOL_ANYWORDTOKEN                 = 11, // AnyWordToken
        SYMBOL_ASSIGNMENTOPERATORTOKEN      = 12, // AssignmentOperatorToken
        SYMBOL_COMMATOKEN                   = 13, // CommaToken
        SYMBOL_COMMENTTOKEN                 = 14, // CommentToken
        SYMBOL_EXECCALL                     = 15, // ExecCall
        SYMBOL_NEWLINE                      = 16, // NewLine
        SYMBOL_NUMBERTOKEN                  = 17, // NumberToken
        SYMBOL_PARAMETERTOKEN               = 18, // ParameterToken
        SYMBOL_RANGEOPERATORTOKEN           = 19, // RangeOperatorToken
        SYMBOL_STRINGTOKEN                  = 20, // StringToken
        SYMBOL_VARIABLETOKEN                = 21, // VariableToken
        SYMBOL_PIPE                         = 22, // '|'
        SYMBOL_ADDEXPRESSIONRULE            = 23, // <addExpressionRule>
        SYMBOL_ARRAYLITERALRULE             = 24, // <arrayLiteralRule>
        SYMBOL_ASSIGNMENTSTATEMENTRULE      = 25, // <assignmentStatementRule>
        SYMBOL_BITWISEEXPRESSIONRULE        = 26, // <bitwiseExpressionRule>
        SYMBOL_CMDLETCALL                   = 27, // <cmdletCall>
        SYMBOL_CMDLETNAME                   = 28, // <cmdletName>
        SYMBOL_CMLETPARAMSLIST              = 29, // <cmletParamsList>
        SYMBOL_COMPARISONEXPRESSIONRULE     = 30, // <comparisonExpressionRule>
        SYMBOL_EXPRESSIONRULE               = 31, // <expressionRule>
        SYMBOL_FORMATEXPRESSIONRULE         = 32, // <formatExpressionRule>
        SYMBOL_LOGICALEXPRESSIONRULE        = 33, // <logicalExpressionRule>
        SYMBOL_LVALUE                       = 34, // <lvalue>
        SYMBOL_LVALUEEXPRESSION             = 35, // <lvalueExpression>
        SYMBOL_MULTIPLYEXPRESSIONRULE       = 36, // <multiplyExpressionRule>
        SYMBOL_PARAMETERARGUMENTTOKEN       = 37, // <ParameterArgumentToken>
        SYMBOL_PIPELINERULE                 = 38, // <pipelineRule>
        SYMBOL_POSTFIXOPERATORRULE          = 39, // <postfixOperatorRule>
        SYMBOL_PROPERTYORARRAYREFERENCERULE = 40, // <propertyOrArrayReferenceRule>
        SYMBOL_RANGEEXPRESSIONRULE          = 41, // <rangeExpressionRule>
        SYMBOL_SIMPLELVALUE                 = 42, // <simpleLvalue>
        SYMBOL_STATEMENTLISTRULE            = 43, // <statementListRule>
        SYMBOL_STATEMENTRULE                = 44, // <statementRule>
        SYMBOL_STATEMENTSEPARATORTOKEN      = 45, // <statementSeparatorToken>
        SYMBOL_VALUERULE                    = 46  // <valueRule>
    };

    enum RuleConstants : int
    {
        RULE_STATEMENTSEPARATORTOKEN_NEWLINE                 =  0, // <statementSeparatorToken> ::= NewLine <statementSeparatorToken>
        RULE_STATEMENTSEPARATORTOKEN                         =  1, // <statementSeparatorToken> ::= 
        RULE_STATEMENTLISTRULE                               =  2, // <statementListRule> ::= <statementRule>
        RULE_STATEMENTLISTRULE2                              =  3, // <statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
        RULE_STATEMENTRULE                                   =  4, // <statementRule> ::= <pipelineRule>
        RULE_STATEMENTRULE_COMMENTTOKEN                      =  5, // <statementRule> ::= CommentToken
        RULE_PIPELINERULE                                    =  6, // <pipelineRule> ::= <cmdletCall>
        RULE_PIPELINERULE_PIPE                               =  7, // <pipelineRule> ::= <cmdletCall> '|' <pipelineRule>
        RULE_PIPELINERULE2                                   =  8, // <pipelineRule> ::= <assignmentStatementRule>
        RULE_PIPELINERULE_PIPE2                              =  9, // <pipelineRule> ::= <assignmentStatementRule> '|' <pipelineRule>
        RULE_ASSIGNMENTSTATEMENTRULE_ASSIGNMENTOPERATORTOKEN = 10, // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
        RULE_LVALUEEXPRESSION                                = 11, // <lvalueExpression> ::= <lvalue>
        RULE_LVALUE                                          = 12, // <lvalue> ::= <simpleLvalue>
        RULE_SIMPLELVALUE_VARIABLETOKEN                      = 13, // <simpleLvalue> ::= VariableToken
        RULE_PARAMETERARGUMENTTOKEN                          = 14, // <ParameterArgumentToken> ::= <valueRule>
        RULE_PARAMETERARGUMENTTOKEN_ANYWORDTOKEN             = 15, // <ParameterArgumentToken> ::= AnyWordToken
        RULE_PARAMETERARGUMENTTOKEN_PARAMETERTOKEN           = 16, // <ParameterArgumentToken> ::= ParameterToken
        RULE_CMLETPARAMSLIST                                 = 17, // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
        RULE_CMLETPARAMSLIST2                                = 18, // <cmletParamsList> ::= <ParameterArgumentToken>
        RULE_CMDLETNAME_ANYWORDTOKEN                         = 19, // <cmdletName> ::= AnyWordToken
        RULE_CMDLETCALL_EXECCALL                             = 20, // <cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
        RULE_CMDLETCALL_EXECCALL2                            = 21, // <cmdletCall> ::= ExecCall <cmdletName>
        RULE_CMDLETCALL                                      = 22, // <cmdletCall> ::= <cmdletName> <cmletParamsList>
        RULE_CMDLETCALL2                                     = 23, // <cmdletCall> ::= <cmdletName>
        RULE_CMDLETCALL3                                     = 24, // <cmdletCall> ::= <expressionRule>
        RULE_EXPRESSIONRULE                                  = 25, // <expressionRule> ::= <logicalExpressionRule>
        RULE_LOGICALEXPRESSIONRULE                           = 26, // <logicalExpressionRule> ::= <bitwiseExpressionRule>
        RULE_BITWISEEXPRESSIONRULE                           = 27, // <bitwiseExpressionRule> ::= <comparisonExpressionRule>
        RULE_COMPARISONEXPRESSIONRULE                        = 28, // <comparisonExpressionRule> ::= <addExpressionRule>
        RULE_ADDEXPRESSIONRULE                               = 29, // <addExpressionRule> ::= <multiplyExpressionRule>
        RULE_ADDEXPRESSIONRULE_ADDITIONOPERATORTOKEN         = 30, // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
        RULE_MULTIPLYEXPRESSIONRULE                          = 31, // <multiplyExpressionRule> ::= <formatExpressionRule>
        RULE_FORMATEXPRESSIONRULE                            = 32, // <formatExpressionRule> ::= <rangeExpressionRule>
        RULE_RANGEEXPRESSIONRULE                             = 33, // <rangeExpressionRule> ::= <arrayLiteralRule>
        RULE_RANGEEXPRESSIONRULE_RANGEOPERATORTOKEN          = 34, // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
        RULE_ARRAYLITERALRULE                                = 35, // <arrayLiteralRule> ::= <postfixOperatorRule>
        RULE_ARRAYLITERALRULE_COMMATOKEN                     = 36, // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
        RULE_POSTFIXOPERATORRULE                             = 37, // <postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
        RULE_PROPERTYORARRAYREFERENCERULE                    = 38, // <propertyOrArrayReferenceRule> ::= <valueRule>
        RULE_VALUERULE_STRINGTOKEN                           = 39, // <valueRule> ::= StringToken
        RULE_VALUERULE_VARIABLETOKEN                         = 40, // <valueRule> ::= VariableToken
        RULE_VALUERULE_NUMBERTOKEN                           = 41, // <valueRule> ::= NumberToken
        RULE_VALUERULE_DOLLARLPARAN_RPARAN                   = 42, // <valueRule> ::= '$(' <statementRule> ')'
        RULE_VALUERULE_LPARAN_RPARAN                         = 43  // <valueRule> ::= '(' <assignmentStatementRule> ')'
    };

        // this class will construct a parser without having to process
        //  the CGT tables with each creation.  It must be initialized
        //  before you can call CreateParser()
	public sealed class ParserFactory
	{
		static Grammar m_grammar;
		static bool _init;
		
		private ParserFactory()
		{
		}
		
		private static BinaryReader GetResourceReader(string resourceName)
		{  
			Assembly assembly = Assembly.GetExecutingAssembly();   
			Stream stream = assembly.GetManifestResourceStream(resourceName);
			return new BinaryReader(stream);
		}
		
		public static void InitializeFactoryFromFile(string FullCGTFilePath)
		{
			if (!_init)
			{
			   BinaryReader reader = new BinaryReader(new FileStream(FullCGTFilePath,FileMode.Open));
			   m_grammar = new Grammar( reader );
			   _init = true;
			}
		}
		
		public static void InitializeFactoryFromResource(string resourceName)
		{
			if (!_init)
			{
				BinaryReader reader = GetResourceReader(resourceName);
				m_grammar = new Grammar( reader );
				_init = true;
			}
		}
		
		public static Parser CreateParser(TextReader reader)
		{
		   if (_init)
		   {
				return new Parser(reader, m_grammar);
		   }
		   throw new Exception("You must first Initialize the Factory before creating a parser!");
		}
	}
        
	public abstract class ASTNode
	{
		public abstract bool IsTerminal
		{
			get;
		}
	}
	
	/// <summary>
	/// Derive this class for Terminal AST Nodes
	/// </summary>
	public class TerminalNode : ASTNode
	{
		private Symbol m_symbol;
		private string m_text;
		private int m_lineNumber;
		private int m_linePosition;

		public TerminalNode(Parser theParser)
		{
			m_symbol = theParser.TokenSymbol;
			m_text = theParser.TokenSymbol.ToString();
			m_lineNumber = theParser.LineNumber;
			m_linePosition = theParser.LinePosition;
		}

		public override bool IsTerminal
		{
			get
			{
				return true;
			}
		}
		
		public Symbol Symbol
		{
			get { return m_symbol; }
		}

		public string Text
		{
			get { return m_text; }
		}

		public override string ToString()
		{
			return m_text;
		}

		public int LineNumber 
		{
			get { return m_lineNumber; }
		}

		public int LinePosition
		{
			get { return m_linePosition; }
		}
	}
	
	/// <summary>
	/// Derive this class for NonTerminal AST Nodes
	/// </summary>
	public class NonTerminalNode : ASTNode
	{
		private int m_reductionNumber;
		private Rule m_rule;
		private ArrayList m_array = new ArrayList();

		public NonTerminalNode(Parser theParser)
		{
			m_rule = theParser.ReductionRule;
		}
		
		public override bool IsTerminal
		{
			get
			{
				return false;
			}
		}

		public int ReductionNumber 
		{
			get { return m_reductionNumber; }
			set { m_reductionNumber = value; }
		}

		public int Count 
		{
			get { return m_array.Count; }
		}

		public ASTNode this[int index]
		{
			get { return m_array[index] as ASTNode; }
		}

		public void AppendChildNode(ASTNode node)
		{
			if (node == null)
			{
				return ; 
			}
			m_array.Add(node);
		}

		public Rule Rule
		{
			get { return m_rule; }
		}

	}

    public class MyParser
    {
        MyParserContext m_context;
        ASTNode m_AST;
        string m_errorString;
        Parser m_parser;
        
        public int LineNumber
        {
            get
            {
                return m_parser.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return m_parser.LinePosition;
            }
        }

        public string ErrorString
        {
            get
            {
                return m_errorString;
            }
        }

        public string ErrorLine
        {
            get
            {
                return m_parser.LineText;
            }
        }

        public ASTNode SyntaxTree
        {
            get
            {
                return m_AST;
            }
        }

        public bool Parse(string source)
        {
            return Parse(new StringReader(source));
        }

        public bool Parse(StringReader sourceReader)
        {
            m_parser = ParserFactory.CreateParser(sourceReader);
            m_parser.TrimReductions = true;
            m_context = new MyParserContext(m_parser);
            
            while (true)
            {
                switch (m_parser.Parse())
                {
                    case ParseMessage.LexicalError:
                        m_errorString = string.Format("Lexical Error. Line {0}. Token {1} was not expected.", m_parser.LineNumber, m_parser.TokenText);
                        return false;

                    case ParseMessage.SyntaxError:
                        StringBuilder text = new StringBuilder();
                        foreach (Symbol tokenSymbol in m_parser.GetExpectedTokens())
                        {
                            text.Append(' ');
                            text.Append(tokenSymbol.ToString());
                        }
                        m_errorString = string.Format("Syntax Error. Line {0}. Expecting: {1}.", m_parser.LineNumber, text.ToString());
                        
                        return false;
                    case ParseMessage.Reduction:
                        m_parser.TokenSyntaxNode = m_context.CreateASTNode();
                        break;

                    case ParseMessage.Accept:
                        m_AST = m_parser.TokenSyntaxNode as ASTNode;
                        m_errorString = null;
                        return true;

                    case ParseMessage.InternalError:
                        m_errorString = "Internal Error. Something is horribly wrong.";
                        return false;

                    case ParseMessage.NotLoadedError:
                        m_errorString = "Grammar Table is not loaded.";
                        return false;

                    case ParseMessage.CommentError:
                        m_errorString = "Comment Error. Unexpected end of input.";
                        
                        return false;

                    case ParseMessage.CommentBlockRead:
                    case ParseMessage.CommentLineRead:
                        // don't do anything 
                        break;
                }
            }
         }

    }

    public class MyParserContext
    {

		// instance fields
        private Parser m_parser;
		
        private TextReader m_inputReader;
		

		
		// constructor
        public MyParserContext(Parser prser)
        {
            m_parser = prser;	
        }
       

        private string GetTokenText()
        {
            // delete any of these that are non-terminals.

            switch (m_parser.TokenSymbol.Index)
            {

                case (int)SymbolConstants.SYMBOL_EOF :
                //(EOF)
                //Token Kind: 3
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ERROR :
                //(Error)
                //Token Kind: 7
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_COMMENT :
                //Comment
                //Token Kind: 2
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_WHITESPACE :
                //Whitespace
                //Token Kind: 2
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_NUM :
                //'#'
                //Token Kind: 6
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_TIMESDIV :
                //'*/'
                //Token Kind: 5
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_DIVTIMES :
                //'/*'
                //Token Kind: 4
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_DOLLARLPARAN :
                //'$('
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_LPARAN :
                //'('
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_RPARAN :
                //')'
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ADDITIONOPERATORTOKEN :
                //AdditionOperatorToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ANYWORDTOKEN :
                //AnyWordToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ASSIGNMENTOPERATORTOKEN :
                //AssignmentOperatorToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_COMMATOKEN :
                //CommaToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_COMMENTTOKEN :
                //CommentToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_EXECCALL :
                //ExecCall
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_NEWLINE :
                //NewLine
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_NUMBERTOKEN :
                //NumberToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_PARAMETERTOKEN :
                //ParameterToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_RANGEOPERATORTOKEN :
                //RangeOperatorToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_STRINGTOKEN :
                //StringToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_VARIABLETOKEN :
                //VariableToken
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_PIPE :
                //'|'
                //Token Kind: 1
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ADDEXPRESSIONRULE :
                //<addExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ARRAYLITERALRULE :
                //<arrayLiteralRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_ASSIGNMENTSTATEMENTRULE :
                //<assignmentStatementRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_BITWISEEXPRESSIONRULE :
                //<bitwiseExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_CMDLETCALL :
                //<cmdletCall>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_CMDLETNAME :
                //<cmdletName>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_CMLETPARAMSLIST :
                //<cmletParamsList>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_COMPARISONEXPRESSIONRULE :
                //<comparisonExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_EXPRESSIONRULE :
                //<expressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_FORMATEXPRESSIONRULE :
                //<formatExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_LOGICALEXPRESSIONRULE :
                //<logicalExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_LVALUE :
                //<lvalue>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_LVALUEEXPRESSION :
                //<lvalueExpression>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_MULTIPLYEXPRESSIONRULE :
                //<multiplyExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_PARAMETERARGUMENTTOKEN :
                //<ParameterArgumentToken>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_PIPELINERULE :
                //<pipelineRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_POSTFIXOPERATORRULE :
                //<postfixOperatorRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_PROPERTYORARRAYREFERENCERULE :
                //<propertyOrArrayReferenceRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_RANGEEXPRESSIONRULE :
                //<rangeExpressionRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_SIMPLELVALUE :
                //<simpleLvalue>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_STATEMENTLISTRULE :
                //<statementListRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_STATEMENTRULE :
                //<statementRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_STATEMENTSEPARATORTOKEN :
                //<statementSeparatorToken>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                case (int)SymbolConstants.SYMBOL_VALUERULE :
                //<valueRule>
                //Token Kind: 0
                //todo: uncomment the next line if it's a terminal token ( if Token Kind = 1 )
                // return m_parser.TokenString;
                return null;

                default:
                    throw new SymbolException("You don't want the text of a non-terminal symbol");

            }
            
        }

        public ASTNode CreateASTNode()
        {
            switch (m_parser.ReductionRule.Index)
            {
                case (int)RuleConstants.RULE_STATEMENTSEPARATORTOKEN_NEWLINE :
                //<statementSeparatorToken> ::= NewLine <statementSeparatorToken>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_STATEMENTSEPARATORTOKEN :
                //<statementSeparatorToken> ::= 
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_STATEMENTLISTRULE :
                //<statementListRule> ::= <statementRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_STATEMENTLISTRULE2 :
                //<statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_STATEMENTRULE :
                //<statementRule> ::= <pipelineRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_STATEMENTRULE_COMMENTTOKEN :
                //<statementRule> ::= CommentToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PIPELINERULE :
                //<pipelineRule> ::= <cmdletCall>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PIPELINERULE_PIPE :
                //<pipelineRule> ::= <cmdletCall> '|' <pipelineRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PIPELINERULE2 :
                //<pipelineRule> ::= <assignmentStatementRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PIPELINERULE_PIPE2 :
                //<pipelineRule> ::= <assignmentStatementRule> '|' <pipelineRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_ASSIGNMENTSTATEMENTRULE_ASSIGNMENTOPERATORTOKEN :
                //<assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_LVALUEEXPRESSION :
                //<lvalueExpression> ::= <lvalue>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_LVALUE :
                //<lvalue> ::= <simpleLvalue>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_SIMPLELVALUE_VARIABLETOKEN :
                //<simpleLvalue> ::= VariableToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PARAMETERARGUMENTTOKEN :
                //<ParameterArgumentToken> ::= <valueRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PARAMETERARGUMENTTOKEN_ANYWORDTOKEN :
                //<ParameterArgumentToken> ::= AnyWordToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PARAMETERARGUMENTTOKEN_PARAMETERTOKEN :
                //<ParameterArgumentToken> ::= ParameterToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMLETPARAMSLIST :
                //<cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMLETPARAMSLIST2 :
                //<cmletParamsList> ::= <ParameterArgumentToken>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMDLETNAME_ANYWORDTOKEN :
                //<cmdletName> ::= AnyWordToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMDLETCALL_EXECCALL :
                //<cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMDLETCALL_EXECCALL2 :
                //<cmdletCall> ::= ExecCall <cmdletName>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMDLETCALL :
                //<cmdletCall> ::= <cmdletName> <cmletParamsList>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMDLETCALL2 :
                //<cmdletCall> ::= <cmdletName>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_CMDLETCALL3 :
                //<cmdletCall> ::= <expressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_EXPRESSIONRULE :
                //<expressionRule> ::= <logicalExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_LOGICALEXPRESSIONRULE :
                //<logicalExpressionRule> ::= <bitwiseExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_BITWISEEXPRESSIONRULE :
                //<bitwiseExpressionRule> ::= <comparisonExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_COMPARISONEXPRESSIONRULE :
                //<comparisonExpressionRule> ::= <addExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_ADDEXPRESSIONRULE :
                //<addExpressionRule> ::= <multiplyExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_ADDEXPRESSIONRULE_ADDITIONOPERATORTOKEN :
                //<addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_MULTIPLYEXPRESSIONRULE :
                //<multiplyExpressionRule> ::= <formatExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_FORMATEXPRESSIONRULE :
                //<formatExpressionRule> ::= <rangeExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_RANGEEXPRESSIONRULE :
                //<rangeExpressionRule> ::= <arrayLiteralRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_RANGEEXPRESSIONRULE_RANGEOPERATORTOKEN :
                //<rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_ARRAYLITERALRULE :
                //<arrayLiteralRule> ::= <postfixOperatorRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_ARRAYLITERALRULE_COMMATOKEN :
                //<arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_POSTFIXOPERATORRULE :
                //<postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_PROPERTYORARRAYREFERENCERULE :
                //<propertyOrArrayReferenceRule> ::= <valueRule>
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_VALUERULE_STRINGTOKEN :
                //<valueRule> ::= StringToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_VALUERULE_VARIABLETOKEN :
                //<valueRule> ::= VariableToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_VALUERULE_NUMBERTOKEN :
                //<valueRule> ::= NumberToken
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_VALUERULE_DOLLARLPARAN_RPARAN :
                //<valueRule> ::= '$(' <statementRule> ')'
                //todo: Perhaps create an object in the AST.
                return null;

                case (int)RuleConstants.RULE_VALUERULE_LPARAN_RPARAN :
                //<valueRule> ::= '(' <assignmentStatementRule> ')'
                //todo: Perhaps create an object in the AST.
                return null;

                default:
					throw new RuleException("Unknown rule: Does your CGT Match your Code Revision?");
            }
            
        }

    }
    
}
