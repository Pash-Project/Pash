#region Copyright

//----------------------------------------------------------------------
// Gold Parser engine.
// See more details on http://www.devincook.com/goldparser/
// 
// Original code is written in VB by Devin Cook (GOLDParser@DevinCook.com)
//
// This translation is done by Vladimir Morozov (vmoroz@hotmail.com)
// 
// The translation is based on the other engine translations:
// Delphi engine by Alexandre Rai (riccio@gmx.at)
// C# engine by Marcus Klimstra (klimstra@home.nl)
//----------------------------------------------------------------------

#endregion

#region Using directives

using System;
using System.IO;
using System.Text;
using System.Collections;

#endregion

namespace GoldParser
{
	/// <summary>
	/// Pull parser which uses Grammar table to parser input stream.
	/// </summary>
	public sealed class Parser
	{
		#region Fields

		private Grammar m_grammar;               // Grammar of parsed language.
		private bool    m_trimReductions = true; // Allowes to minimize reduction tree.

		private TextReader m_textReader;       // Data to parse.
		private char[]     m_buffer;           // Buffer to keep current characters.
		private int        m_bufferSize;       // Size of the buffer.
		private int        m_bufferStartIndex; // Absolute position of buffered first character. 
		private int        m_charIndex;        // Index of character in the buffer.
		private int        m_preserveChars;    // Number of characters to preserve when buffer is refilled.
		private int        m_lineStart;        // Relative position of line start to the buffer beginning.
		private int        m_lineLength;       // Length of current source line.
		private int        m_lineNumber = 1;   // Current line number.
		private int        m_commentLevel;     // Keeps stack level for embedded comments
		private StringBuilder m_commentText;   // Current comment text.

		private SourceLineReadCallback m_sourceLineReadCallback; // Called when line reading finished. 

		private Token   m_token;            // Current token
		private Token[] m_inputTokens;      // Stack of input tokens.
		private int     m_inputTokenCount;  // How many tokens in the input.
		
		private LRStackItem[] m_lrStack;        // Stack of LR states used for LR parsing.
		private int           m_lrStackIndex;   // Index of current LR state in the LR parsing stack. 
		private LRState       m_lrState;        // Current LR state.
		private int           m_reductionCount; // Number of items in reduction. It is Undefined if no reducton available. 
		private Symbol[]      m_expectedTokens; // What tokens are expected in case of error?  
		
		private const int  MinimumBufferSize = 4096;   // Minimum size of char buffer.
		private const char EndOfString = (char) 0;     // Designates last string terminator.
		private const int  MinimumInputTokenCount = 2; // Minimum input token stack size.
		private const int  MinimumLRStackSize = 256;   // Minimum size of reduction stack.
		private const int  Undefined = -1;             // Used for undefined int values. 
		
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes new instance of Parser class.
		/// </summary>
		/// <param name="textReader">TextReader instance to read data from.</param>
		/// <param name="grammar">Grammar with parsing tables to parser input stream.</param>
		public Parser(TextReader textReader, Grammar grammar)
		{
			if (textReader == null)
			{
				throw new ArgumentNullException("textReader");
			}
			if (grammar == null)
			{
				throw new ArgumentNullException("grammar");
			}

			m_textReader = textReader;
			m_bufferSize = MinimumBufferSize;
			m_buffer = new char[m_bufferSize + 1];
			m_lineLength = Undefined;
			ReadBuffer();

			m_inputTokens = new Token[MinimumInputTokenCount];
			m_lrStack     = new LRStackItem[MinimumLRStackSize];

			m_grammar = grammar;
			
			// Put grammar start symbol into LR parsing stack.
			m_lrState = m_grammar.InitialLRState;
			LRStackItem start = new LRStackItem();
			start.m_token.m_symbol = m_grammar.StartSymbol;
			start.m_state = m_lrState;
			m_lrStack[m_lrStackIndex] = start;

			m_reductionCount = Undefined; // there are no reductions yet.
		}

		#endregion

		#region Parser general properties

		/// <summary>
		/// Gets the parser's grammar.
		/// </summary>
		public Grammar Grammar 
		{
			get { return m_grammar; }
		}

		/// <summary>
		/// Gets or sets flag to trim reductions.
		/// </summary>
		public bool TrimReductions
		{
			get { return m_trimReductions; }
			set { m_trimReductions = value; }
		}

		#endregion

		#region Data Source properties and methods

		/// <summary>
		/// Gets source of parsed data.
		/// </summary>
		public TextReader TextReader
		{
			get { return m_textReader; }
		}

		/// <summary>
		/// Gets current char position.
		/// </summary>
		public int CharPosition
		{
			get { return m_charIndex + m_bufferStartIndex; }
		}

		/// <summary>
		/// Gets current line number. It is 1-based.
		/// </summary>
		public int LineNumber
		{
			get { return m_lineNumber; }
		}

		/// <summary>
		/// Gets current char position in the current source line. It is 1-based.
		/// </summary>
		public int LinePosition
		{
			get { return CharPosition - m_lineStart + 1; }
		}

		/// <summary>
		/// Gets current source line text. It can be truncated if line is longer than 2048 characters.
		/// </summary>
		public string LineText 
		{
			get 
			{
				int lineStart = Math.Max(m_lineStart, 0);
				int lineLength;
				if (m_lineLength == Undefined)
				{
					// Line was requested outside of SourceLineReadCallback call
					lineLength = m_charIndex - lineStart;
				}
				else
				{
					lineLength = m_lineLength - (lineStart - m_lineStart);
				}
				if (lineLength > 0) 
				{
					return new String(m_buffer, lineStart, lineLength);
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets callback function to track source line text.
		/// </summary>
		public SourceLineReadCallback SourceLineReadCallback
		{
			get { return m_sourceLineReadCallback; }
			set { m_sourceLineReadCallback = value; }
		}

		/// <summary>
		/// Reads next characters to the buffer.
		/// </summary>
		/// <returns>Number of characters read.</returns>
		private int ReadBuffer()
		{
			// Find out how many bytes to preserve.
			// We truncate long lines.
			int lineStart = (m_lineStart < 0) ? 0 : m_lineStart;
			int lineCharCount = m_charIndex - lineStart;
			if (lineCharCount > m_bufferSize / 2)
			{
				lineCharCount = m_bufferSize / 2;
			}
			int moveIndex = m_charIndex - lineCharCount;
			int moveCount = lineCharCount + m_preserveChars;
			if (moveCount > 0)
			{
				// We need to keep current token characters.
				if (m_bufferSize - moveCount < 20)
				{
					// Grow the buffer
					m_bufferSize = m_bufferSize * 2;
					char[] newBuffer = new char[m_bufferSize + 1];
					Array.Copy(m_buffer, moveIndex, newBuffer, 0, moveCount);
					m_buffer = newBuffer;				
				}
				else
				{
					Array.Copy(m_buffer, moveIndex, m_buffer, 0, moveCount); 
				}
			}

			// Read as many characters as possible.
			int count = m_bufferSize - moveCount;
			int result = m_textReader.Read(m_buffer, moveCount, count);
			// Mark character after last read one as End-Of-String
			m_buffer[moveCount + result] = EndOfString; 
			// Adjust buffer variables.
			m_bufferStartIndex += moveIndex;
			m_charIndex -= moveIndex;
			m_lineStart -= moveIndex;
			return result;
		}
		
		/// <summary>
		/// Increments current char index by delta character positions.
		/// </summary>
		/// <param name="delta">Number to increment char index.</param>
		private void MoveBy(int delta)
		{
			for (int i = delta; --i >= 0;)
			{
				if (m_buffer[m_charIndex++] == '\n')
				{
					if (m_sourceLineReadCallback != null)
					{
						m_lineLength = m_charIndex - m_lineStart - 1; // Exclude '\n'
						int lastIndex = m_lineStart + m_lineLength - 1;
						if (lastIndex >= 0 && m_buffer[lastIndex] == '\r')
						{
							m_lineLength--;
						}
						if (m_lineLength < 0)
						{
							m_lineLength = 0;
						}
						m_sourceLineReadCallback(this, m_lineStart + m_bufferStartIndex, m_lineLength);
					}
					m_lineNumber++;
					m_lineStart = m_charIndex;
					m_lineLength = Undefined;
				}
				if (m_buffer[m_charIndex] == '\0')
				{
					if (m_sourceLineReadCallback != null)
					{
						m_lineLength = m_charIndex - m_lineStart; 
						if (m_lineLength > 0)
						{
							m_sourceLineReadCallback(this, m_lineStart + m_bufferStartIndex, m_lineLength);
						}
						m_lineLength = Undefined;
					}
				}
			}
		}

		/// <summary>
		/// Moves current char pointer to the end of source line.
		/// </summary>
		private void MoveToLineEnd()
		{
			while (true)
			{
				char ch = m_buffer[m_charIndex];
				switch (ch)
				{
					case '\r':
					case '\n':
						return;

					case EndOfString:
						if (ReadBuffer() == 0)
						{
							return;
						}
						break;
					
					default:
						if (m_commentText != null)
						{
							m_commentText.Append(ch);
						}
						break;
				}
				m_charIndex++;
			}
		}

		#endregion

		#region Tokenizer properties and methods

		/// <summary>
		/// Gets or sets current token symbol.
		/// </summary>
		public Symbol TokenSymbol
		{
			get { return m_token.m_symbol; }
			set { m_token.m_symbol = value; }
		}

		/// <summary>
		/// Gets or sets current token text.
		/// </summary>
		public string TokenText 
		{
			get 
			{ 
				if (m_token.m_text == null)
				{
					if (m_token.m_length > 0)
					{
						m_token.m_text = new String(m_buffer, m_token.m_start - m_bufferStartIndex, m_token.m_length);
					}
					else
					{
						m_token.m_text = string.Empty;
					}
				}
				return m_token.m_text; 
			}
			set { m_token.m_text = value; }
		}

		/// <summary>
		/// Gets or sets current token position relative to input stream beginning.
		/// </summary>
		public int TokenCharPosition 
		{
			get { return m_token.m_start; }
			set { m_token.m_start = value; }
		}

		/// <summary>
		/// Gets or sets current token text length.
		/// </summary>
		public int TokenLength 
		{
			get { return m_token.m_length; }
			set { m_token.m_length = value; }
		}

		/// <summary>
		/// Gets or sets current token line number. It is 1-based.
		/// </summary>
		public int TokenLineNumber 
		{
			get { return m_token.m_lineNumber; }
			set { m_token.m_lineNumber = value; }
		}

		/// <summary>
		/// Gets or sets current token position in current source line. It is 1-based.
		/// </summary>
		public int TokenLinePosition
		{
			get { return m_token.m_linePosition; }
			set { m_token.m_linePosition = value; }
		}

		/// <summary>
		/// Gets or sets token syntax object associated with the current token or reduction.
		/// </summary>
		public object TokenSyntaxNode 
		{
			get 
			{ 
				if (m_reductionCount == Undefined)
				{
					return m_token.m_syntaxNode; 
				}
				else
				{
					return m_lrStack[m_lrStackIndex].m_token.m_syntaxNode;
				}
			}
			set 
			{ 
				if (m_reductionCount == Undefined)
				{
					m_token.m_syntaxNode = value;
				}
				else
				{
					m_lrStack[m_lrStackIndex].m_token.m_syntaxNode = value;
				}
			}
		}

		/// <summary>
		/// Returns string representation of the token.
		/// </summary>
		/// <returns>String representation of the token.</returns>
		public string TokenString
		{
			get
			{
				if (m_token.m_symbol.m_symbolType != SymbolType.Terminal)
				{
					return m_token.m_symbol.ToString();
				}
				StringBuilder sb = new StringBuilder(m_token.m_length);
				for (int i = 0; i < m_token.m_length; i++)
				{
					char ch = m_buffer[m_token.m_start - m_bufferStartIndex + i];
					if (ch < ' ')
					{
						switch (ch)
						{
							case '\n': 
								sb.Append("{LF}");
								break;
							case '\r': 
								sb.Append("{CR}");
								break;
							case '\t': 
								sb.Append("{HT}");
								break;
						}
					}
					else
					{
						sb.Append(ch);
					}
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Pushes a token to the input token stack.
		/// </summary>
		/// <param name="symbol">Token symbol.</param>
		/// <param name="text">Token text.</param>
		/// <param name="syntaxNode">Syntax node associated with the token.</param>
		public void PushInputToken(Symbol symbol, string text, object syntaxNode)
		{
			if (m_token.m_symbol != null) 
			{
				if (m_inputTokenCount == m_inputTokens.Length)
				{
					Token[] newTokenArray = new Token[m_inputTokenCount * 2];
					Array.Copy(m_inputTokens, newTokenArray, m_inputTokenCount);
					m_inputTokens = newTokenArray;
				}
				m_inputTokens[m_inputTokenCount++] = m_token;
			}
			m_token = new Token();
			m_token.m_symbol = symbol;
			m_token.m_text = text;
			m_token.m_length = (text != null) ? text.Length : 0;
			m_token.m_syntaxNode = syntaxNode;
		}

		/// <summary>
		/// Pops token from the input token stack.
		/// </summary>
		/// <returns>Token symbol from the top of input token stack.</returns>
		public Symbol PopInputToken()
		{
			Symbol result = m_token.m_symbol;
			if (m_inputTokenCount > 0)
			{
				m_token = m_inputTokens[--m_inputTokenCount];
			}
			else
			{
				m_token.m_symbol = null;
				m_token.m_text = null;
			}
			return result;
		}

		/// <summary>
		/// Reads next token from the input stream.
		/// </summary>
		/// <returns>Token symbol which was read.</returns>
		public Symbol ReadToken()
		{
			m_token.m_text = null;
			m_token.m_start = m_charIndex + m_bufferStartIndex;
			m_token.m_lineNumber = m_lineNumber;
			m_token.m_linePosition = m_charIndex + m_bufferStartIndex - m_lineStart + 1;
			int lookahead   = m_charIndex;  // Next look ahead char in the input
			int tokenLength = 0;       
			Symbol tokenSymbol = null;
			DfaState[] dfaStateTable = m_grammar.m_dfaStateTable;
			
			char ch = m_buffer[lookahead];
			if (ch == EndOfString)
			{
				if (ReadBuffer() == 0)
				{
					m_token.m_symbol = m_grammar.m_endSymbol;
					m_token.m_length = 0;
					return m_token.m_symbol;
				}
				lookahead   = m_charIndex;
				ch = m_buffer[lookahead];
			}
			DfaState dfaState = m_grammar.m_dfaInitialState;
			while (true)
			{
				dfaState = dfaState.m_transitionVector[ch] as DfaState;

				// This block-if statement checks whether an edge was found from the current state.
				// If so, the state and current position advance. Otherwise it is time to exit the main loop
				// and report the token found (if there was it fact one). If the LastAcceptState is -1,
				// then we never found a match and the Error Token is created. Otherwise, a new token
				// is created using the Symbol in the Accept State and all the characters that
				// comprise it.
				if (dfaState != null)
				{
					// This code checks whether the target state accepts a token. If so, it sets the
					// appropiate variables so when the algorithm in done, it can return the proper
					// token and number of characters.
					lookahead++;
					if (dfaState.m_acceptSymbol != null)
					{
						tokenSymbol = dfaState.m_acceptSymbol;
						tokenLength = lookahead - m_charIndex;
					}
					ch = m_buffer[lookahead];
					if (ch == EndOfString)
					{
						m_preserveChars = lookahead - m_charIndex;
						if (ReadBuffer() == 0)
						{
							// Found end of of stream
							lookahead = m_charIndex + m_preserveChars;
						}
						else
						{
							lookahead = m_charIndex + m_preserveChars;
							ch = m_buffer[lookahead];
						}
						m_preserveChars = 0;
					}
				}
				else
				{
					if (tokenSymbol != null)
					{
						m_token.m_symbol = tokenSymbol;
						m_token.m_length = tokenLength;
						MoveBy(tokenLength);
					}
					else
					{
						//Tokenizer cannot recognize symbol
						m_token.m_symbol = m_grammar.m_errorSymbol;
						m_token.m_length = 1;
						MoveBy(1);
					}        
					break;
				}
			}
			return m_token.m_symbol;
		}

		/// <summary>
		/// Removes current token and pops next token from the input stack.
		/// </summary>
		private void DiscardInputToken()
		{
			if (m_inputTokenCount > 0)
			{
				m_token = m_inputTokens[--m_inputTokenCount];
			}
			else
			{
				m_token.m_symbol = null;
				m_token.m_text = null;
			}
		}

		#endregion

		#region LR parser properties and methods

		/// <summary>
		/// Gets current LR state.
		/// </summary>
		public LRState CurrentLRState
		{
			get { return m_lrState; }
		}

		/// <summary>
		/// Gets current reduction syntax rule.
		/// </summary>
		public Rule ReductionRule 
		{
			get { return m_lrStack[m_lrStackIndex].m_rule; }
		}

		/// <summary>
		/// Gets number of items in the current reduction
		/// </summary>
		public int ReductionCount 
		{
			get { return m_reductionCount; }
		}

		/// <summary>
		/// Gets reduction item syntax object by its index.
		/// </summary>
		/// <param name="index">Index of reduction item.</param>
		/// <returns>Syntax object attached to reduction item.</returns>
		public object GetReductionSyntaxNode(int index)
		{
			if (index < 0 || index >= m_reductionCount)
			{
				throw new IndexOutOfRangeException();
			}
			return m_lrStack[m_lrStackIndex - m_reductionCount + index].m_token.m_syntaxNode;
		}

		/// <summary>
		/// Gets array of expected token symbols.
		/// </summary>
		public Symbol[] GetExpectedTokens() 
		{
			return m_expectedTokens;  
		}

		/// <summary>
		/// Executes next step of parser and returns parser state.
		/// </summary>
		/// <returns>Parser current state.</returns>
		public ParseMessage Parse()
		{
			if (m_token.m_symbol != null)
			{
				switch (m_token.m_symbol.m_symbolType)
				{
					case SymbolType.CommentLine:
						DiscardInputToken(); //Remove it 
						MoveToLineEnd();
						break;
					
					case SymbolType.CommentStart:
						ProcessBlockComment();
						break;
				}
			}
			while (true)
			{
				if (m_token.m_symbol == null)
				{
					//We must read a token
					Symbol readTokenSymbol = ReadToken();
					SymbolType symbolType = readTokenSymbol.m_symbolType;					
					if (m_commentLevel == 0 
						&& symbolType != SymbolType.CommentLine
						&& symbolType != SymbolType.CommentStart
						&& symbolType != SymbolType.WhiteSpace) 
					{
						return ParseMessage.TokenRead;
					}
				}
				else
				{
					//==== Normal parse mode - we have a token and we are not in comment mode
					switch (m_token.m_symbol.m_symbolType)
					{
						case SymbolType.WhiteSpace:
							DiscardInputToken();  // Discard Whitespace
							break;

						case SymbolType.CommentStart:
							m_commentLevel = 1; // Switch to block comment mode.
							return ParseMessage.CommentBlockRead;

						case SymbolType.CommentLine:
							return ParseMessage.CommentLineRead;
								
						case SymbolType.Error:
							return ParseMessage.LexicalError;
					
						default:
							//Finally, we can parse the token
							TokenParseResult parseResult = ParseToken();
						switch (parseResult)
						{
							case TokenParseResult.Accept:
								return ParseMessage.Accept;

							case TokenParseResult.InternalError:
								return ParseMessage.InternalError;

							case TokenParseResult.ReduceNormal:
								return ParseMessage.Reduction;

							case TokenParseResult.Shift: 
								//A simple shift, we must continue
								DiscardInputToken(); // Okay, remove the top token, it is on the stack
								break;

							case TokenParseResult.SyntaxError:
								return ParseMessage.SyntaxError;

							default:
								//Do nothing
								break;
						}
							break;
					}
				}
			}
		}

		private void ProcessBlockComment()
		{
			if (m_commentLevel > 0)
			{
				if (m_commentText != null)
				{
					m_commentText.Append(TokenText);
				}
				DiscardInputToken();
				while (true)
				{
					SymbolType symbolType = ReadToken().SymbolType;
					if (m_commentText != null)
					{
						m_commentText.Append(TokenText);
					}
					DiscardInputToken();
					switch (symbolType)
					{
						case SymbolType.CommentStart: 
							m_commentLevel++;
							break;

						case SymbolType.CommentEnd: 
							m_commentLevel--;
							if (m_commentLevel == 0)
							{
								// Done with comment.
								return;
							}
							break;

						case SymbolType.End:
							//TODO: replace with special exception.
							throw new Exception("CommentError");

						default:
							//Do nothing, ignore
							//The 'comment line' symbol is ignored as well
							break;
					}
				}
			}
		}

		/// <summary>
		/// Gets current comment text.
		/// </summary>
		public string CommentText
		{
			get 
			{
				if (m_token.m_symbol != null)
				{
					switch (m_token.m_symbol.m_symbolType)
					{
						case SymbolType.CommentLine:
							m_commentText = new StringBuilder();
							m_commentText.Append(TokenText);
							DiscardInputToken(); //Remove token 
							MoveToLineEnd();
							string lineComment = m_commentText.ToString();
							m_commentText = null;
							return lineComment;

						case SymbolType.CommentStart:
							m_commentText = new StringBuilder();
							ProcessBlockComment(); 
							string blockComment = m_commentText.ToString();
							m_commentText = null;
							return blockComment;
					}
				}
				return String.Empty;
			}
		}

		private TokenParseResult ParseToken()
		{
			LRStateAction stateAction = m_lrState.m_transitionVector[m_token.m_symbol.m_index];
			if (stateAction != null)
			{
				//Work - shift or reduce
				if (m_reductionCount > 0)
				{
					int newIndex = m_lrStackIndex - m_reductionCount;
					m_lrStack[newIndex] = m_lrStack[m_lrStackIndex];
					m_lrStackIndex = newIndex;
				}
				m_reductionCount = Undefined;
				switch (stateAction.Action)
				{
					case LRAction.Accept:
						m_reductionCount = 0;
						return TokenParseResult.Accept;
	
					case LRAction.Shift:
						m_lrState = m_grammar.m_lrStateTable[stateAction.m_value];
						LRStackItem nextToken = new LRStackItem();
						nextToken.m_token = m_token;
						nextToken.m_state = m_lrState;
						if (m_lrStack.Length == ++m_lrStackIndex)
						{
							LRStackItem[] larger_m_lrStack = new LRStackItem[m_lrStack.Length + MinimumLRStackSize];
							Array.Copy(m_lrStack, larger_m_lrStack, m_lrStack.Length);
							m_lrStack = larger_m_lrStack;
						}
						m_lrStack[m_lrStackIndex] = nextToken;
						return TokenParseResult.Shift;

					case LRAction.Reduce:
						//Produce a reduction - remove as many tokens as members in the rule & push a nonterminal token
						int ruleIndex = stateAction.m_value;
						Rule currentRule = m_grammar.m_ruleTable[ruleIndex];

						//======== Create Reduction
						LRStackItem head;
						TokenParseResult parseResult;
						LRState nextState;
						if (m_trimReductions && currentRule.m_hasOneNonTerminal) 
						{
							//The current rule only consists of a single nonterminal and can be trimmed from the
							//parse tree. Usually we create a new Reduction, assign it to the Data property
							//of Head and push it on the stack. However, in this case, the Data property of the
							//Head will be assigned the Data property of the reduced token (i.e. the only one
							//on the stack).
							//In this case, to save code, the value popped of the stack is changed into the head.
							head = m_lrStack[m_lrStackIndex];
							head.m_token.m_symbol = currentRule.m_nonTerminal;
							head.m_token.m_text = null;
							parseResult = TokenParseResult.ReduceEliminated;
							//========== Goto
							nextState = m_lrStack[m_lrStackIndex - 1].m_state;
						}
						else
						{
							//Build a Reduction
							head = new LRStackItem();
							head.m_rule = currentRule;
							head.m_token.m_symbol = currentRule.m_nonTerminal;
							head.m_token.m_text = null;
							m_reductionCount = currentRule.m_symbols.Length;
							parseResult = TokenParseResult.ReduceNormal;
							//========== Goto
							nextState = m_lrStack[m_lrStackIndex - m_reductionCount].m_state;
						}

						//========= If nextAction is null here, then we have an Internal Table Error!!!!
						LRStateAction nextAction = nextState.m_transitionVector[currentRule.m_nonTerminal.m_index];
						if (nextAction != null)
						{
							m_lrState = m_grammar.m_lrStateTable[nextAction.m_value];
							head.m_state = m_lrState;
							if (parseResult == TokenParseResult.ReduceNormal)
							{
								if (m_lrStack.Length == ++m_lrStackIndex)
								{
									LRStackItem[] larger_m_lrStack = new LRStackItem[m_lrStack.Length 
										+ MinimumLRStackSize];
									Array.Copy(m_lrStack, larger_m_lrStack, m_lrStack.Length);
									m_lrStack = larger_m_lrStack;
								}
								m_lrStack[m_lrStackIndex] = head;
							}
							else
							{
								m_lrStack[m_lrStackIndex] = head;
							}
							return parseResult;
						}
						else
						{
							return TokenParseResult.InternalError;
						}
				}
			}

			//=== Syntax Error! Fill Expected Tokens
			m_expectedTokens = new Symbol[m_lrState.ActionCount]; 
			int length = 0;
			for (int i = 0; i < m_lrState.ActionCount; i++)
			{
				switch (m_lrState.GetAction(i).Symbol.SymbolType)
				{
					case SymbolType.Terminal:
					case SymbolType.End:
						m_expectedTokens[length++] = m_lrState.GetAction(i).Symbol;
						break;
				}
			}
			if (length < m_expectedTokens.Length)
			{
				Symbol[] newArray = new Symbol[length];
				Array.Copy(m_expectedTokens, newArray, length);
				m_expectedTokens = newArray;
			}
			return TokenParseResult.SyntaxError;
		}

		#endregion

		#region TokenParseResult enumeration

		/// <summary>
		/// Result of parsing token.
		/// </summary>
		private enum TokenParseResult
		{
			Empty            = 0,
			Accept           = 1,
			Shift            = 2,
			ReduceNormal     = 3,
			ReduceEliminated = 4,
			SyntaxError      = 5,
			InternalError    = 6
		}

		#endregion

		#region Token struct

		/// <summary>
		/// Represents data about current token.
		/// </summary>
		private struct Token
		{
			internal Symbol m_symbol;     // Token symbol.
			internal string m_text;       // Token text.
			internal int m_start;         // Token start stream start.
			internal int m_length;        // Token length.
			internal int m_lineNumber;    // Token source line number. (1-based).
			internal int m_linePosition;  // Token position in source line (1-based).
			internal object m_syntaxNode; // Syntax node which can be attached to the token.
		}

		#endregion

		#region LRStackItem struct

		/// <summary>
		/// Represents item in the LR parsing stack.
		/// </summary>
		private struct LRStackItem
		{
			internal Token m_token;   // Token in the LR stack item.
			internal LRState m_state; // LR state associated with the item.
			internal Rule m_rule;     // Reference to a grammar rule if the item contains non-terminal.
		}

		#endregion
	}
}
