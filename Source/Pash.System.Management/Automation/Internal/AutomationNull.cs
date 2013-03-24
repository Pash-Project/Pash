// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation.Internal
{
    public static class AutomationNull
    {
        public static PSObject Value { get; private set; }

        static AutomationNull()
        {
            Value = new PSObject();
        }
    }
}
