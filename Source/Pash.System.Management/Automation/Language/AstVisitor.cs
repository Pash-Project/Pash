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
