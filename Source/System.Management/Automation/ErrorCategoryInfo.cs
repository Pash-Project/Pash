// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

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

        public string GetMessage()
        {
            return ToString();
        }

        public string GetMessage(CultureInfo uiCultureInfo)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return String.Format("{0}: ({1}:{2}) [{3}], {4}",
                Category,
                TargetName,
                TargetType,
                Activity,
                Reason);
        }
    }
}
