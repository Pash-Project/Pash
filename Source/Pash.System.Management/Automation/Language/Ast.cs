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
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Extensions.Queue;
using Extensions.Reflection;

namespace System.Management.Automation.Language
{
    static class _
    {
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            return new ReadOnlyCollection<T>(new List<T>(source ?? new T[] { }));
        }

        public static AstVisitAction Visit(this AstVisitor astVisitor, Ast ast)
        {
            var dispatchMethodInfos = from dmi in astVisitor.GetType().GetMethods()
                                      where dmi.Name.StartsWith("Visit")
                                      where dmi.GetParameters()[0].ParameterType.IsAssignableFrom(ast)
                                      select dmi;

            // TODO: Find out what PowerShell does when there's more than one match.
            //
            // e.g., `StringConstantExpression` is also a `ConstantExpression`
            if (dispatchMethodInfos.Any())
            {
                return (AstVisitAction)dispatchMethodInfos.First().Invoke(astVisitor, new[] { ast });
            }

            else throw new InvalidOperationException(ast.ToString());
        }
    }

    public abstract class Ast
    {
        protected Ast(IScriptExtent extent) { this.Extent = extent; }

        public IScriptExtent Extent { get; private set; }
        public Ast Parent { get; private set; }

        public Ast Find(Func<Ast, bool> predicate, bool searchNestedScriptBlocks) { throw new NotImplementedException(this.ToString()); }

        public IEnumerable<Ast> FindAll(Func<Ast, bool> predicate, bool searchNestedScriptBlocks) { throw new NotImplementedException(this.ToString()); }

        public override string ToString() { throw new NotImplementedException(); }

        internal virtual IEnumerable<Ast> Children { get { yield break; } }

        public void Visit(AstVisitor astVisitor)
        {
            Queue<Ast> queue = new Queue<Ast>();

            queue.Enqueue(this);

            while (queue.Any())
            {
                var nextAst = queue.Dequeue();
                AstVisitAction astVisitAction = astVisitor.Visit(nextAst);

                switch (astVisitAction)
                {
                    case AstVisitAction.Continue:
                        queue.EnqueueAll(nextAst.Children);
                        break;

                    case AstVisitAction.SkipChildren:
                        break;

                    case AstVisitAction.StopVisit:
                        return;

                    default:
                        throw new InvalidOperationException(astVisitAction.ToString());
                }
            }
        }

        public object Visit(ICustomAstVisitor astVisitor)
        {
            throw new NotImplementedException(this.ToString());
        }
    }
}
