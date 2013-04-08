// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    /// <summary>
    /// Base class for custom attributes that transform arguements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ArgumentTransformationAttribute : CmdletMetadataAttribute
    {
        public abstract object Transform(EngineIntrinsics engineIntrinsics, object inputData);
    }
}

