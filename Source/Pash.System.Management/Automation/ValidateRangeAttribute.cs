// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
	/// <summary>
	/// Ensures that the Parameter falls in between two values.
	/// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateRangeAttribute : ValidateEnumeratedArgumentsAttribute
    {
        public object MaxRange { get; private set; }
        public object MinRange { get; private set; }

        private IComparable minCompare;
        private IComparable maxCompare;

        public ValidateRangeAttribute(object minRange, object maxRange)
        {
            if ((minRange == null) || (maxRange == null) || (!(minRange.GetType().Equals(maxRange.GetType())))) 
            {
                throw new ValidationMetadataException("Fatal error with ValidateRange!");
            }
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("Value is null!");
            }

            if (minCompare.CompareTo(element) > 0)
            {
                throw new ValidationMetadataException("Value is to small!");
            }

            if (maxCompare.CompareTo(element) < 0)
            {
                throw new ValidationMetadataException("Value is too large!");
            } 
        }
    }
}
