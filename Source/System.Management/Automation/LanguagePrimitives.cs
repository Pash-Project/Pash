// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//todo: handle value types (if needed)

using System;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using Extensions.Reflection;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
    public static class LanguagePrimitives
    {
        private static readonly Regex _signedHexRegex = new Regex(@"^\s*[\+-]?0[xX][0-9a-fA-F]+\s*$");
        #region Equals primitives

        /// <summary>
        /// Tests if the two given objects are equal.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public new static bool Equals(object first, object second)
        {
            return Equals(first, second, false, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tests if the two given objects are equal.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <param name="ignoreCase">If comparing strings, ignore case.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public static bool Equals(object first, object second, bool ignoreCase)
        {
            return Equals(first, second, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tests if the two given objects are equal.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <param name="ignoreCase">If comparing strings, ignore case.</param>
        /// <param name="formatProvider">A FormatProvider to use when comparing strings.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public static bool Equals(object first, object second, bool ignoreCase, IFormatProvider formatProvider)
        {
            PSObject _psfirst = first as PSObject;
            PSObject _pssecond = second as PSObject;

            // PSObject comparison
            if ((_psfirst != null) || (_pssecond != null))
                return (_psfirst.BaseObject == _pssecond.BaseObject);

            // String comparison
            String _strfirst = first as String;

            if (_strfirst != null)
            {
                String _strsecond = second as String;
                if (_strsecond != null)
                {
                    return (0 == String.Compare(_strsecond, _strfirst, ignoreCase, formatProvider as CultureInfo));
                }

                return (0 == String.Compare(ConvertTo(second, typeof(String), formatProvider).ToString(), _strfirst, ignoreCase, formatProvider as CultureInfo));

            }

            // If all else fails, stardard object comparison
            return (first == second);

        }

        #endregion

        #region Conversation primitives

        /// <summary>
        /// Tries to convert between types.
        /// </summary>
        /// <typeparam name="T">The type of the resulting object.</typeparam>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="result">The converted type.</param>
        /// <returns>True if the conversion worked, false otherwise.</returns>
        public static bool TryConvertTo<T>(object valueToConvert, out T result)
        {
            return TryConvertTo<T>(valueToConvert, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Tries to convert between types.
        /// </summary>
        /// <typeparam name="T">The type of the resulting object.</typeparam>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="formatProvider">The format provider to use for type conversions.</param>
        /// <param name="result">The converted type.</param>
        /// <returns>True if the conversion worked, false otherwise.</returns>
        public static bool TryConvertTo<T>(object valueToConvert, IFormatProvider formatProvider, out T result)
        {
            try
            {
                result = (T)ConvertTo(valueToConvert, typeof(T), formatProvider);
            }

            catch
            {
                result = default(T);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to convert between types.
        /// </summary>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="resultType">The type you want to convert to.</param>
        /// <param name="result">The converted type.</param>
        /// <returns>True if the conversion worked, false otherwise.</returns>
        public static bool TryConvertTo(object valueToConvert, Type resultType, out object result)
        {
            return TryConvertTo(valueToConvert, resultType, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Tries to convert between types.
        /// </summary>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="resultType">The type you want to convert to.</param>
        /// <param name="formatProvider">The format provider to use for type conversions.</param>
        /// <param name="result">The converted type.</param>
        /// <param name="result">The converted type.</param>
        public static bool TryConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider, out object result)
        {
            try
            {
                result = ConvertTo(valueToConvert, resultType, formatProvider);
            }

            catch (Exception)
            {
                result = null;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Convert between types.
        /// </summary>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="resultType">The type you want to convert to.</param>
        /// <returns>The converted type.</returns>
        public static object ConvertTo(object valueToConvert, Type resultType)
        {
            return ConvertTo(valueToConvert, resultType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert between types.
        /// </summary>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="resultType">The type you want to convert to.</param>
        /// <param name="formatProvider">The format provider to use for type conversions.</param>
        /// <returns>The converted type.</returns>
        public static object ConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider)
        {
            // TODO: Make it use formatProvider
            // TODO: read more about the Extended Type System (ETS) of Powershell and enhance this functionality
            // TODO: check "3.7.5 Better conversion" of Windows Powershell Language Specification 3.0
            if (resultType == null)
            {
                throw new ArgumentException("Result type can not be null.");
            }

            // result is no PSObject, so unpack the value if we deal with one
            if (valueToConvert is PSObject &&
                resultType != typeof(PSObject) &&
                resultType != typeof(PSObject[]))
            {
                valueToConvert = ((PSObject)valueToConvert).BaseObject;
            }

            // check if the result is an array and we have something that needs to be casted
            if (resultType.IsArray && valueToConvert != null && resultType != valueToConvert.GetType())
            {
                var elementType = resultType.GetElementType();
                var enumerableValue = GetEnumerable(valueToConvert);
                // check for simple packaging
                // Powershell seems to neither enumerate dictionaries nor strings
                if (enumerableValue == null)
                {
                    var array = Array.CreateInstance(elementType, 1);
                    array.SetValue(ConvertTo(valueToConvert, elementType, formatProvider), 0);
                    return array;
                }
                // otherwise we have some IEnumerable thing. recursively create a list and copy to array
                // a list first, because we don't know the number of arguments
                var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var curValue in enumerableValue)
                {
                    list.Add(ConvertTo(curValue, elementType, formatProvider));
                }
                var targetArray = Array.CreateInstance(elementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    targetArray.SetValue(list[i], i);
                }
                return targetArray;
            }

            // resultType is no array
            // TODO: use PSTypeConverters for the following!
            if (resultType == typeof(PSObject))
            {
                return PSObject.WrapOrNull(valueToConvert);
            }

            if (valueToConvert != null && resultType.IsAssignableFrom(valueToConvert.GetType()))
            {
                return valueToConvert;
            }

            if (valueToConvert != null && resultType == typeof(String))
            {
                return valueToConvert.ToString();
            }

            if (valueToConvert != null && resultType.IsEnum) // enums have to be parsed
            {
                return Enum.Parse(resultType, valueToConvert.ToString(), true);
            }

            if (valueToConvert != null && resultType.IsNumeric())
            {
                try
                {
                    return ConvertToNumeric(valueToConvert, resultType);
                }
                catch (Exception)
                {
                    throw new PSInvalidCastException(String.Format("Couldn't convert from '{0}' to numeric type",
                                                                 valueToConvert.ToString()));
                }
            }

            if (resultType == typeof(SwitchParameter)) // switch parameters can simply be present
            {
                return new SwitchParameter(true);
            }

            object result = null;
            if (valueToConvert != null && TryConvertUsingTypeConverter(valueToConvert, resultType, out result))
            {
                return result;
            }
            /* TODO: further conversion methods:
             * Parse Method: If the source type is string and the destination type has a method called Parse, that
             * method is called to perform the conversion.
             * Constructors: If the destination type has a constructor taking a single argument whose type is that of
             * the source type, that constructor is called to perform the conversion.
            */
            return DefaultConvertOrCast(valueToConvert, resultType);
        }

        #endregion

        #region Comparison primitives

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="first">First object to compare.</param>
        /// <param name="second">Second object to compare.</param>
        /// <returns>0 if they are equal, negative if first is less then second, positive otherwise.</returns>
        public static int Compare(object first, object second)
        {
            return Compare(first, second, false, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="first">First object to compare.</param>
        /// <param name="second">Second object to compare.</param>
        /// <param name="ignoreCase">If the objects are strings, ignore case.</param>
        /// <returns>0 if they are equal, negative if first is less then second, positive otherwise.</returns>
        public static int Compare(object first, object second, bool ignoreCase)
        {
            return Compare(first, second, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="first">First object to compare.</param>
        /// <param name="second">Second object to compare.</param>
        /// <param name="ignoreCase">If the objects are strings, ignore case.</param>
        /// <param name="formatProvider">Use the given format provider.</param>
        /// <returns>0 if they are equal, negative if first is less then second, positive otherwise.</returns>
        public static int Compare(object first, object second, bool ignoreCase, IFormatProvider formatProvider)
        { //TODO: Something with the formatProivider
            // Null check
            if ((first == null) && (second == null))
            {
                return 0;
            }

            if (first is PSObject) first = (first as PSObject).BaseObject;
            if (second is PSObject) second = (second as PSObject).BaseObject;

            // Int check
            //todo

            if (first is int)
            {
                if (second is int)
                {
                    return (int)first - (int)second;
                }
                else
                {
                    throw new ArgumentException("Types are not comparable.");
                }
            }

            // Floating point check
            //todo
            /*
            if (first is Decimal)
            {
                if (second is Decimal)
                {
                    return first - second;
                }
                else 
                {
                    throw new ArgumentException("Types are not comparable.");
                }
            }*/

            // String check
            if (first is String)
            {
                if (second is String)
                {
                    return String.Compare(first.ToString(), second.ToString(), ignoreCase);
                }
                else
                {
                    throw new ArgumentException("Types are not comparable.");
                }
            }

            // Return 0 otherwise.
            throw new NotImplementedException();
        }

        #endregion

        #region Enumeration primitives

        /// <summary>
        /// Returns an IEnumerable for a given object compatable with the shell.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        /// <returns>The enumerator if it can work, null otherwise.</returns>
        public static IEnumerable GetEnumerable(object obj)
        {
            obj = PSObject.Unwrap(obj);
            // Powershell seems to exclude dictionaries and strings from being enumerables
            if (obj is IDictionary || obj is string)
            {
                return null;
            }
            // either return it as enumerable or null
            return obj as IEnumerable;
        }

        /// <summary>
        /// Returns an enumerator for a given object compatable with the shell.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        /// <returns>The enumerator if it can work, null otherwise.</returns>
        public static IEnumerator GetEnumerator(object obj)
        {
            obj = PSObject.Unwrap(obj);
            // Powershell seems to exclude dictionaries and strings from being enumerables
            if (obj is IDictionary || obj is string)
            {
                return null;
            }
            var enumerable = GetEnumerable(obj);
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            if (obj is IEnumerator)
            {
                return obj as IEnumerator;
            }
            return null;
        }

        #endregion

        #region Misc primitives

        /// <summary>
        /// Tests if a given object is true.
        /// </summary>
        /// <param name="obj">The given object</param>
        /// <returns>True is it's true, false otherwise.</returns>
        public static bool IsTrue(object obj)
        {//todo test numbers
            return (!((obj == null) || (obj as String == String.Empty)));
        }

        #endregion

        internal static bool UsualArithmeticConversion(object left, object right, out object leftConverted,
                                                       out object rightConverted)
        {
            left = PSObject.Unwrap(left);
            right = PSObject.Unwrap(right);
            Type leftType = left.GetType();
            Type rightType = right.GetType();
            leftConverted = null;
            rightConverted = null;
            // 6.15 Usual arithmetic conversions
            // If neither operand designates a value having numeric type, then
            if (!leftType.IsNumeric() && !rightType.IsNumeric() &&
                !UsualArithmeticConversionOneOperand(ref left, ref right))
            {
                return false;
            }

            // Numeric conversions:
            // If one operand designates a value of type decimal, the value designated by the other operand is converted
            // to that type, if necessary. The result has type decimal.
            // Otherwise, if one operand designates a value of type double, the value designated by the 
            // other operand is converted to that type, if necessary. The result has type double.
            // Otherwise, if one operand designates a value of type long, the value designated by the other operand 
            // value is converted to that type, if necessary. The result has the type first in the sequence long and
            // double that can represent its value.
            foreach (var type in new [] {typeof(decimal), typeof(double), typeof(long)})
            {
                if (leftType == type)
                {
                    leftConverted = left;
                    return TryConvertToNumericTryHexFirst(right, type, out rightConverted);
                }
                if (rightType == type)
                {
                    rightConverted = right;
                    return TryConvertToNumericTryHexFirst(left, type, out leftConverted);
                }
            }
            // Otherwise, if one operand designates a value of type float, the values designated by both operands are
            // converted to type double, if necessary. The result has type double.
            if (left is float || right is float)
            {
                return TryConvertToNumericTryHexFirst(left, typeof(double), out leftConverted) &&
                       TryConvertToNumericTryHexFirst(right, typeof(double), out rightConverted);
            }
            // Otherwise, the values designated by both operands are converted to type int, if necessary. The result 
            // has the first in the sequence int, long, double that can represent its value without truncation.
            return TryConvertToNumericTryHexFirst(left, typeof(int), out leftConverted) &&
                   TryConvertToNumericTryHexFirst(right, typeof(int), out rightConverted);
        }

        private static bool TryParseSignedHexString(string str, out long parsed)
        {
            parsed = 0;
            str = str.Trim();
            char firstChar = str[0];
            int sign = 1;
            int prefixLen = 2;
            if (firstChar == '-')
            {
                sign = -1;
                prefixLen = 3;
            }
            else if (firstChar == '+')
            {
                prefixLen = 3;
            }
            str = str.Substring(prefixLen);
            long intermediate;
            if (!long.TryParse(str, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out intermediate))
            {
                return false;
            }
            parsed = sign * intermediate;
            return true;
        }

        private static bool TryParseNumeric(string str, out object parsed)
        {
            // in arithmetics, signed hex strings are allowed
            if (_signedHexRegex.IsMatch(str))
            {
                long parsedLong;
                var res = TryParseSignedHexString(str, out parsedLong);
                parsed = parsedLong;
                return res;
            }
            parsed = null;
            foreach (var type in new [] {typeof(int), typeof(long), typeof(double)})
            {
                try
                {
                    parsed = ParseStringToNumeric(str, type);
                    return true;
                }
                catch (InvalidCastException)
                {
                }
            }
            return false;
        }

        private static bool TryConvertToNumericTryHexFirst(object obj, Type type, out object converted)
        {
            var input = obj;
            converted = null;
            var str = obj as string;
            if (str != null && _signedHexRegex.IsMatch(str))
            {
                long parsedLong;
                if (!TryParseSignedHexString(str, out parsedLong))
                {
                    return false;
                }
                input = parsedLong;
            }
            return TryConvertTo(input, type, out converted);
        }

        private static bool UsualArithmeticConversionOneOperand(ref object left, ref object right)
        {
            //If the left operand designates a value of type bool, the conversion is in error.
            if (left is bool)
            {
                return false;
            }
            // Otherwise, all operands designating the value $null are converted to zero of type int and the
            // process continues with the numeric conversions listed below.
            bool converted = false;
            if (left == null)
            {
                left = (int)0;
                converted = true;
            }
            if (right == null)
            {
                right = (int)0;
                converted = true;
            }
            if (converted)
            {
                return true;
            }
            // Otherwise, if the left operand designates a value of type char and the right operand designates a
            // value of type bool, the conversion is in error.
            if (left is char && right is bool)
            {
                return false;
            }
            // Otherwise, if the left operand designates a value of type string but does not represent a number (§6.16),
            // the conversion is in error.
            object leftParsed, rightParsed;
            if (left is string)
            {
                if (!TryParseNumeric((string)left, out leftParsed))
                {
                    return false;
                }
                left = leftParsed;
            }
            // Otherwise, if the right operand designates a value of type string but does not represent a number (§6.16),
            // the conversion is in error.
            if (right is string)
            {
                if (!TryParseNumeric((string)right, out rightParsed))
                {
                    return false;
                }
                right = rightParsed;
            }
            // Otherwise, all operands designating values of type string are converted to numbers (§6.16), and the
            // process continues with the numeric conversions listed below.
            if (left is string || right is string)
            {
                return true;
            }
            // Otherwise, the conversion is in error.
            return false;
        }

        private static object ConvertToNumeric(object value, Type numericType)
        {
            if (value is string)
            {
                return ParseStringToNumeric((string)value, numericType);
            }
            switch (Type.GetTypeCode(numericType))
            {
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.Int32:
                    return Convert.ToInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
            }
            throw new InvalidCastException("Cannot convert to non-numeric type " + numericType.ToString());
        }

        private static object ParseStringToNumeric(string value, Type numericType)
        {
            /*
            6.16 Conversion from string to numeric type
            [...]
            An empty string is converted to the value zero. [...]
            A string containing only white space and/or line terminators is converted to the value zero.
            One leading + or - sign is permitted.
            An integer number may have a hexadecimal prefix (0x or 0X).
            An optionally signed exponent is permitted.
            Type suffixes and multipliers are not permitted.
            The case-distinct strings "-Infinity", "Infinity", and "NaN" are recognized as the values -inf, +inf, 
            and NaN, respectively.
            */
            if (String.IsNullOrWhiteSpace(value))
            {
                return DefaultConvertOrCast(0, numericType);
            }
            value = value.Trim();
            var intStyle = NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands;
            var floatStyle = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint;
            if (!numericType.IsNumericFloat() && value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                intStyle = NumberStyles.AllowHexSpecifier;
                value = value.Substring(2);
            }
            switch (Type.GetTypeCode(numericType))
            {
                case TypeCode.Byte:
                    return Byte.Parse(value, intStyle);
                case TypeCode.SByte:
                    return SByte.Parse(value, intStyle);
                case TypeCode.UInt16:
                    return UInt16.Parse(value, intStyle);
                case TypeCode.UInt32:
                    return UInt32.Parse(value, intStyle);
                case TypeCode.UInt64:
                    return UInt64.Parse(value, intStyle);
                case TypeCode.Int16:
                    return Int16.Parse(value, intStyle);
                case TypeCode.Int32:
                    return Int32.Parse(value, intStyle);
                case TypeCode.Int64:
                    return Int64.Parse(value, intStyle);
                case TypeCode.Decimal:
                    return Decimal.Parse(value, floatStyle, CultureInfo.InvariantCulture);
                case TypeCode.Double:
                    return Double.Parse(value, floatStyle, CultureInfo.InvariantCulture);
                case TypeCode.Single:
                    return Single.Parse(value, floatStyle, CultureInfo.InvariantCulture);
            }
            var msg = String.Format("Cannot convert '{0}' to non-numeric type {1}", value, numericType.ToString());
            throw new InvalidCastException(msg);
        }

        private static bool TryConvertUsingTypeConverter(object value, Type type, out object result)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter != null && converter.CanConvertFrom(value.GetType()))
            {
                try
                {
                    result = converter.ConvertFrom(value);
                    return true;
                }
                catch
                {
                }
            }

            result = null;
            return false;
        }

        private static object DefaultConvertOrCast(object value, Type type)
        {
            // check for convertibles
            try
            {
                if (value is IConvertible)
                {
                    return Convert.ChangeType(value, type);
                }
            }
            catch (Exception) //ignore exception and try cast
            {
            }
            // following idea from http://stackoverflow.com/questions/3062807/dynamic-casting-based-on-type-information
            var castMethod = typeof(LanguagePrimitives).GetMethod("Cast").MakeGenericMethod(type);
            // it's okay to have an excpetion if we can't do anything anymore, then the parameter just doesn't work
            try
            {
                return castMethod.Invoke(null, new object[] { value });
            }
            catch (Exception e)
            {
                var msg = String.Format("Value '{0}' can't be converted or casted to '{1}'",
                    value.ToString(), type.ToString());
                throw new PSInvalidCastException(msg, e);
            }
        }

        public static T Cast<T>(object obj)
        {
            // TODO: make private
            return (T) obj;
        }


    }
}


