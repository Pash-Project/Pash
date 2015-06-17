// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Language;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Irony.Parsing;
using Pash.ParserIntrinsics;

namespace Pash
{
    public class ExpandableStringParser
    {
        private const int EOF = -1;

        private string _input;
        private IScriptExtent _extent;
        private int _currentIndex = -1;

        public ExpandableStringParser(IScriptExtent extent, string input)
        {
            NestedExpressions = new List<ExpressionAst>();
            _extent = extent;
            _input = input;
        }

        public IList<ExpressionAst> NestedExpressions { get; private set; }

        public void Parse()
        {
            while (MoveToNextCharacter())
            {
                switch (CurrentCharacter())
                {
                    case '$':
                        ReadVariable();
                        break;

                    case '`':
                        ReadEscapedCharacter();
                        break;
                }
            }
        }

        private bool MoveToNextCharacter()
        {
            _currentIndex++;
            return !EndOfString();
        }

        private bool EndOfString()
        {
            return _currentIndex >= _input.Length;
        }

        private int CurrentCharacter()
        {
            return _input[_currentIndex];
        }

        private int Peek()
        {
            int peekIndex = _currentIndex + 1;
            if (peekIndex >= _input.Length)
            {
                return EOF;
            }

            return _input[peekIndex];
        }

        private void ReadVariable()
        {
            int startIndex = _currentIndex;

            if (!MoveToNextCharacter())
                return;

            while (IsValidVariableNameCharacter(Peek()))
            {
                MoveToNextCharacter();
            }

            AddVariable(startIndex, _currentIndex + 1);
        }

        private bool IsValidVariableNameCharacter(int c)
        {
            return Char.IsLetterOrDigit((char)c) || (c == ':'); // colon is for scope pattern
        }

        private void AddVariable(int startIndex, int endIndex)
        {
            if (endIndex <= startIndex + 1)
                return;

            string variableName = GetText(startIndex + 1, endIndex);
            IScriptExtent variableExtent = GetScriptExtent(startIndex, endIndex);
            var variableAst = new VariableExpressionAst(variableExtent, variableName, false);
            NestedExpressions.Add(variableAst);
        }

        private string GetText(int startIndex, int endIndex)
        {
            int length = endIndex - startIndex;
            return _input.Substring(startIndex, length);
        }

        private IScriptExtent GetScriptExtent(int startIndex, int endIndex)
        {
            SourceLocation location = GetSourceLocation(startIndex, endIndex);
            var token = new Token(new Terminal(""), location, GetText(startIndex, endIndex), null);
            var parseTreeNode = new ParseTreeNode(token);
            return new ScriptExtent(parseTreeNode);
        }

        private SourceLocation GetSourceLocation(int startIndex, int endIndex)
        {
            return new SourceLocation(
                _extent.StartOffset + startIndex + 1,
                _extent.StartLineNumber - 1,
                _extent.StartColumnNumber + startIndex);
        }

        private void ReadEscapedCharacter()
        {
            MoveToNextCharacter();
        }
    }
}

