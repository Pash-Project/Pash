using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class ErrorCategoryInfo
    {
        public string Activity { get; set; }
        public ErrorCategory Category { get; private set; }
        public string Reason { get; set; }
        public string TargetName { get; set; }
        public string TargetType { get; set; }
        /*
        public string GetMessage();
        public string GetMessage(CultureInfo uiCultureInfo);
        public override string ToString();
        */
    }
}
