// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Gives options of how a copy of a provider container should happen.
    /// </summary>
    public enum CopyContainers
    {
        CopyTargetContainer = 0,
        CopyChildrenOfTargetContainer = 1,
    }
}
