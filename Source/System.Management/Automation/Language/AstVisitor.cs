// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    public abstract class AstVisitor
    {
        public virtual AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitAttribute(AttributeAst attributeAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst) { return AstVisitAction.Continue; }
        // TODO: public virtual AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst){return  AstVisitAction.Continue; }
        public virtual AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitCommand(CommandAst commandAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst) { return AstVisitAction.Continue; }
        // TODO: public virtual AstVisitAction VisitErrorExpression(ErrorExpressionAst errorExpressionAst){return  AstVisitAction.Continue; }
        // TODO: public virtual AstVisitAction VisitErrorStatement(ErrorStatementAst errorStatementAst){return  AstVisitAction.Continue; }
        public virtual AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitExpandableStringWithSubexpressionExpression(ExpandableStringWithSubexpressionExpressionAst expandableStringWitSubexpressionExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitForStatement(ForStatementAst forStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitHashtable(HashtableAst hashtableAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitIfStatement(IfStatementAst ifStmtAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitMergingRedirection(MergingRedirectionAst redirectionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitParameter(ParameterAst parameterAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitPipeline(PipelineAst pipelineAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitTrap(TrapStatementAst trapStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst) { return AstVisitAction.Continue; }
        public virtual AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst) { return AstVisitAction.Continue; }
    }
}
