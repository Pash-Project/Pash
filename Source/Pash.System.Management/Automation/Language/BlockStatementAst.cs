#region Assembly System.Management.Automation.dll, v4.0.30319
// C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll
#endregion

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class BlockStatementAst : StatementAst
    {
        public BlockStatementAst(IScriptExtent extent, Token kind, StatementBlockAst body)
            : base(extent)
        {
        }

        public StatementBlockAst Body { get { throw new NotImplementedException(this.ToString()); } }
        public Token Kind { get { throw new NotImplementedException(this.ToString()); } }
    }
}
