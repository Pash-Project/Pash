// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Language;
using Irony.Parsing;

namespace Pash.ParserIntrinsics
{
    class ScriptExtent : IScriptExtent
    {
        readonly ParseTreeNode _parseTreeNode;

        public ScriptExtent(ParseTreeNode parseTreeNode)
        {
            this._parseTreeNode = parseTreeNode;
        }

        int IScriptExtent.EndColumnNumber
        {
            get { return Location.Column + Span.Length + 1; }
        }

        int IScriptExtent.EndLineNumber
        {
            get { return Location.Line + 1; }
        }

        int IScriptExtent.EndOffset
        {
            get { return Span.EndPosition; }
        }

        IScriptPosition IScriptExtent.EndScriptPosition
        {
            get { throw new NotImplementedException(); }
        }

        string IScriptExtent.File
        {
            get { return null; }
        }

        int IScriptExtent.StartColumnNumber
        {
            get { return Location.Column + 1; }
        }

        int IScriptExtent.StartLineNumber
        {
            get { return Location.Line + 1; }
        }

        int IScriptExtent.StartOffset
        {
            get { return Location.Position; }
        }

        IScriptPosition IScriptExtent.StartScriptPosition
        {
            get { throw new NotImplementedException(); }
        }

        string IScriptExtent.Text
        {
            get { return this._parseTreeNode.FindTokenAndGetText() ?? string.Empty; }
        }

        private SourceLocation Location
        {
            get { return Span.Location; }
        }

        private SourceSpan Span
        {
            get { return _parseTreeNode.Span; }
        }
    }
}
