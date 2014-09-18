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
using System.Text;
using System.Management.Automation.Language;

namespace System.Management.Automation
{
    public static class LanguagePrimitives
    {
        private static readonly Regex _signedHexRegex = new Regex(@"^\s*[\+-]?0[xX][0-9a-fA-F]+\s*$");
        private static readonly Type[] _numericConversionTypeOrder = new Type[] {
            typeof(int), typeof(long), typeof(decimal), typeof(double)
        };
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

        #region Conversion primitives

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

            if (resultType == typeof(string))
            {
                return ConvertToString(valueToConvert, formatProvider);
            }

            if (resultType == typeof(bool))
            {
                return ConvertToBool(valueToConvert);
            }

            if (valueToConvert != null && resultType.IsEnum) // enums have to be parsed
            {
                return Enum.Parse(resultType, valueToConvert.ToString(), true);
            }

            if (resultType.IsNumeric())
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

        public static T ConvertTo<T>(object valueToConvert)
        {
            return (T)ConvertTo(valueToConvert, typeof(T));
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
        {
            return ConvertToBool(obj);
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
            // Otherwise, if one operand designates a value of type float, the values designated by both operands are
            // converted to type double, if necessary. The result has type double.
            if (leftType == typeof(float) || rightType  == typeof(float))
            {
                return TryConvertToAnyNumericWithLeastType(left, typeof(double), out leftConverted) &&
                    TryConvertToAnyNumericWithLeastType(right, typeof(double), out rightConverted);
            }

            // If one operand designates a value of type decimal, the value designated by the other operand is converted
            // to that type, if necessary. The result has type decimal.
            // Otherwise, if one operand designates a value of type double, the value designated by the 
            // other operand is converted to that type, if necessary. The result has type double.
            // Otherwise, if one operand designates a value of type long, the value designated by the other operand 
            // value is converted to that type, if necessary. The result has the type first in the sequence long and
            // double that can represent its value.
            // Otherwise, the values designated by both operands are converted to type int, if necessary.
            foreach (var type in new [] {typeof(decimal), typeof(double), typeof(long), typeof(int)})
            {
                // NOTE: the specification tells us, that if e.g. the left is int, the right should be
                // converted to int. However, if it doesn't fit in an int, we need to try more!
                if (leftType == type)
                {
                    leftConverted = left;
                    return TryConvertToAnyNumericWithLeastType(right, type, out rightConverted);
                }
                if (rightType == type)
                {
                    rightConverted = right;
                    return TryConvertToAnyNumericWithLeastType(left, type, out leftConverted);
                }
            }
            // shouldn't happen as one operand should be of one of the numeric types
            return false;
        }

        private static bool ConvertToBool(object rawValue)
        {
            rawValue = PSObject.Unwrap(rawValue); // just make this sure
            if (rawValue == null)
            {
                return false;
            }
            if (rawValue is bool)
            {
                return ((bool)rawValue);
            }
            else if (rawValue is string)
            {
                return ((string)rawValue).Length != 0;
            }
            else if (rawValue is IList)
            {
                var list = rawValue as IList;
                if (list.Count > 1)
                {
                    return true;
                }
                else if (list.Count == 1)
                {
                    return ConvertToBool(list[0]);
                }
                else // empty list
                {
                    return false;
                }
            }
            else if (rawValue.GetType().IsNumeric())
            {
                return ((dynamic)rawValue) != 0;
            }
            else if (rawValue is char)
            {
                return ((char)rawValue) != ((char)0);
            }
            else if (rawValue is SwitchParameter)
            {
                return ((SwitchParameter)rawValue).IsPresent;
            }
            return true; // any object that's not null
        }

        private static string ConvertToString(object rawValue, IFormatProvider formatProvider)
        {
            rawValue = PSObject.Unwrap(rawValue);
            // A value of null type is converted to the empty string.
            if (rawValue == null)
            {
                return string.Empty;
            }
            // The bool value $false is converted to "False"; the bool value $true is converted to "True".
            if (rawValue is bool)
            {
                return (bool)rawValue ? "True" : "False";
            }
            // A char type value is converted to a 1-character string containing that char.
            if (rawValue is char)
            {
                return new string((char)rawValue, 1);
            }
            // A numeric type value is converted to a string having the form of a corresponding numeric literal.
            // However, the result has no leading or trailing spaces, no leading plus sign, integers have base 10, and
            // there is no type suffix. For a decimal conversion, the scale is preserved. For values of -∞, +∞, and
            // NaN, the resulting strings are "-Infinity", "Infinity", and "NaN", respectively.
            if (rawValue is byte || rawValue is short || rawValue is int || rawValue is long)
            {
                return Convert.ToInt64(rawValue).ToString(formatProvider);
            }
            if (rawValue is decimal)
            {
                return ((decimal)rawValue).ToString(formatProvider);
            }
            if (rawValue is float)
            {
                return ((float)rawValue).ToString(formatProvider);
            }
            if (rawValue is double)
            {
                return ((double)rawValue).ToString(formatProvider);
            }
            // For an enumeration type value, the result is a string containing the name of each enumeration
            // constant encoded in that value, separated by commas.
            if (rawValue is Enum)
            {
                // Nicely enough, this is the format specified by the G format specifier. Except for the spaces
                // after the comma. But PowerShell seems to do exactly the same.
                return ((Enum)rawValue).ToString("G");
            }
            // For a 1-dimensional array, the result is a string containing the value of each element in that array, from
            // start to end, converted to string, with elements being separated by the current Output Field
            // Separator (§2.3.2.2). For an array having elements that are themselves arrays, only the top-level
            // elements are converted. The string used to represent the value of an element that is an array, is
            // implementation defined. For a multi-dimensional array, it is flattened (§9.12) and then treated as a
            // 1-dimensional array.
            // Windows PowerShell: For other enumerable types, the source value is treated like a 1-dimensional
            // array.
            if (rawValue is IEnumerable)
            {
                var arr = (IEnumerable)rawValue;
                var runspace = Runspaces.Runspace.DefaultRunspace;
                var ofsV = runspace.SessionStateProxy.GetVariable("OFS");
                var ofs = ofsV != null ? ofsV.ToString() : " ";

                var sb = new StringBuilder();
                bool first = true;
                // foreach visits multidimensional arrays in row-major order, thereby handling the flattening for us
                foreach (var o in arr)
                {
                    if (!first)
                    {
                        sb.Append(ofs);
                    }
                    sb.Append(o.ToString());
                    first = false;
                }

                return sb.ToString();
            }
            // A scriptblock type value is converted to a string containing the text of that block without the
            // delimiting { and } characters.
            if (rawValue is ScriptBlock)
            {
                var scriptBlock = (ScriptBlock)rawValue;
                var ast = (ScriptBlockAst)scriptBlock.Ast;
                // TODO: I would have thought ast.EndBlock.Extent.Text would suffice here, but this corresponds to only the first token in the script block
            }

            // For other reference type values, if the reference type supports such a conversion, that conversion is
            // used; otherwise, the conversion is in error.
            // Windows PowerShell: The string used to represent the value of an element that is an array has the
            // form System.type[], System.type[,], and so on.
            // Windows PowerShell: For other reference types, the method ToString is called.
            return Convert.ToString(rawValue, formatProvider);
        }

        private static bool TryParseSignedHexString(string str, out object parsed)
        {
            parsed = null;
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
            // long.MinValue * -1 would cause an overflow;
            if (intermediate == long.MinValue && sign == -1)
            {
                parsed = ((decimal)intermediate) * sign;
                return true;
            }
            intermediate *= sign;
            // check if the result can be an int
            if (intermediate >= int.MinValue && intermediate <= int.MaxValue)
            {
                parsed = (int)intermediate;
            }
            else // otherwise it's the long value
            {
                parsed = intermediate;
            }
            return true;
        }

        private static bool TryConvertToAnyNumericWithLeastType(object obj, out object converted)
        {
            return TryConvertToAnyNumericWithLeastType(obj, _numericConversionTypeOrder[0], out converted);
        }

        private static bool TryConvertToAnyNumericWithLeastType(object obj, Type leastType, out object converted)
        {
            // This method tries to convert some object to a numeric. It tries to cast to types in this order:
            // int, long, decimal, double. It begins with with the type designated by leastType
            var input = obj;
            converted = null;
            var str = obj as string;
            var tryFromTypeIdx = Array.IndexOf(_numericConversionTypeOrder, leastType);
            if (str != null && _signedHexRegex.IsMatch(str))
            {
                if (!TryParseSignedHexString(str, out input))
                {
                    return false;
                }
                // if we got for example a long, we don't even need to try to convert to int
                // so we can begin later on at a higher index
                var newTypeIdx = Array.IndexOf(_numericConversionTypeOrder, input.GetType());
                if (newTypeIdx > tryFromTypeIdx)
                {
                    tryFromTypeIdx = newTypeIdx;
                }
            }
            // try to convert to the types in their order beginning at leastType
            for (int i = tryFromTypeIdx; i < _numericConversionTypeOrder.Length; i++)
            {
                if (TryConvertTo(input, _numericConversionTypeOrder[i], out converted))
                {
                    return true;
                }
            }
            return false;
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
                if (!TryConvertToAnyNumericWithLeastType((string)left, out leftParsed))
                {
                    return false;
                }
                left = leftParsed;
            }
            // Otherwise, if the right operand designates a value of type string but does not represent a number (§6.16),
            // the conversion is in error.
            if (right is string)
            {
                if (!TryConvertToAnyNumericWithLeastType((string)right, out rightParsed))
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


