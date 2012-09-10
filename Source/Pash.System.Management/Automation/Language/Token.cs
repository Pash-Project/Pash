#region Assembly System.Management.Automation.dll, v4.0.30319
// C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll
#endregion

using System;

namespace System.Management.Automation.Language
{
    public class Token
    {
        public IScriptExtent Extent { get { throw new NotImplementedException(this.ToString()); } }
        public bool HasError { get { throw new NotImplementedException(this.ToString()); } }
        public TokenKind Kind { get { throw new NotImplementedException(this.ToString()); } }
        public string Text { get { throw new NotImplementedException(this.ToString()); } }
        public TokenFlags TokenFlags { get; internal set; }

        public override string ToString();
    }
}
