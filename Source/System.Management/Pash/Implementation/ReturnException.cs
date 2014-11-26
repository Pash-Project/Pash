// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

namespace System.Management.Pash.Implementation
{
    public class ReturnException : FlowControlException
    {
        public object Value { get; set; }
        public ReturnException(object returnValue)
        {
            Value = returnValue;
        }
    }
}

