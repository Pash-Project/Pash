using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Management.Automation;

namespace System.Management.Pash.Implementation
{
    ///
    /// ComparisonOperations:
    ///     - class implementing comparison operations in the language
    ///     - uses IComparer<T> interface and default implementations to perform comparisons
    ///     - handles type mapping for objects to specific comparers based on LHS type
    ///     - currently only handles primitive types (but should be extensible)
    ///     - numeric types are handled by LHS or RHS type match in:  double, float, long, int, char, byte, bool order
    ///     - TODO:  handle ==, != cases here as well
    ///     - TODO:  handle arrays, hashtables
    ///     - TODO:  improve test cases to ensure correctness with Microsoft Powershell
    ///
    internal static class ComparisonOperations
    {
        ///
        /// NOTE:  dynamic doesn't seem to work in Mono (on Mac OS X)
        ///        use generic functions and type based dispatching instead.
        /// EXAMPLE:
        ///     Func<IComparer<dynamic>, dynamic, dynamic, object> opComparer =
        ///         (IComparer<dynamic> c, dynamic l, dynamic r) => checked(c.Compare(l, r) > 0);
        ///     return ComparisonOperation(lhs, rhs, ignoreCase, "-gt", opComparer);
        ///
        private static object GreaterThanOp<T>(IComparer<T> c, T l, T r) { return (c.Compare(l, r) > 0); }
        private static object GreaterThanEqualsOp<T>(IComparer<T> c, T l, T r) { return (c.Compare(l, r) >= 0); }
        private static object LessThanOp<T>(IComparer<T> c, T l, T r) { return (c.Compare(l, r) < 0); }
        private static object LessThanEqualsOp<T>(IComparer<T> c, T l, T r) { return (c.Compare(l, r) <= 0); }
        private static Func<IComparer<T>, T, T, object> GetComparerOp<T>(string op)
        {
            if (op.Equals("-gt")) { return GreaterThanOp<T>; }
            if (op.Equals("-ge")) { return GreaterThanEqualsOp<T>; }
            if (op.Equals("-lt")) { return LessThanOp<T>; }
            if (op.Equals("-le")) { return LessThanEqualsOp<T>; }
            throw new NotImplementedException(String.Format("Invalid Operation: {0}", op));
        }


        ///
        /// GreaterThan:
        ///     - method for handling -gt, -igt, -cgt operations
        ///     - called with unpacked lhs/rhs operands and whether to respect case
        ///
        public static object GreaterThan(object lhs, object rhs, bool ignoreCase)
        {
            return ComparisonOperation(lhs, rhs, ignoreCase, "-gt");
        }


        ///
        /// GreaterThanEquals:
        ///     - method for handling -ge, -ige, -cge operations
        ///     - called with unpacked lhs/rhs operands and whether to respect case
        ///
        public static object GreaterThanEquals(object lhs, object rhs, bool ignoreCase)
        {
            return ComparisonOperation(lhs, rhs, ignoreCase, "-ge");
        }


        ///
        /// LessThan:
        ///     - method for handling -lt, -ilt, -clt operations
        ///     - called with unpacked lhs/rhs operands and whether to respect case
        ///
        public static object LessThan(object lhs, object rhs, bool ignoreCase)
        {
            return ComparisonOperation(lhs, rhs, ignoreCase, "-lt");
        }


        ///
        /// LessThanEquals:
        ///     - method for handling -le, -ile, -cle operations
        ///     - called with unpacked lhs/rhs operands and whether to respect case
        ///
        public static object LessThanEquals(object lhs, object rhs, bool ignoreCase)
        {
            return ComparisonOperation(lhs, rhs, ignoreCase, "-le");
        }


        ///
        /// ComparisonOperation:
        ///     - implements type mapping and ordering (primitive overload resolution)
        ///
        ///     7.9 Relational and type-testing Operators
        ///     Description:
        ///
        ///         Rules in Order:
        ///             - LHS is String then (String op String)
        ///             - LHS is DateTiem then (DateTime op DateTime)
        ///             - LHS or RHS is Double then (Double op Double)
        ///             - LHS or RHS is Float then (Float op Float)
        ///             - LHS or RHS is Long then (Long op Long)
        ///             - LHS or RHS is Int then (Int op Int)
        ///             - LHS or RHS is Char then (Char op Char)
        ///             - LHS or RHS is Byte then (Byte op Byte)
        ///             - LHS or RHS is Bool then (Bool op Bool)
        ///         Overload resolution is determined per 7.2.4.
        ///         This operator is left associative.
        ///
        ///      Examples: See ReferenceTests.RelationshipOperatorTests_7_9
        ///
        ///         Operations:  gt, gte, lt, lte, eq, neq
        ///
        ///         string op string, string op *
        ///         datetime op datetime, datetime op *
        ///         double op double, double op *
        ///         float op float, float op *
        ///         long op long, long op *
        ///         int op int, int op *
        ///         char op char, char op *
        ///         byte op byte, byte op *
        ///         bool op bool, bool op *
        ///
        private static object ComparisonOperation(
                object leftValue,
                object rightValue,
                bool ignoreCase,
                string op)
        {
            var lhs = PSObject.Unwrap(leftValue);
            var rhs = PSObject.Unwrap(rightValue);

            ///
            /// allow some null cases to be handled by RHS operand type
            ///
            if (((lhs == null) &&
                 ((rhs == null) ||
                  (rhs is string) ||
                  (rhs is DateTime) ||
                  (rhs is bool))) ||
                (lhs is string))
            {
                Func<IComparer<string>, string, string, object> checkedOp = GetComparerOp<string>(op);
                return Compare<string>(getStringComparer(ignoreCase), lhs, rhs, op, checkedOp);
            }

            if (((lhs == null) && (rhs is DateTime)) ||
                (lhs is DateTime))
            {
                return Compare<DateTime>(lhs, rhs, op);
            }

            ///
            /// these cases will automatically handle the null case if the RHS operand is of the correct type
            ///
            if (lhs is double || rhs is double) { return Compare<double>(lhs, rhs, op); }
            if (lhs is float || rhs is float) { return Compare<float>(lhs, rhs, op); }
            if (lhs is long || rhs is long) { return Compare<long>(lhs, rhs, op); }
            if (lhs is int || rhs is int) { return Compare<int>(lhs, rhs, op); }
            if (lhs is char || rhs is char) { return Compare<char>(lhs, rhs, op); }
            if (lhs is byte || rhs is byte) { return Compare<byte>(lhs, rhs, op); }
            if (lhs is bool || rhs is bool) { return Compare<bool>(lhs, rhs, op); }

            // array comparison
            // hashtable comparison
            throw new NotImplementedException(String.Format("{0} {1} {2}", lhs, op, rhs));
        }


        ///
        /// getStringComparer:
        ///     returns an IComparer<T> for StringComparer based on the case sensititivity flag
        ///     used in the -i*, -c* relationship comparison operators
        ///
        private static IComparer<string> getStringComparer(bool ignoreCase)
        {
            return ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        }


        ///
        /// Compare<T>:
        ///     - generic case Comparison
        ///
        private static object Compare<T>(object lhs, object rhs, string op)
        {
            Func<IComparer<T>, T, T, object> checkedOp = GetComparerOp<T>(op);
            return Compare<T>(Comparer<T>.Default, lhs, rhs, op, checkedOp);
        }


        ///
        /// Compare<T>:
        ///     - generic comparison function
        ///     - callers provide the type specific comparer
        ///     - depends on LanguagePrimitives.ConvertTo for type conversion
        ///     - type conversion can fail in some cases (such as to DateTime)
        ///     - performs the actual comparison operation as +ve, 0, -ve check in checkOp
        ///
        private static object Compare<T>(
                IComparer<T> comparer,
                object lhs,
                object rhs,
                string op,
                Func<IComparer<T>, T, T, object> checkedOp)
        {
            try
            {
                var left = LanguagePrimitives.ConvertTo<T>(lhs);
                var right = LanguagePrimitives.ConvertTo<T>(rhs);
                return checkedOp(comparer, left, right);
            }
            catch (Exception ex)
            {
                ThrowInvalidComparisonOperationException(lhs, rhs, op, ex);
            }
            return null;
        }


        ///
        /// constructs invalid comparison errors similar to Posh
        ///
        private static void ThrowInvalidComparisonOperationException(
                object left,
                object right,
                string op,
                Exception ex)
        {
            var msg = String.Format("Could not compare \"{0}\" to \"{1}\". {2}", left, right, ex.Message);
            throw new PSInvalidOperationException(msg);
        }
    }
}

