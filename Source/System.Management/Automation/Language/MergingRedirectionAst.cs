﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public class MergingRedirectionAst : RedirectionAst
    {
        public MergingRedirectionAst(IScriptExtent extent, RedirectionStream from, RedirectionStream to)
            : base(extent, from)
        {
        }

        public RedirectionStream ToStream { get { throw new NotImplementedException(this.ToString()); } }
    }
}
