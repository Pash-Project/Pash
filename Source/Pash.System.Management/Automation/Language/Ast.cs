using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
                        foreach (var item in nextItem.Children)
                        {
                            queue.Enqueue(item);
                        }
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

        [DebuggerStepThrough]
        static AstVisitAction DispatchVisitor(AstVisitor astVisitor, Ast nextItem)
        {
            if (nextItem is ArrayExpressionAst) return astVisitor.VisitArrayExpression((ArrayExpressionAst)nextItem);
            if (nextItem is ArrayLiteralAst) return astVisitor.VisitArrayLiteral((ArrayLiteralAst)nextItem);
            if (nextItem is AssignmentStatementAst) return astVisitor.VisitAssignmentStatement((AssignmentStatementAst)nextItem);
            if (nextItem is AttributeAst) return astVisitor.VisitAttribute((AttributeAst)nextItem);
            if (nextItem is AttributedExpressionAst) return astVisitor.VisitAttributedExpression((AttributedExpressionAst)nextItem);
            if (nextItem is BinaryExpressionAst) return astVisitor.VisitBinaryExpression((BinaryExpressionAst)nextItem);
            if (nextItem is BreakStatementAst) return astVisitor.VisitBreakStatement((BreakStatementAst)nextItem);
            if (nextItem is CatchClauseAst) return astVisitor.VisitCatchClause((CatchClauseAst)nextItem);
            if (nextItem is CommandAst) return astVisitor.VisitCommand((CommandAst)nextItem);
            if (nextItem is CommandExpressionAst) return astVisitor.VisitCommandExpression((CommandExpressionAst)nextItem);
            if (nextItem is CommandParameterAst) return astVisitor.VisitCommandParameter((CommandParameterAst)nextItem);
            if (nextItem is ConstantExpressionAst) return astVisitor.VisitConstantExpression((ConstantExpressionAst)nextItem);
            if (nextItem is ContinueStatementAst) return astVisitor.VisitContinueStatement((ContinueStatementAst)nextItem);
            if (nextItem is ConvertExpressionAst) return astVisitor.VisitConvertExpression((ConvertExpressionAst)nextItem);
            if (nextItem is DataStatementAst) return astVisitor.VisitDataStatement((DataStatementAst)nextItem);
            if (nextItem is DoUntilStatementAst) return astVisitor.VisitDoUntilStatement((DoUntilStatementAst)nextItem);
            if (nextItem is DoWhileStatementAst) return astVisitor.VisitDoWhileStatement((DoWhileStatementAst)nextItem);
            if (nextItem is ExitStatementAst) return astVisitor.VisitExitStatement((ExitStatementAst)nextItem);
            if (nextItem is ExpandableStringExpressionAst) return astVisitor.VisitExpandableStringExpression((ExpandableStringExpressionAst)nextItem);
            if (nextItem is FileRedirectionAst) return astVisitor.VisitFileRedirection((FileRedirectionAst)nextItem);
            if (nextItem is ForEachStatementAst) return astVisitor.VisitForEachStatement((ForEachStatementAst)nextItem);
            if (nextItem is ForStatementAst) return astVisitor.VisitForStatement((ForStatementAst)nextItem);
            if (nextItem is FunctionDefinitionAst) return astVisitor.VisitFunctionDefinition((FunctionDefinitionAst)nextItem);
            if (nextItem is HashtableAst) return astVisitor.VisitHashtable((HashtableAst)nextItem);
            if (nextItem is IfStatementAst) return astVisitor.VisitIfStatement((IfStatementAst)nextItem);
            if (nextItem is IndexExpressionAst) return astVisitor.VisitIndexExpression((IndexExpressionAst)nextItem);
            if (nextItem is InvokeMemberExpressionAst) return astVisitor.VisitInvokeMemberExpression((InvokeMemberExpressionAst)nextItem);
            if (nextItem is MemberExpressionAst) return astVisitor.VisitMemberExpression((MemberExpressionAst)nextItem);
            if (nextItem is MergingRedirectionAst) return astVisitor.VisitMergingRedirection((MergingRedirectionAst)nextItem);
            if (nextItem is NamedAttributeArgumentAst) return astVisitor.VisitNamedAttributeArgument((NamedAttributeArgumentAst)nextItem);
            if (nextItem is NamedBlockAst) return astVisitor.VisitNamedBlock((NamedBlockAst)nextItem);
            if (nextItem is ParamBlockAst) return astVisitor.VisitParamBlock((ParamBlockAst)nextItem);
            if (nextItem is ParameterAst) return astVisitor.VisitParameter((ParameterAst)nextItem);
            if (nextItem is ParenExpressionAst) return astVisitor.VisitParenExpression((ParenExpressionAst)nextItem);
            if (nextItem is PipelineAst) return astVisitor.VisitPipeline((PipelineAst)nextItem);
            if (nextItem is ReturnStatementAst) return astVisitor.VisitReturnStatement((ReturnStatementAst)nextItem);
            if (nextItem is ScriptBlockAst) return astVisitor.VisitScriptBlock((ScriptBlockAst)nextItem);
            if (nextItem is ScriptBlockExpressionAst) return astVisitor.VisitScriptBlockExpression((ScriptBlockExpressionAst)nextItem);
            if (nextItem is StatementBlockAst) return astVisitor.VisitStatementBlock((StatementBlockAst)nextItem);
            if (nextItem is StringConstantExpressionAst) return astVisitor.VisitStringConstantExpression((StringConstantExpressionAst)nextItem);
            if (nextItem is SubExpressionAst) return astVisitor.VisitSubExpression((SubExpressionAst)nextItem);
            if (nextItem is SwitchStatementAst) return astVisitor.VisitSwitchStatement((SwitchStatementAst)nextItem);
            if (nextItem is ThrowStatementAst) return astVisitor.VisitThrowStatement((ThrowStatementAst)nextItem);
            if (nextItem is TrapStatementAst) return astVisitor.VisitTrap((TrapStatementAst)nextItem);
            if (nextItem is TryStatementAst) return astVisitor.VisitTryStatement((TryStatementAst)nextItem);
            if (nextItem is TypeConstraintAst) return astVisitor.VisitTypeConstraint((TypeConstraintAst)nextItem);
            if (nextItem is TypeExpressionAst) return astVisitor.VisitTypeExpression((TypeExpressionAst)nextItem);
            if (nextItem is UnaryExpressionAst) return astVisitor.VisitUnaryExpression((UnaryExpressionAst)nextItem);
            if (nextItem is UsingExpressionAst) return astVisitor.VisitUsingExpression((UsingExpressionAst)nextItem);
            if (nextItem is VariableExpressionAst) return astVisitor.VisitVariableExpression((VariableExpressionAst)nextItem);
            if (nextItem is WhileStatementAst) return astVisitor.VisitWhileStatement((WhileStatementAst)nextItem);
            else throw new InvalidOperationException(nextItem.ToString()); // did I miss one?
        }

        public object Visit(ICustomAstVisitor astVisitor)
        {
            throw new NotImplementedException(this.ToString());
        }
    }
}
