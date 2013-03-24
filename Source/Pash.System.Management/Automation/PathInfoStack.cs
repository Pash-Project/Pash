// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public sealed class PathInfoStack : Stack<PathInfo>
    {
        public string Name { get { throw new NotImplementedException(); } }

        // internals
        //internal PathInfoStack(string stackName, System.Collections.Generic.Stack<PathInfo> locationStack);
    }
}
