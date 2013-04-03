// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    /// <summary>
    /// Validation attribute that checks if the parameter is of a certain set of options.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateSetAttribute : ValidateEnumeratedArgumentsAttribute
    {

        public bool IgnoreCase { get; set; }

        public IList<string> ValidValues { get; private set; }

        public ValidateSetAttribute(params string[] validValues)
        {
            if ((validValues == null) || (validValues.Length == 0))
            {
                throw new MetadataException("Invalid set provided to ValidateSetAttribute");
            }

            ValidValues = validValues;
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("No argument passed.");
            }

            string _element = element.ToString();

            foreach (String _valid in ValidValues)
            {
                if (String.Equals(_valid, _element, StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }
            }

            //TODO: Might need to be fixed
            throw new ValidationMetadataException("Parameter must have value " + ValidValues.ToString());
        }
    }
}
