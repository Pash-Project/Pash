// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Pash.Implementation
{
    public class ReturnException : Exception
    {
        public object Value { get; set; }
        public ReturnException(object returnValue)
        {
            Value = returnValue;
        }
    }
}

