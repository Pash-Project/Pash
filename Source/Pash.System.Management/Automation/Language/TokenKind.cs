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
    public enum TokenKind
    {
        Unknown = 0,
        Variable = 1,
        SplattedVariable = 2,
        Parameter = 3,
        Number = 4,
        Label = 5,
        Identifier = 6,
        Generic = 7,
        NewLine = 8,
        LineContinuation = 9,
        Comment = 10,
        EndOfInput = 11,
        StringLiteral = 12,
        StringExpandable = 13,
        HereStringLiteral = 14,
        HereStringExpandable = 15,
        LParen = 16,
        RParen = 17,
        LCurly = 18,
        RCurly = 19,
        LBracket = 20,
        RBracket = 21,
        AtParen = 22,
        AtCurly = 23,
        DollarParen = 24,
        Semi = 25,
        AndAnd = 26,
        OrOr = 27,
        Ampersand = 28,
        Pipe = 29,
        Comma = 30,
        MinusMinus = 31,
        PlusPlus = 32,
        DotDot = 33,
        ColonColon = 34,
        Dot = 35,
        Exclaim = 36,
        Multiply = 37,
        Divide = 38,
        Rem = 39,
        Plus = 40,
        Minus = 41,
        Equals = 42,
        PlusEquals = 43,
        MinusEquals = 44,
        MultiplyEquals = 45,
        DivideEquals = 46,
        RemainderEquals = 47,
        Redirection = 48,
        RedirectInStd = 49,
        Format = 50,
        Not = 51,
        Bnot = 52,
        And = 53,
        Or = 54,
        Xor = 55,
        Band = 56,
        Bor = 57,
        Bxor = 58,
        Join = 59,
        Ieq = 60,
        Ine = 61,
        Ige = 62,
        Igt = 63,
        Ilt = 64,
        Ile = 65,
        Ilike = 66,
        Inotlike = 67,
        Imatch = 68,
        Inotmatch = 69,
        Ireplace = 70,
        Icontains = 71,
        Inotcontains = 72,
        Iin = 73,
        Inotin = 74,
        Isplit = 75,
        Ceq = 76,
        Cne = 77,
        Cge = 78,
        Cgt = 79,
        Clt = 80,
        Cle = 81,
        Clike = 82,
        Cnotlike = 83,
        Cmatch = 84,
        Cnotmatch = 85,
        Creplace = 86,
        Ccontains = 87,
        Cnotcontains = 88,
        Cin = 89,
        Cnotin = 90,
        Csplit = 91,
        Is = 92,
        IsNot = 93,
        As = 94,
        PostfixPlusPlus = 95,
        PostfixMinusMinus = 96,
        Shl = 97,
        Shr = 98,
        Begin = 119,
        Break = 120,
        Catch = 121,
        Class = 122,
        Continue = 123,
        Data = 124,
        Define = 125,
        Do = 126,
        Dynamicparam = 127,
        Else = 128,
        ElseIf = 129,
        End = 130,
        Exit = 131,
        Filter = 132,
        Finally = 133,
        For = 134,
        Foreach = 135,
        From = 136,
        Function = 137,
        If = 138,
        In = 139,
        Param = 140,
        Process = 141,
        Return = 142,
        Switch = 143,
        Throw = 144,
        Trap = 145,
        Try = 146,
        Until = 147,
        Using = 148,
        Var = 149,
        While = 150,
        Workflow = 151,
        Parallel = 152,
        Sequence = 153,
        InlineScript = 154,
    }
}
