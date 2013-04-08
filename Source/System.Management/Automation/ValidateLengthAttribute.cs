// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    /// <summary>
    /// Ensures the the value of the parameter is of a specifc range of lengths.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateLengthAttribute : ValidateEnumeratedArgumentsAttribute
    {
        public int MaxLength { get; private set; }

        public int MinLength { get; private set; }

        public ValidateLengthAttribute(int minLength, int maxLength)
        {
            if ((minLength < 0) || (maxLength <= 0) || (maxLength < minLength))
            {
                throw new ValidationMetadataException("Fatal error with the fields of ValidateLength! Make sure MinLength and MaxLength make sense.");
            }

            MaxLength = maxLength;
            MinLength = minLength;
        }

        protected override void ValidateElement(object element)
        {
            String _element = element.ToString();

            if (_element == null)
            {
                throw new ValidationMetadataException("Argument is null!");
            }

            if (_element.Length < MinLength)
            {
                throw new ValidationMetadataException("The element is too small! Must be " + MinLength + " characters long.");
            }

            if (_element.Length > MaxLength)
            {
                throw new ValidationMetadataException("The element is too large! Must be " + MaxLength + " characters long.");
            }
        }
    }
}

