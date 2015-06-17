// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace Microsoft.PowerShell.Commands
{
    public sealed class MatchInfoContext : ICloneable
    {
        public string[] DisplayPostContext { get; set; }
        public string[] DisplayPreContext { get; set; }
        public string[] PostContext { get; set; }
        public string[] PreContext { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
