// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class ErrorCategoryInfo
    {
        internal ErrorCategoryInfo(Exception exception, ErrorCategory category)
        {
            this.Category = category;
            this.Activity = string.Empty;
            this.Reason = exception.GetType().Name;
            this.TargetName = string.Empty;
            this.TargetType = string.Empty;
        }

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
