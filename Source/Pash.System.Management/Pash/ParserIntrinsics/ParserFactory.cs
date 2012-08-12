using System;
using GoldParser;
using System.IO;
using System.Reflection;

namespace Pash.ParserIntrinsics
{
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
                BinaryReader reader = new BinaryReader(new FileStream(FullCGTFilePath, FileMode.Open));
                m_grammar = new Grammar(reader);
                _init = true;
            }
        }

        public void InitializeFactoryFromResource(string resourceName)
        {
            if (!_init)
            {
                BinaryReader reader = GetResourceReader(resourceName);
                m_grammar = new Grammar(reader);
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
}

