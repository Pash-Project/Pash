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
    using Nodes;

    public class PashParser
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
}
