#region Assembly System.Management.Automation.dll, v4.0.30319
// C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ErrorStatementAst : PipelineBaseAst
    {
        public ReadOnlyCollection<Ast> Bodies { get { throw new NotImplementedException(this.ToString()); } }
        public ReadOnlyCollection<Ast> Conditions { get { throw new NotImplementedException(this.ToString()); } }
        public Dictionary<string, Tuple<Token, Ast>> Flags { get { throw new NotImplementedException(this.ToString()); } }
        public Token Kind { get { throw new NotImplementedException(this.ToString()); } }
        public ReadOnlyCollection<Ast> NestedAst { get { throw new NotImplementedException(this.ToString()); } }
    }
}
