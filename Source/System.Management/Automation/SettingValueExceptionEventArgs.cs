﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class SettingValueExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public bool ShouldThrow { get; set; }

        internal SettingValueExceptionEventArgs(System.Exception exception) { Exception = exception; }
    }
}
