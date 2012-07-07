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

namespace Pash.ParserIntrinsics
{
    #region Exceptions
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
    #endregion
    
    enum SymbolConstants : int
    {
        Symbol_Eof                          =  0, // (EOF)
        Symbol_Error                        =  1, // (Error)
        Symbol_Whitespace                   =  2, // (Whitespace)
        Symbol_Commentend                   =  3, // (Comment End)
        Symbol_Commentline                  =  4, // (Comment Line)
        Symbol_Commentstart                 =  5, // (Comment Start)
        Symbol_Dollarlparan                 =  6, // $(
        Symbol_Lparan                       =  7, // (
        Symbol_Rparan                       =  8, // )
        Symbol_Pipe                         =  9, // |
        Symbol_Additionoperatortoken        = 10, // AdditionOperatorToken
        Symbol_Anywordtoken                 = 11, // AnyWordToken
        Symbol_Assignmentoperatortoken      = 12, // AssignmentOperatorToken
        Symbol_Commatoken                   = 13, // CommaToken
        Symbol_Commenttoken                 = 14, // CommentToken
        Symbol_Execcall                     = 15, // ExecCall
        Symbol_Newline                      = 16, // NewLine
        Symbol_Numbertoken                  = 17, // NumberToken
        Symbol_Parametertoken               = 18, // ParameterToken
        Symbol_Rangeoperatortoken           = 19, // RangeOperatorToken
        Symbol_Stringtoken                  = 20, // StringToken
        Symbol_Variabletoken                = 21, // VariableToken
        Symbol_Addexpressionrule            = 22, // <addExpressionRule>
        Symbol_Arrayliteralrule             = 23, // <arrayLiteralRule>
        Symbol_Assignmentstatementrule      = 24, // <assignmentStatementRule>
        Symbol_Bitwiseexpressionrule        = 25, // <bitwiseExpressionRule>
        Symbol_Cmdletcall                   = 26, // <cmdletCall>
        Symbol_Cmdletname                   = 27, // <cmdletName>
        Symbol_Cmletparamslist              = 28, // <cmletParamsList>
        Symbol_Comparisonexpressionrule     = 29, // <comparisonExpressionRule>
        Symbol_Expressionrule               = 30, // <expressionRule>
        Symbol_Formatexpressionrule         = 31, // <formatExpressionRule>
        Symbol_Logicalexpressionrule        = 32, // <logicalExpressionRule>
        Symbol_Lvalue                       = 33, // <lvalue>
        Symbol_Lvalueexpression             = 34, // <lvalueExpression>
        Symbol_Multiplyexpressionrule       = 35, // <multiplyExpressionRule>
        Symbol_Parameterargumenttoken       = 36, // <ParameterArgumentToken>
        Symbol_Pipelinerule                 = 37, // <pipelineRule>
        Symbol_Postfixoperatorrule          = 38, // <postfixOperatorRule>
        Symbol_Propertyorarrayreferencerule = 39, // <propertyOrArrayReferenceRule>
        Symbol_Rangeexpressionrule          = 40, // <rangeExpressionRule>
        Symbol_Simplelvalue                 = 41, // <simpleLvalue>
        Symbol_Statementlistrule            = 42, // <statementListRule>
        Symbol_Statementrule                = 43, // <statementRule>
        Symbol_Statementseparatortoken      = 44, // <statementSeparatorToken>
        Symbol_Valuerule                    = 45  // <valueRule>
    };

    enum RuleConstants : int
    {
        Rule_Statementseparatortoken_Newline                 =  0, // <statementSeparatorToken> ::= NewLine <statementSeparatorToken>
        Rule_Statementseparatortoken                         =  1, // <statementSeparatorToken> ::= 
        Rule_Statementlistrule                               =  2, // <statementListRule> ::= <statementRule>
        Rule_Statementlistrule2                              =  3, // <statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
        Rule_Statementrule                                   =  4, // <statementRule> ::= <pipelineRule>
        Rule_Statementrule_Commenttoken                      =  5, // <statementRule> ::= CommentToken
        Rule_Pipelinerule                                    =  6, // <pipelineRule> ::= <cmdletCall>
        Rule_Pipelinerule_Pipe                               =  7, // <pipelineRule> ::= <cmdletCall> | <pipelineRule>
        Rule_Pipelinerule2                                   =  8, // <pipelineRule> ::= <assignmentStatementRule>
        Rule_Pipelinerule_Pipe2                              =  9, // <pipelineRule> ::= <assignmentStatementRule> | <pipelineRule>
        Rule_Assignmentstatementrule_Assignmentoperatortoken = 10, // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
        Rule_Lvalueexpression                                = 11, // <lvalueExpression> ::= <lvalue>
        Rule_Lvalue                                          = 12, // <lvalue> ::= <simpleLvalue>
        Rule_Simplelvalue_Variabletoken                      = 13, // <simpleLvalue> ::= VariableToken
        Rule_Parameterargumenttoken                          = 14, // <ParameterArgumentToken> ::= <valueRule>
        Rule_Parameterargumenttoken_Anywordtoken             = 15, // <ParameterArgumentToken> ::= AnyWordToken
        Rule_Parameterargumenttoken_Parametertoken           = 16, // <ParameterArgumentToken> ::= ParameterToken
        Rule_Cmletparamslist                                 = 17, // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
        Rule_Cmletparamslist2                                = 18, // <cmletParamsList> ::= <ParameterArgumentToken>
        Rule_Cmdletname_Anywordtoken                         = 19, // <cmdletName> ::= AnyWordToken
        Rule_Cmdletcall_Execcall                             = 20, // <cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
        Rule_Cmdletcall_Execcall2                            = 21, // <cmdletCall> ::= ExecCall <cmdletName>
        Rule_Cmdletcall                                      = 22, // <cmdletCall> ::= <cmdletName> <cmletParamsList>
        Rule_Cmdletcall2                                     = 23, // <cmdletCall> ::= <cmdletName>
        Rule_Cmdletcall3                                     = 24, // <cmdletCall> ::= <expressionRule>
        Rule_Expressionrule                                  = 25, // <expressionRule> ::= <logicalExpressionRule>
        Rule_Logicalexpressionrule                           = 26, // <logicalExpressionRule> ::= <bitwiseExpressionRule>
        Rule_Bitwiseexpressionrule                           = 27, // <bitwiseExpressionRule> ::= <comparisonExpressionRule>
        Rule_Comparisonexpressionrule                        = 28, // <comparisonExpressionRule> ::= <addExpressionRule>
        Rule_Addexpressionrule                               = 29, // <addExpressionRule> ::= <multiplyExpressionRule>
        Rule_Addexpressionrule_Additionoperatortoken         = 30, // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
        Rule_Multiplyexpressionrule                          = 31, // <multiplyExpressionRule> ::= <formatExpressionRule>
        Rule_Formatexpressionrule                            = 32, // <formatExpressionRule> ::= <rangeExpressionRule>
        Rule_Rangeexpressionrule                             = 33, // <rangeExpressionRule> ::= <arrayLiteralRule>
        Rule_Rangeexpressionrule_Rangeoperatortoken          = 34, // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
        Rule_Arrayliteralrule                                = 35, // <arrayLiteralRule> ::= <postfixOperatorRule>
        Rule_Arrayliteralrule_Commatoken                     = 36, // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
        Rule_Postfixoperatorrule                             = 37, // <postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
        Rule_Propertyorarrayreferencerule                    = 38, // <propertyOrArrayReferenceRule> ::= <valueRule>
        Rule_Valuerule_Stringtoken                           = 39, // <valueRule> ::= StringToken
        Rule_Valuerule_Variabletoken                         = 40, // <valueRule> ::= VariableToken
        Rule_Valuerule_Numbertoken                           = 41, // <valueRule> ::= NumberToken
        Rule_Valuerule_Dollarlparan_Rparan                   = 42, // <valueRule> ::= $( <statementRule> )
        Rule_Valuerule_Lparan_Rparan                         = 43  // <valueRule> ::= ( <assignmentStatementRule> )
    };

    #region ParserFactory
    // this class will construct a parser without having to process
    //  the CGT tables with each creation.  It must be initialized
    //  before you can call CreateParser()
	public sealed class ParserFactory
	{
		Grammar m_grammar;
		bool _init;
		
		public ParserFactory()
		{
		}
		
		private BinaryReader GetResourceReader(string resourceName)
		{  
			Assembly assembly = Assembly.GetExecutingAssembly();   
			Stream stream = assembly.GetManifestResourceStream(resourceName);
			return new BinaryReader(stream);
		}
		
		public void InitializeFactoryFromFile(string FullCGTFilePath)
		{
			if (!_init)
			{
			   BinaryReader reader = new BinaryReader(new FileStream(FullCGTFilePath,FileMode.Open));
			   m_grammar = new Grammar( reader );
			   _init = true;
			}
		}
		
		public void InitializeFactoryFromResource(string resourceName)
		{
			if (!_init)
			{
				BinaryReader reader = GetResourceReader(resourceName);
				m_grammar = new Grammar( reader );
				_init = true;
			}
		}
		
		public Parser CreateParser(TextReader reader)
		{
		   if (_init)
		   {
				return new Parser(reader, m_grammar);
		   }
		   throw new Exception("You must first Initialize the Factory before creating a parser!");
		}
	}
    #endregion
    
	public abstract partial class ASTNode
	{
		public abstract bool IsTerminal
		{
			get;
		}
		
		protected object Token(Parser theParser, int index)
		{
			return theParser.GetReductionSyntaxNode(index);
		}

		protected ASTNode Node(Parser theParser, int index)
		{
			return (ASTNode)theParser.GetReductionSyntaxNode(index);
		}

        internal abstract void Execute(ExecutionContext context, ICommandRuntime commandRuntime);

	    protected object _value = null;
        internal virtual object GetValue(ExecutionContext context)
        {
            return _value;
        }
	}
	
	/// <summary>
	/// Derive this class for Terminal AST Nodes
	/// </summary>
	public partial class TerminalNode : ASTNode
	{
		public Symbol Symbol { get; private set; }
		public string Text { get; protected set; }
		public int LineNumber { get; private set; }
		public int LinePosition { get; private set; }
		public override bool IsTerminal { get { return true; } }

		public TerminalNode(Parser theParser)
		{
			Symbol = theParser.TokenSymbol;
			//Text = theParser.TokenSymbol.ToString();
			Text = (string)Token(theParser, 0);
			LineNumber = theParser.LineNumber;
			LinePosition = theParser.LinePosition;
		}

		public override string ToString()
		{
			return Text;
		}

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(Text);
        }
	}
	
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
		
		public override bool IsTerminal
		{
			get
			{
				return false;
			}
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
				return ; 
			}
			m_array.Add(node);
		}
	}

    public partial class PashParser
    {
        PashParserContext m_context;
        ASTNode m_AST;
        string m_errorString;
        Parser m_parser;
        private ParserFactory pFactory;
        
        public PashParser(ParserFactory factory)
        {
			pFactory = factory;
        }
        
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
            [System.Diagnostics.DebuggerStepThrough]
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
            m_parser = pFactory.CreateParser(sourceReader);
            m_parser.TrimReductions = true;
            m_context = new PashParserContext(m_parser);
            
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
                        
                    case ParseMessage.TokenRead:
                        //=== Make sure that we store token string for needed tokens.
                        m_parser.TokenSyntaxNode = m_context.GetTokenText();
                        break;

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

    public partial class PashParserContext
    {
		private class TokenContainer
		{
			public string Token = null;
		}
		
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
			return (ASTNode) m_parser.GetReductionSyntaxNode(index);
		}
		
		private object Token(int index)
		{
			return m_parser.GetReductionSyntaxNode(index);
		}

        public string GetTokenText()
        {
            // delete any of these that are non-terminals.
            var tokenContainer = new TokenContainer();

            //if (Enum.IsDefined(typeof(SymbolConstants), m_parser.TokenSymbol.Index))
            //    tokenContainer.Token = m_parser.TokenText;

            System.Diagnostics.Debug.WriteLine(Enum.GetName(typeof(SymbolConstants), m_parser.TokenSymbol.Index));
            
            switch ((SymbolConstants)m_parser.TokenSymbol.Index)
            {

                case SymbolConstants.Symbol_Eof:              // (EOF)
                if (3 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Eof(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Error:              // (Error)
                if (7 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Error(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Whitespace:              // (Whitespace)
                if (2 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Whitespace(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Commentend:              // (Comment End)
                if (5 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Commentend(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Commentline:              // (Comment Line)
                if (6 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Commentline(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Commentstart:              // (Comment Start)
                if (4 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Commentstart(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Dollarlparan:              // $(
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Dollarlparan(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Lparan:              // (
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Lparan(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Rparan:              // )
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Rparan(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Pipe:              // |
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Pipe(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Additionoperatortoken:              // AdditionOperatorToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Additionoperatortoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Anywordtoken:              // AnyWordToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Anywordtoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Assignmentoperatortoken:              // AssignmentOperatorToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Assignmentoperatortoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Commatoken:              // CommaToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Commatoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Commenttoken:              // CommentToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Commenttoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Execcall:              // ExecCall
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Execcall(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Newline:              // NewLine
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Newline(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Numbertoken:              // NumberToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Numbertoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Parametertoken:              // ParameterToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Parametertoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Rangeoperatortoken:              // RangeOperatorToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Rangeoperatortoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Stringtoken:              // StringToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Stringtoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Variabletoken:              // VariableToken
                if (1 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Variabletoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Addexpressionrule:              // <addExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Addexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Arrayliteralrule:              // <arrayLiteralRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Arrayliteralrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Assignmentstatementrule:              // <assignmentStatementRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Assignmentstatementrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Bitwiseexpressionrule:              // <bitwiseExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Bitwiseexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Cmdletcall:              // <cmdletCall>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Cmdletcall(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Cmdletname:              // <cmdletName>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Cmdletname(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Cmletparamslist:              // <cmletParamsList>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Cmletparamslist(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Comparisonexpressionrule:              // <comparisonExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Comparisonexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Expressionrule:              // <expressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Expressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Formatexpressionrule:              // <formatExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Formatexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Logicalexpressionrule:              // <logicalExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Logicalexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Lvalue:              // <lvalue>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Lvalue(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Lvalueexpression:              // <lvalueExpression>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Lvalueexpression(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Multiplyexpressionrule:              // <multiplyExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Multiplyexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Parameterargumenttoken:              // <ParameterArgumentToken>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Parameterargumenttoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Pipelinerule:              // <pipelineRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Pipelinerule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Postfixoperatorrule:              // <postfixOperatorRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Postfixoperatorrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Propertyorarrayreferencerule:              // <propertyOrArrayReferenceRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Propertyorarrayreferencerule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Rangeexpressionrule:              // <rangeExpressionRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Rangeexpressionrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Simplelvalue:              // <simpleLvalue>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Simplelvalue(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Statementlistrule:              // <statementListRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Statementlistrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Statementrule:              // <statementRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Statementrule(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Statementseparatortoken:              // <statementSeparatorToken>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Statementseparatortoken(m_parser, tokenContainer);
                break;

                case SymbolConstants.Symbol_Valuerule:              // <valueRule>
                if (0 == 1)  // If 1 - terminal token
					return m_parser.TokenString;
				else
					CreateToken_Symbol_Valuerule(m_parser, tokenContainer);
                break;

                default:
                    throw new SymbolException("You don't want the text of a non-terminal symbol");

            }
            return tokenContainer.Token;
        }
        
        #region Abstract Parser Methods
        // (EOF) ( Terminal [1] = 3 )
        partial void CreateToken_Symbol_Eof(Parser theParser, TokenContainer token);
        
        // (Error) ( Terminal [1] = 7 )
        partial void CreateToken_Symbol_Error(Parser theParser, TokenContainer token);
        
        // (Whitespace) ( Terminal [1] = 2 )
        partial void CreateToken_Symbol_Whitespace(Parser theParser, TokenContainer token);
        
        // (Comment End) ( Terminal [1] = 5 )
        partial void CreateToken_Symbol_Commentend(Parser theParser, TokenContainer token);
        
        // (Comment Line) ( Terminal [1] = 6 )
        partial void CreateToken_Symbol_Commentline(Parser theParser, TokenContainer token);
        
        // (Comment Start) ( Terminal [1] = 4 )
        partial void CreateToken_Symbol_Commentstart(Parser theParser, TokenContainer token);
        
        // $( ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Dollarlparan(Parser theParser, TokenContainer token);
        
        // ( ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Lparan(Parser theParser, TokenContainer token);
        
        // ) ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Rparan(Parser theParser, TokenContainer token);
        
        // | ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Pipe(Parser theParser, TokenContainer token);
        
        // AdditionOperatorToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Additionoperatortoken(Parser theParser, TokenContainer token);
        
        // AnyWordToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Anywordtoken(Parser theParser, TokenContainer token);
        
        // AssignmentOperatorToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Assignmentoperatortoken(Parser theParser, TokenContainer token);
        
        // CommaToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Commatoken(Parser theParser, TokenContainer token);
        
        // CommentToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Commenttoken(Parser theParser, TokenContainer token);
        
        // ExecCall ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Execcall(Parser theParser, TokenContainer token);
        
        // NewLine ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Newline(Parser theParser, TokenContainer token);
        
        // NumberToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Numbertoken(Parser theParser, TokenContainer token);
        
        // ParameterToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Parametertoken(Parser theParser, TokenContainer token);
        
        // RangeOperatorToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Rangeoperatortoken(Parser theParser, TokenContainer token);
        
        // StringToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Stringtoken(Parser theParser, TokenContainer token);
        
        // VariableToken ( Terminal [1] = 1 )
        partial void CreateToken_Symbol_Variabletoken(Parser theParser, TokenContainer token);
        
        // <addExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Addexpressionrule(Parser theParser, TokenContainer token);
        
        // <arrayLiteralRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Arrayliteralrule(Parser theParser, TokenContainer token);
        
        // <assignmentStatementRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Assignmentstatementrule(Parser theParser, TokenContainer token);
        
        // <bitwiseExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Bitwiseexpressionrule(Parser theParser, TokenContainer token);
        
        // <cmdletCall> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Cmdletcall(Parser theParser, TokenContainer token);
        
        // <cmdletName> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Cmdletname(Parser theParser, TokenContainer token);
        
        // <cmletParamsList> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Cmletparamslist(Parser theParser, TokenContainer token);
        
        // <comparisonExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Comparisonexpressionrule(Parser theParser, TokenContainer token);
        
        // <expressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Expressionrule(Parser theParser, TokenContainer token);
        
        // <formatExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Formatexpressionrule(Parser theParser, TokenContainer token);
        
        // <logicalExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Logicalexpressionrule(Parser theParser, TokenContainer token);
        
        // <lvalue> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Lvalue(Parser theParser, TokenContainer token);
        
        // <lvalueExpression> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Lvalueexpression(Parser theParser, TokenContainer token);
        
        // <multiplyExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Multiplyexpressionrule(Parser theParser, TokenContainer token);
        
        // <ParameterArgumentToken> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Parameterargumenttoken(Parser theParser, TokenContainer token);
        
        // <pipelineRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Pipelinerule(Parser theParser, TokenContainer token);
        
        // <postfixOperatorRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Postfixoperatorrule(Parser theParser, TokenContainer token);
        
        // <propertyOrArrayReferenceRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Propertyorarrayreferencerule(Parser theParser, TokenContainer token);
        
        // <rangeExpressionRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Rangeexpressionrule(Parser theParser, TokenContainer token);
        
        // <simpleLvalue> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Simplelvalue(Parser theParser, TokenContainer token);
        
        // <statementListRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Statementlistrule(Parser theParser, TokenContainer token);
        
        // <statementRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Statementrule(Parser theParser, TokenContainer token);
        
        // <statementSeparatorToken> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Statementseparatortoken(Parser theParser, TokenContainer token);
        
        // <valueRule> ( Terminal [1] = 0 )
        partial void CreateToken_Symbol_Valuerule(Parser theParser, TokenContainer token);
        
        #endregion

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
