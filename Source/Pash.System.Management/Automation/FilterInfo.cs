// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Information on a Pash filter.
    /// </summary>
    public class FilterInfo : CommandInfo
    {
        //todo: implement
        private string definition;
        public override string Definition
        {
            get
            {
                return definition;
            }
        }

        public ScopedItemOptions Options { get; set; }

        public ScriptBlock ScriptBlock { get; private set; }
    }
}

