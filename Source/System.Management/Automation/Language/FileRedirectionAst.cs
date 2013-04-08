﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public class FileRedirectionAst : RedirectionAst
    {
        public FileRedirectionAst(IScriptExtent extent, RedirectionStream stream, ExpressionAst file, bool append)
            : base(extent, stream)
        {
            this.Append = append;
            this.Location = file;
        }

        public bool Append { get; private set; }
        public ExpressionAst Location { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return Location;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("> {0}", this.Location);
        }
    }
}
