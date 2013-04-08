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
        public object MaxRange
        {
            get
            {
                return maxRange;
            }
        }

        public object MinRange
        {
            get
            {
                return minRange;
            }
        }

        private object minRange;
        private object maxRange;
        private IComparable minCompare;
        private IComparable maxCompare;

        public ValidateRangeAttribute(object minRange, object maxRange)
        {
            if ((minRange == null) || (maxRange == null) || (!(minRange.GetType().Equals(maxRange.GetType()))))
            {
                throw new ValidationMetadataException("Fatal error with ValidateRange!");
            }

            minCompare = minRange as IComparable;
            if (minCompare == null)
            {
                throw new ValidationMetadataException("MinRange Not IComparable");
            }
            maxCompare = maxRange as IComparable;
            if (maxCompare == null)
            {
                throw new ValidationMetadataException("MaxRange Not IComparable");
            }

            this.minRange = minRange;
            this.maxRange = maxRange;
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
