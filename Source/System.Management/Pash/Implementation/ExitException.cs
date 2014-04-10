// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Pash.Implementation
{
    public class ExitException : Exception
    {
        public int ExitCode { get; private set; }
        public ExitException(int exitCode)
        {
            ExitCode = exitCode;
        }
    }
}

