// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation.Internal
{
    internal interface ISupportShouldProcess
    {
        bool SupportsShouldProcess { get; }
    }
}
