// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public abstract class PSPropertyInfo : PSMemberInfo
    {
        protected PSPropertyInfo()
        {
        }

        public abstract bool IsGettable { get; }
        public abstract bool IsSettable { get; }
    }
}
