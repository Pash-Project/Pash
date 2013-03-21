// Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

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
