using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extensions.Irony
{
    static class _
    {
        // Remove this when it appears in Irony (if ever). See https://irony.codeplex.com/discussions/438247#post1026398
        /// <summary>
        /// Replace the current token with a token based on <paramref name="newTerminal"/>, while keeping everything else
        /// the same as the current token.
        /// </summary>
        /// <param name="newTerminal"></param>
        public static void ReplaceToken(this ValidateTokenEventArgs @this, Terminal newTerminal)
        {
            @this.ReplaceToken(new Token(newTerminal, @this.Context.CurrentToken.Location, @this.Context.CurrentToken.Text, @this.Context.CurrentToken.Value));
        }
    }
}
