// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace Pash
{
    static class NumericMultiplier
    {
        internal static long GetValue(string multiplier)
        {
            multiplier = multiplier.ToLowerInvariant();
            switch (multiplier)
            {
                case "kb":
                    return 1024;
                case "mb":
                    return 1048576;
                case "gb":
                    return 1073741824;
                case "tb":
                    return 1099511627776;
                case "pb":
                    return 1125899906842624;
                default:
                    throw new InvalidOperationException(String.Format("Invalid numeric multiplier '{0}'"));
            }
        }
    }
}
