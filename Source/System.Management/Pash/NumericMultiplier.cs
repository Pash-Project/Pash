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
            if (string.IsNullOrEmpty(multiplier))
                return 1;

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

        internal static object Multiply(double value, string multiplier)
        {
            return value * GetValue(multiplier);
        }

        internal static object Multiply(decimal value, string multiplier)
        {
            decimal result;
            if (Multiply<decimal>(value, GetValue(multiplier), out result))
                return result;

            return Multiply((double)value, multiplier);
        }

        internal static object Multiply(long value, string multiplier)
        {
            long result;
            if (Multiply<long>(value, GetValue(multiplier), out result))
                return result;

            return Multiply((decimal)value, multiplier);
        }

        internal static object Multiply(int value, string multiplier)
        {
            long result;
            if (Multiply<long>(value, GetValue(multiplier), out result))
            {
                // Literals like 1kb are of type int, so return an int if the
                // result fits in one.
                if (result <= Int32.MaxValue && result >= Int32.MinValue)
                    return (int)result;

                return result;
            }

            return Multiply((long)value, multiplier);
        }

        static bool Multiply<T>(dynamic x, dynamic y, out T result)
        {
            try
            {
                checked
                {
                    result = x * y;
                    return true;
                }
            }
            catch (OverflowException)
            {
                result = default(T);
                return false;
            }
        }
    }
}
