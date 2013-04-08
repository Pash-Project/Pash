// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System.Collections;
using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Ensures that the value assigned to the parameter is not null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateNotNullAttribute : ValidateArgumentsAttribute
    {
        public ValidateNotNullAttribute()
        {

        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            // Null object test
            if (arguments == null)
            {
                throw new ValidationMetadataException("Parameter arguments can not be empty or null.");
            }

            // Collections test
            IEnumerable collection = arguments as IEnumerable;

            if (collection != null)
            {
                object obj = collection.GetEnumerator().Current;
                if (obj == null)
                {
                    throw new ValidationMetadataException("Parameter arguments can not be empty or null.");
                }
            }
        }
    }
}
