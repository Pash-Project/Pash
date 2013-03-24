// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ValidateNotNullOrEmptyAttribute : ValidateArgumentsAttribute
    {
        public ValidateNotNullOrEmptyAttribute() { }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            // TODO: implement argument validation
        }
    }
}
