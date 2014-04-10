// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace ReferenceTests
{
    class ReferenceTestInfo
    {
        public const string SHELL_NAME = "Pash";
        public static string SHELL_EXECUTABLE = new Uri(typeof(Pash.Program).Assembly.CodeBase).LocalPath;
        public const bool IS_PASH = true;
    }
}

