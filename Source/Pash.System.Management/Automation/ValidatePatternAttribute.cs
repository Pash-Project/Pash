// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation.Internal;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
	/// <summary>
	/// Compares a regular expression to the value assigned to a parameter.
	/// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidatePatternAttribute : ValidateEnumeratedArgumentsAttribute
    {
        public RegexOptions Options { get; set; }

        public string RegexPattern { get; private set; }

        public ValidatePatternAttribute(string regexPattern)
        { 
            if (regexPattern == null)
            {
                throw new MetadataException("Invalid regex provided to ValidatePatternAttribute");
            }

            Options = RegexOptions.IgnoreCase;
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("Argument is null!");
            }

            if (!(new Regex(RegexPattern, Options).Match(element.ToString()).Success))
            {
                throw new ValidationMetadataException("Argument must validate against regex: " + RegexPattern);
            }
        }
    }
}

