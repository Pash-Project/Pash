// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    /// <summary>
    /// Validates the range of number of arguements a parameter can have.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateCountAttribute : ValidateArgumentsAttribute
    {
        public int MaxLength { get; private set; }

        public int MinLength { get; private set; }

        public ValidateCountAttribute(int minLength, int maxLength)
        {
            if ((minLength < 0) || (maxLength <= 0) || (maxLength < minLength))
            {
                throw new ValidationMetadataException("Fatal error with the fields of ValidateCount! Make sure MinLength and MaxLength make sense.");
            }

            MaxLength = maxLength;
            MinLength = minLength;
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            int _total = 0;

            if (arguments != null)
            {
                IEnumerable _collection = arguments as IEnumerable;
                if (_collection != null)
                {
                    while (_collection.GetEnumerator().MoveNext())
                    {
                        _total++;
                    }
                }

                else _total = 1;
            }

            if (_total < MinLength)
            {
                throw new ValidationMetadataException("Argument requires minimum of " + MinLength + " values.");
            }

            if (_total > MaxLength)
            {
                throw new ValidationMetadataException("Argument requires maximum of " + MaxLength + " values.");
            }
        }
    }
}
