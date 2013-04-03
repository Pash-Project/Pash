// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation
{
    /// <summary>
    /// Defines the danger of the action a cmdlet is about to take.
    /// </summary>
    public enum ConfirmImpact
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }
}
