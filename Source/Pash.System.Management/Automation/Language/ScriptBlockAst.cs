// Copyright (C) Pash Contributors. All Rights Reserved. See https://github.com/Pash-Project/Pash/

#region BSD License
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, (either expressed or implied, of the FreeBSD Project.
#endregion

#region GPL License
// This file is part of Pash.
//
// Pash is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Pash is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along
// with Pash.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    // "The ast representing a begin, process, end, or dynamic parameter block in a scriptblock. This ast is used even when the block is unnamed, in which case the block is either an end block (for functions) or process block (for filters)."
    public class ScriptBlockAst : Ast
    {
        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter)
            : base(extent)
        {
            this.EndBlock = new NamedBlockAst(extent, TokenKind.End, statements, true);
            if (isFilter) throw new NotImplementedException(this.ToString());
        }

        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, NamedBlockAst beginBlock, NamedBlockAst processBlock, NamedBlockAst endBlock, NamedBlockAst dynamicParamBlock)
            : base(extent)
        {
            this.BeginBlock = beginBlock;
            this.DynamicParamBlock = dynamicParamBlock;
            this.EndBlock = endBlock;
            this.ParamBlock = paramBlock;
            this.ProcessBlock = processBlock;
        }

        public NamedBlockAst BeginBlock { get; private set; }
        public NamedBlockAst DynamicParamBlock { get; private set; }
        public NamedBlockAst EndBlock { get; private set; }
        public ParamBlockAst ParamBlock { get; private set; }
        public NamedBlockAst ProcessBlock { get; private set; }
        //public ScriptRequirements ScriptRequirements { get; internal set; }

        //public CommentHelpInfo GetHelpContent();
        public ScriptBlock GetScriptBlock()
        {
            return new ScriptBlock(this);
        }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                if (this.BeginBlock != null) yield return this.BeginBlock;
                if (this.DynamicParamBlock != null) yield return this.DynamicParamBlock;
                if (this.EndBlock != null) yield return this.EndBlock;
                if (this.ParamBlock != null) yield return this.ParamBlock;
                if (this.ProcessBlock != null) yield return this.ProcessBlock;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.EndBlock.ToString();
        }
    }
}
