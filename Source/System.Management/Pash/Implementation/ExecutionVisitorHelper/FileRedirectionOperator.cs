// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Irony.Parsing;
using System.Management.Automation.Language;

namespace System.Management.Pash.Implementation
{
    class FileRedirectionOperator
    {
        FileRedirectionOperator(RedirectionStream redirectionStream, bool append = false)
        {
            FromStream = redirectionStream;
            IsAppend = append;
        }

        public RedirectionStream FromStream { get; private set; }
        public bool IsAppend { get; private set; }

        public static FileRedirectionOperator Get(ParseTreeNode parseTreeNode)
        {
            switch (parseTreeNode.Token.ValueString)
            {
                case ">":
                    return new FileRedirectionOperator(RedirectionStream.Output);
                case ">>":
                    return new FileRedirectionOperator(RedirectionStream.Output, true);
                case "2>":
                    return new FileRedirectionOperator(RedirectionStream.Error);
                case "2>>":
                    return new FileRedirectionOperator(RedirectionStream.Error, true);
                default:
                    throw new NotImplementedException(parseTreeNode.ToString());
            }
        }
    }
}
