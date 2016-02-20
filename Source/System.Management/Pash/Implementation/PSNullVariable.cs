// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Pash.Implementation
{
    class PSNullVariable : PSVariable
    {
        internal PSNullVariable()
            : base("null", null, ScopedItemOptions.None)
        {
        }

        public override object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
            }
        }

        public override ScopedItemOptions Options
        {
            get
            {
                return ScopedItemOptions.None;
            }
            set
            {
            }
        }
    }
}
