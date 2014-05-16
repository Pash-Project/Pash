using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class FormatGeneratorOptions
    {
        internal enum ExpandingOptions
        {
            CoreOnly,
            EnumOnly,
            Both
        }

        internal ExpandingOptions Expand { get; set; }

        internal bool Force { get; set; }

        internal object GroupBy { get; set; }

        internal bool DisplayError { get; set; }

        internal bool ShowError { get; set; }

        internal string View { get; set; }

        internal object[] Properties { get; set; }

        internal FormatGeneratorOptions()
        {
            Expand = ExpandingOptions.EnumOnly;
            View = "";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as FormatGeneratorOptions;
            if (other == null)
            {
                throw new PSInvalidOperationException("Object to be compared is of different type "
                                                      + obj.GetType().Name);
            }
            if (((Properties == null) != (other.Properties == null)) ||
                ((GroupBy == null) != (other.GroupBy == null))
               )
            {
                return false;
            }
            return (
                Expand.Equals(other.Expand) &&
                Force.Equals(other.Force) &&
                ((GroupBy == null && other.GroupBy == null) || GroupBy.Equals(other.GroupBy)) &&
                DisplayError.Equals(other.DisplayError) &&
                ShowError.Equals(other.ShowError) &&
                String.Equals(View, other.View) &&
                ((Properties == null && other.Properties == null) || Properties.Equals(other.Properties))
                );
        }

        public override int GetHashCode()
        {
            return Expand.GetHashCode() ^ Force.GetHashCode() ^ GroupBy.GetHashCode() ^ DisplayError.GetHashCode()
                ^ ShowError.GetHashCode() ^ View.GetHashCode() ^ Properties.GetHashCode();
        }
    }
}

