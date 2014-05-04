using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class TableFormatGeneratorOptions : FormatGeneratorOptions
    {
        public bool AutoSize { get; set; }

        public bool HideTableHeaders { get; set; }

        public bool Wrap { get; set; }

        internal TableFormatGeneratorOptions()
        {
        }

        internal TableFormatGeneratorOptions(FormatGeneratorOptions options)
        {
            Expand = options.Expand;
            Force = options.Force;
            GroupBy = options.GroupBy;
            DisplayError = options.DisplayError;
            ShowError = options.ShowError;
            View = options.View;
            Properties = options.Properties;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as TableFormatGeneratorOptions;
            if (other == null)
            {
                return false;
            }
            return (
                base.Equals(other) &&
                AutoSize.Equals(other.AutoSize) &&
                HideTableHeaders.Equals(other.HideTableHeaders) &&
                Wrap.Equals(other.Wrap)
                );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ AutoSize.GetHashCode() ^ HideTableHeaders.GetHashCode() ^ Wrap.GetHashCode();
        }
    }
}

