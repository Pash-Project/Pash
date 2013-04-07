using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pash.ParserIntrinsics
{
    // A non-grammar terminal that controls how the parser handles line continuation.
    class BacktickLineContinuationTerminal : LineContinuationTerminal
    {
        ////            `   (The backtick character U+0060) followed by new_line_character
        ////        new_line_character:
        ////            Carriage return character (U+000D)
        ////            Line feed character (U+000A)
        ////            Carriage return character (U+000D) followed by line feed character (U+000A)
        public BacktickLineContinuationTerminal()
            : base(typeof(BacktickLineContinuationTerminal).Name, "`")
        {
            this.LineTerminators = "\r\n";
        }
    }
}
