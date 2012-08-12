using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    enum RuleConstants : int
    {
        Rule_Statementseparatortoken_Newline = 0, // <statementSeparatorToken> ::= NewLine <statementSeparatorToken>
        Rule_Statementseparatortoken = 1, // <statementSeparatorToken> ::= 
        Rule_Statementlistrule = 2, // <statementListRule> ::= <statementRule>
        Rule_Statementlistrule2 = 3, // <statementListRule> ::= <statementRule> <statementSeparatorToken> <statementListRule>
        Rule_Statementrule = 4, // <statementRule> ::= <pipelineRule>
        Rule_Statementrule_Commenttoken = 5, // <statementRule> ::= CommentToken
        Rule_Pipelinerule = 6, // <pipelineRule> ::= <cmdletCall>
        Rule_Pipelinerule_Pipe = 7, // <pipelineRule> ::= <cmdletCall> | <pipelineRule>
        Rule_Pipelinerule2 = 8, // <pipelineRule> ::= <assignmentStatementRule>
        Rule_Pipelinerule_Pipe2 = 9, // <pipelineRule> ::= <assignmentStatementRule> | <pipelineRule>
        Rule_Assignmentstatementrule_Assignmentoperatortoken = 10, // <assignmentStatementRule> ::= <lvalueExpression> AssignmentOperatorToken <pipelineRule>
        Rule_Lvalueexpression = 11, // <lvalueExpression> ::= <lvalue>
        Rule_Lvalue = 12, // <lvalue> ::= <simpleLvalue>
        Rule_Simplelvalue_Variabletoken = 13, // <simpleLvalue> ::= VariableToken
        Rule_Parameterargumenttoken = 14, // <ParameterArgumentToken> ::= <valueRule>
        Rule_Parameterargumenttoken_Anywordtoken = 15, // <ParameterArgumentToken> ::= AnyWordToken
        Rule_Parameterargumenttoken_Parametertoken = 16, // <ParameterArgumentToken> ::= ParameterToken
        Rule_Cmletparamslist = 17, // <cmletParamsList> ::= <ParameterArgumentToken> <cmletParamsList>
        Rule_Cmletparamslist2 = 18, // <cmletParamsList> ::= <ParameterArgumentToken>
        Rule_Cmdletname_Anywordtoken = 19, // <cmdletName> ::= AnyWordToken
        Rule_Cmdletcall_Execcall = 20, // <cmdletCall> ::= ExecCall <cmdletName> <cmletParamsList>
        Rule_Cmdletcall_Execcall2 = 21, // <cmdletCall> ::= ExecCall <cmdletName>
        Rule_Cmdletcall = 22, // <cmdletCall> ::= <cmdletName> <cmletParamsList>
        Rule_Cmdletcall2 = 23, // <cmdletCall> ::= <cmdletName>
        Rule_Cmdletcall3 = 24, // <cmdletCall> ::= <expressionRule>
        Rule_Expressionrule = 25, // <expressionRule> ::= <logicalExpressionRule>
        Rule_Logicalexpressionrule = 26, // <logicalExpressionRule> ::= <bitwiseExpressionRule>
        Rule_Bitwiseexpressionrule = 27, // <bitwiseExpressionRule> ::= <comparisonExpressionRule>
        Rule_Comparisonexpressionrule = 28, // <comparisonExpressionRule> ::= <addExpressionRule>
        Rule_Addexpressionrule = 29, // <addExpressionRule> ::= <multiplyExpressionRule>
        Rule_Addexpressionrule_Additionoperatortoken = 30, // <addExpressionRule> ::= <multiplyExpressionRule> AdditionOperatorToken <addExpressionRule>
        Rule_Multiplyexpressionrule = 31, // <multiplyExpressionRule> ::= <formatExpressionRule>
        Rule_Formatexpressionrule = 32, // <formatExpressionRule> ::= <rangeExpressionRule>
        Rule_Rangeexpressionrule = 33, // <rangeExpressionRule> ::= <arrayLiteralRule>
        Rule_Rangeexpressionrule_Rangeoperatortoken = 34, // <rangeExpressionRule> ::= <arrayLiteralRule> RangeOperatorToken <rangeExpressionRule>
        Rule_Arrayliteralrule = 35, // <arrayLiteralRule> ::= <postfixOperatorRule>
        Rule_Arrayliteralrule_Commatoken = 36, // <arrayLiteralRule> ::= <postfixOperatorRule> CommaToken <arrayLiteralRule>
        Rule_Postfixoperatorrule = 37, // <postfixOperatorRule> ::= <propertyOrArrayReferenceRule>
        Rule_Propertyorarrayreferencerule = 38, // <propertyOrArrayReferenceRule> ::= <valueRule>
        Rule_Valuerule_Stringtoken = 39, // <valueRule> ::= StringToken
        Rule_Valuerule_Variabletoken = 40, // <valueRule> ::= VariableToken
        Rule_Valuerule_Numbertoken = 41, // <valueRule> ::= NumberToken
        Rule_Valuerule_Dollarlparan_Rparan = 42, // <valueRule> ::= $( <statementRule> )
        Rule_Valuerule_Lparan_Rparan = 43  // <valueRule> ::= ( <assignmentStatementRule> )
    };
}
