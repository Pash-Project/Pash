// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    public interface ICustomAstVisitor
    {
        object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst);
        object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst);
        object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst);
        object VisitAttribute(AttributeAst attributeAst);
        object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst);
        object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst);
        // TODO: object VisitBlockStatement(BlockStatementAst blockStatementAst);
        object VisitBreakStatement(BreakStatementAst breakStatementAst);
        object VisitCatchClause(CatchClauseAst catchClauseAst);
        object VisitCommand(CommandAst commandAst);
        object VisitCommandExpression(CommandExpressionAst commandExpressionAst);
        object VisitCommandParameter(CommandParameterAst commandParameterAst);
        object VisitConstantExpression(ConstantExpressionAst constantExpressionAst);
        object VisitContinueStatement(ContinueStatementAst continueStatementAst);
        object VisitConvertExpression(ConvertExpressionAst convertExpressionAst);
        object VisitDataStatement(DataStatementAst dataStatementAst);
        object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst);
        object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst);
        // TODO: object VisitErrorExpression(ErrorExpressionAst errorExpressionAst);
        // TODO: object VisitErrorStatement(ErrorStatementAst errorStatementAst);
        object VisitExitStatement(ExitStatementAst exitStatementAst);
        object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst);
        object VisitFileRedirection(FileRedirectionAst fileRedirectionAst);
        object VisitForEachStatement(ForEachStatementAst forEachStatementAst);
        object VisitForStatement(ForStatementAst forStatementAst);
        object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst);
        object VisitHashtable(HashtableAst hashtableAst);
        object VisitIfStatement(IfStatementAst ifStmtAst);
        object VisitIndexExpression(IndexExpressionAst indexExpressionAst);
        object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst);
        object VisitMemberExpression(MemberExpressionAst memberExpressionAst);
        object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst);
        object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst);
        object VisitNamedBlock(NamedBlockAst namedBlockAst);
        object VisitParamBlock(ParamBlockAst paramBlockAst);
        object VisitParameter(ParameterAst parameterAst);
        object VisitParenExpression(ParenExpressionAst parenExpressionAst);
        object VisitPipeline(PipelineAst pipelineAst);
        object VisitReturnStatement(ReturnStatementAst returnStatementAst);
        object VisitScriptBlock(ScriptBlockAst scriptBlockAst);
        object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst);
        object VisitStatementBlock(StatementBlockAst statementBlockAst);
        object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst);
        object VisitSubExpression(SubExpressionAst subExpressionAst);
        object VisitSwitchStatement(SwitchStatementAst switchStatementAst);
        object VisitThrowStatement(ThrowStatementAst throwStatementAst);
        object VisitTrap(TrapStatementAst trapStatementAst);
        object VisitTryStatement(TryStatementAst tryStatementAst);
        object VisitTypeConstraint(TypeConstraintAst typeConstraintAst);
        object VisitTypeExpression(TypeExpressionAst typeExpressionAst);
        object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst);
        object VisitUsingExpression(UsingExpressionAst usingExpressionAst);
        object VisitVariableExpression(VariableExpressionAst variableExpressionAst);
        object VisitWhileStatement(WhileStatementAst whileStatementAst);
    }
}
