// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//todo: handle value types (if needed)

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace System.Management.Automation
{
    public static class LanguagePrimitives
    {
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
        { //TODO: Make it use formatProvider

            MethodInfo _cast = valueToConvert.GetType().GetMethod("", new Type[] { resultType });

            if (resultType == null)
            {
                throw new ArgumentException("Result type can not be null.");
            }

            if (resultType.IsAssignableFrom(valueToConvert.GetType()))
            {
                return _cast.Invoke(null, new object[] { valueToConvert });
            }

            PSObject _psvalue = valueToConvert as PSObject;

            if (_psvalue != null)
            {
                if (resultType.IsAssignableFrom(_psvalue.BaseObject.GetType()))
                {
                    return _cast.Invoke(null, new object[] { valueToConvert });
                }
            }

            return null;

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
            PSObject _psobj = obj as PSObject;

            if (_psobj != null)
                obj = _psobj.BaseObject;

            return obj as IEnumerable;
        }

        /// <summary>
        /// Returns an enumerator for a given object compatable with the shell.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        /// <returns>The enumerator if it can work, null otherwise.</returns>
        public static IEnumerator GetEnumerator(object obj)
        {
            PSObject _psobj = obj as PSObject;

            if (_psobj != null)
                obj = _psobj.BaseObject;

            return obj as IEnumerator;
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


    }
}


