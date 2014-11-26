// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    public class ExitException : FlowControlException
    {
        public Object Argument { get; private set; }
        public ExitException(object arg)
        {
            Argument = arg;
        }
    }
}

