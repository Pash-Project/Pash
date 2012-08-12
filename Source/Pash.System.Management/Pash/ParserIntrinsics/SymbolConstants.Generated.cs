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
    enum SymbolConstants : int
    {
        Symbol_Eof = 0, // (EOF)
        Symbol_Error = 1, // (Error)
        Symbol_Whitespace = 2, // (Whitespace)
        Symbol_Commentend = 3, // (Comment End)
        Symbol_Commentline = 4, // (Comment Line)
        Symbol_Commentstart = 5, // (Comment Start)
        Symbol_Dollarlparan = 6, // $(
        Symbol_Lparan = 7, // (
        Symbol_Rparan = 8, // )
        Symbol_Pipe = 9, // |
        Symbol_Additionoperatortoken = 10, // AdditionOperatorToken
        Symbol_Anywordtoken = 11, // AnyWordToken
        Symbol_Assignmentoperatortoken = 12, // AssignmentOperatorToken
        Symbol_Commatoken = 13, // CommaToken
        Symbol_Commenttoken = 14, // CommentToken
        Symbol_Execcall = 15, // ExecCall
        Symbol_Newline = 16, // NewLine
        Symbol_Numbertoken = 17, // NumberToken
        Symbol_Parametertoken = 18, // ParameterToken
        Symbol_Rangeoperatortoken = 19, // RangeOperatorToken
        Symbol_Stringtoken = 20, // StringToken
        Symbol_Variabletoken = 21, // VariableToken
        Symbol_Addexpressionrule = 22, // <addExpressionRule>
        Symbol_Arrayliteralrule = 23, // <arrayLiteralRule>
        Symbol_Assignmentstatementrule = 24, // <assignmentStatementRule>
        Symbol_Bitwiseexpressionrule = 25, // <bitwiseExpressionRule>
        Symbol_Cmdletcall = 26, // <cmdletCall>
        Symbol_Cmdletname = 27, // <cmdletName>
        Symbol_Cmletparamslist = 28, // <cmletParamsList>
        Symbol_Comparisonexpressionrule = 29, // <comparisonExpressionRule>
        Symbol_Expressionrule = 30, // <expressionRule>
        Symbol_Formatexpressionrule = 31, // <formatExpressionRule>
        Symbol_Logicalexpressionrule = 32, // <logicalExpressionRule>
        Symbol_Lvalue = 33, // <lvalue>
        Symbol_Lvalueexpression = 34, // <lvalueExpression>
        Symbol_Multiplyexpressionrule = 35, // <multiplyExpressionRule>
        Symbol_Parameterargumenttoken = 36, // <ParameterArgumentToken>
        Symbol_Pipelinerule = 37, // <pipelineRule>
        Symbol_Postfixoperatorrule = 38, // <postfixOperatorRule>
        Symbol_Propertyorarrayreferencerule = 39, // <propertyOrArrayReferenceRule>
        Symbol_Rangeexpressionrule = 40, // <rangeExpressionRule>
        Symbol_Simplelvalue = 41, // <simpleLvalue>
        Symbol_Statementlistrule = 42, // <statementListRule>
        Symbol_Statementrule = 43, // <statementRule>
        Symbol_Statementseparatortoken = 44, // <statementSeparatorToken>
        Symbol_Valuerule = 45  // <valueRule>
    };
}
