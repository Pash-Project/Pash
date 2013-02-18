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
                var nextItem = queue.Dequeue();
                AstVisitAction astVisitAction = DispatchVisitor(astVisitor, nextItem);

                switch (astVisitAction)
                {
                    case AstVisitAction.Continue:
                        queue.EnqueueAll(nextItem.Children);
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

        static AstVisitAction DispatchVisitor(AstVisitor astVisitor, Ast nextItem)
        {
            var dispatchMethodInfos = from dmi in astVisitor.GetType().GetMethods()
                                      where dmi.Name.StartsWith("Visit")
                                      where dmi.GetParameters()[0].ParameterType.IsAssignableFrom(nextItem)
                                      select dmi;

            if (dispatchMethodInfos.Any())
            {
                return (AstVisitAction)dispatchMethodInfos.First().Invoke(astVisitor, new[] { nextItem });
            }

            else throw new InvalidOperationException(nextItem.ToString());
        }

        public object Visit(ICustomAstVisitor astVisitor)
        {
            throw new NotImplementedException(this.ToString());
        }
    }
}
