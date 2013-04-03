// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Parameter that accepts credentials.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CredentialAttribute : ArgumentTransformationAttribute
    {
        //todo: needs implemetation
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            return null;
        }
    }
}

