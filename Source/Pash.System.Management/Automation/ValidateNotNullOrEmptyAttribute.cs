// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;

namespace System.Management.Automation
{
	/// <summary>
	/// Ensures that the value assigned to the parameter is not null or empty.
	/// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ValidateNotNullOrEmptyAttribute : ValidateArgumentsAttribute
    {
        public ValidateNotNullOrEmptyAttribute() { }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics) 
        {
            // Null object test
            if (arguments == null)
            {
                throw new ValidationMetadataException("Parameter arguments can not be empty or null.");
            }

            // String test
            if (String.IsNullOrEmpty(arguments as String))
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
