// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Language;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pash
{
    public class ExpandableStringParser
    {
        private const int EOF = -1;

        private string _input;
        private IScriptExtent _extent;
        private List<ExpressionAst> _nestedExpressions = new List<ExpressionAst>();
        private int _currentIndex = -1;

        public ExpandableStringParser(IScriptExtent extent, string input)
        {
            _extent = extent;
            _input = input;
        }

        public ReadOnlyCollection<ExpressionAst> NestedExpressions { get { return _nestedExpressions.AsReadOnly(); } }

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
            return Char.IsLetterOrDigit((char)c);
        }

        private void AddVariable(int startIndex, int endIndex)
        {
            if (endIndex <= startIndex + 1)
                return;

            string variableName = GetText(startIndex + 1, endIndex);
            var variableAst = new VariableExpressionAst(_extent, variableName, false);
            _nestedExpressions.Add(variableAst);
        }

        private string GetText(int startIndex, int endIndex)
        {
            int length = endIndex - startIndex;
            return _input.Substring(startIndex, length);
        }

        private void ReadEscapedCharacter()
        {
            MoveToNextCharacter();
        }
    }
}

