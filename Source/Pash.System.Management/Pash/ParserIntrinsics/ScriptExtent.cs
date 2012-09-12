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
            get { throw new NotImplementedException(); }
        }

        int IScriptExtent.EndLineNumber
        {
            get { throw new NotImplementedException(); }
        }

        int IScriptExtent.EndOffset
        {
            get { throw new NotImplementedException(); }
        }

        IScriptPosition IScriptExtent.EndScriptPosition
        {
            get { throw new NotImplementedException(); }
        }

        string IScriptExtent.File
        {
            get { throw new NotImplementedException(); }
        }

        int IScriptExtent.StartColumnNumber
        {
            get { throw new NotImplementedException(); }
        }

        int IScriptExtent.StartLineNumber
        {
            get { throw new NotImplementedException(); }
        }

        int IScriptExtent.StartOffset
        {
            get { throw new NotImplementedException(); }
        }

        IScriptPosition IScriptExtent.StartScriptPosition
        {
            get { throw new NotImplementedException(); }
        }

        string IScriptExtent.Text
        {
            get { return this._parseTreeNode.FindTokenAndGetText(); }
        }
    }
}
