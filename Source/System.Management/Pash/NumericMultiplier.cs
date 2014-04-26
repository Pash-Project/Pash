// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace Pash
{
    static class NumericMultiplier
    {
        static int Kilobyte = 1024;
        static int Megabyte = Kilobyte * Kilobyte;
        static int Gigabyte = Megabyte * Kilobyte;
        static long Terabyte = Gigabyte * (long)Kilobyte;
        static long Petabyte = Terabyte * (long)Kilobyte;

        internal static long GetValue(string multiplier)
        {
            multiplier = multiplier.ToLowerInvariant();
            switch (multiplier)
            {
                case "kb":
                    return Kilobyte;
                case "mb":
                    return Megabyte;
                case "gb":
                    return Gigabyte;
                case "tb":
                    return Terabyte;
                case "pb":
                    return Petabyte;
                default:
                    throw new InvalidOperationException(String.Format("Invalid numeric multiplier '{0}'"));
            }
        }

        internal static object Multiply(long value, string multiplier)
        {
            if (string.IsNullOrEmpty(multiplier))
                return value;

            return value * GetValue(multiplier);
        }

        internal static object Multiply(int value, string multiplier)
        {
            if (string.IsNullOrEmpty(multiplier))
                return value;

            int multiplierValue;
            if (GetInt32MultiplierValue(multiplier, out multiplierValue))
            {
                return value * multiplierValue;
            }

            return value * GetValue(multiplier);
        }

        static bool GetInt32MultiplierValue(string multiplier, out int multiplierValue)
        {
            multiplier = multiplier.ToLowerInvariant();
            switch (multiplier)
            {
                case "kb":
                    multiplierValue = Kilobyte;
                    return true;
                case "mb":
                    multiplierValue = Megabyte;
                    return true;
                case "gb":
                    multiplierValue = Gigabyte;
                    return true;
                default:
                    multiplierValue = 1;
                    return false;
            }
        }
    }
}
