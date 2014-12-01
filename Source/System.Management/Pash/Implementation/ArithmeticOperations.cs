using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Management.Automation;

namespace System.Management.Pash.Implementation
{
    /*
     * Class implementing the arithmetic operations used in the language
     */
    internal static class ArithmeticOperations
    {
        public static object Add(object leftValuePacked, object rightValuePacked)
        {
            var leftValue = PSObject.Unwrap(leftValuePacked);
            var rightValue = PSObject.Unwrap(rightValuePacked);

            ////  7.7.1 Addition
            ////      Description:
            ////      
            ////          The result of the addition operator + is the sum of the values designated by the two operands after the usual arithmetic conversions (§6.15) have been applied.
            ////      
            ////          This operator is left associative.
            ////      
            ////      Examples: See ReferenceTests.AdditiveOperatorTests_7_7
            ////      
            ////          12 + -10L               # long result 2
            ////          -10.300D + 12           # decimal result 1.700
            ////          10.6 + 12               # double result 22.6
            ////          12 + "0xabc"            # int result 2760

            // not in the specification, but shown by tests: if left is null, return the right object. not a copy.
            // this even works with othe objects as FileInfo
            if (leftValue == null)
            {
                return rightValue;
            }

            // string concatenation 7.7.2
            if (leftValue is string)
            {
                return leftValue + LanguagePrimitives.ConvertTo<string>(rightValue);
            }

            // array concatenation (7.7.3)
            if (leftValue is Array)
            {
                var resultList = new List<object>();
                var enumerable = LanguagePrimitives.GetEnumerable(rightValuePacked);
                foreach (var el in ((Array) leftValue))
                {
                    resultList.Add(el);
                }
                if (enumerable == null)
                {
                    resultList.Add(rightValuePacked);
                }
                else
                {
                    foreach (var el in enumerable)
                    {
                        resultList.Add(el);
                    }
                }
                return resultList.ToArray();
            }
            // hashtable concatenation (7.7.4)
            if (leftValue is Hashtable)
            {
                if (!(rightValue is Hashtable))
                {
                    throw new RuntimeException("Only a hashtable can be added to another hashtable",
                                               "AddHashtableToNonHashtable", ErrorCategory.InvalidOperation, rightValue);
                }
                var resultHash = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
                var leftHash = (Hashtable) leftValue;
                var rightHash = (Hashtable) rightValue;
                foreach (var key in leftHash.Keys)
                {
                    resultHash[key] = leftHash[key];
                }
                foreach (var key in rightHash.Keys)
                {
                    if (resultHash.ContainsKey(key))
                    {
                        var fmt = "Cannot concat hashtables: The key '{0}' is already used in the left Hashtable";
                        throw new InvalidOperationException(String.Format(fmt, key.ToString()));
                    }
                    resultHash[key] = rightHash[key];
                }
                return resultHash;
            }

            // arithmetic expression (7.7.1)
            Func<dynamic, dynamic, dynamic> addOp = (dynamic x, dynamic y) => checked(x + y);
            return ArithmeticOperation(leftValue, rightValue, "+", addOp);
        }

        public static object Multiply(object leftValuePacked, object rightValuePacked)
        {
            var leftValue = PSObject.Unwrap(leftValuePacked);
            var rightValue = PSObject.Unwrap(rightValuePacked);
            // well, this is not part of the specification, but can be shown with tests:
            // if left is null, then null is returned. This even works with other objects like FileInfo on the right
            if (leftValue == null)
            {
                return null;
            }

            // string replication (7.6.2)
            if (leftValue is string)
            {
                long num;
                if (!LanguagePrimitives.TryConvertTo<long>(rightValue, out num))
                {
                    ThrowInvalidArithmeticOperationException(leftValue, rightValue, "*");
                }
                var sb = new StringBuilder();
                for (int i = 0; i < num; i++)
                {
                    sb.Append(leftValue);
                }
                return sb.ToString();
            }

            // array replication (7.6.3)
            if (leftValue is Array)
            {
                long num;
                if (!LanguagePrimitives.TryConvertTo<long>(rightValue, out num))
                {
                    ThrowInvalidArithmeticOperationException(leftValue, rightValue, "*");
                }
                var leftArray = (Array)leftValue;
                long length = leftArray.Length;
                var resultArray = Array.CreateInstance(leftArray.GetType().GetElementType(), length * num);
                for (long i = 0; i < num; i++)
                {
                    Array.Copy(leftArray, 0, resultArray, i * length, length);
                }
                return resultArray;
            }

            // arithmetic expression (7.6.1)
            Func<dynamic, dynamic, dynamic> mulOp = (dynamic x, dynamic y) => checked(x * y);
            return ArithmeticOperation(leftValue, rightValue, "*", mulOp);
        }

        public static object Divide(object leftValuePacked, object rightValuePacked)
        {
            var leftValue = PSObject.Unwrap(leftValuePacked);
            var rightValue = PSObject.Unwrap(rightValuePacked);
            // arithmetic division (7.6.4)
            object convLeft, convRight;
            if (!LanguagePrimitives.UsualArithmeticConversion(leftValue, rightValue, 
                                                              out convLeft, out convRight))
            {
                ThrowInvalidArithmeticOperationException(leftValue, rightValue, "/");
            }
            dynamic left = convLeft;
            dynamic right = convRight;
            // check for zero division first an throw same exception as PS
            if (!(convLeft is double || convLeft is float) && right == 0)
            {
                throw new RuntimeException("Attempted to divide by zero");
            }
            // if left is decimal, do decimal operation
            if (convLeft is decimal)
            {
                return left / right;
            }
            // otherwise int/long/double. Making sure double conversion takes place
            // iff integer division would have a remainder
            return left % right == 0 ? left / right : ((double)left) / right;
        }

        public static object Remainder(object leftValuePacked, object rightValuePacked)
        {
            var leftValue = PSObject.Unwrap(leftValuePacked);
            var rightValue = PSObject.Unwrap(rightValuePacked);
            // arithmetic remainder (7.6.5)
            Func<dynamic, dynamic, dynamic> remOp = (dynamic x, dynamic y) => checked(x % y);
            return ArithmeticOperation(leftValue, rightValue, "%", remOp);
        }

        public static object Subtract(object leftValuePacked, object rightValuePacked)
        {
            var leftValue = PSObject.Unwrap(leftValuePacked);
            var rightValue = PSObject.Unwrap(rightValuePacked);
            // arithmetic expression (7.7.5)
            Func<dynamic, dynamic, dynamic> subOp = (dynamic x, dynamic y) => checked(x - y);
            return ArithmeticOperation(leftValue, rightValue, "-", subOp);
        }

        private static void ThrowInvalidArithmeticOperationException(object left, object right, string op)
        {
            var msg = String.Format("Operation [{0}] {1} [{2}] is not defined",
                                    left == null ? "null" : left.GetType().FullName, op,
                                    right == null ? "null" : right.GetType().FullName);
            throw new PSInvalidOperationException(msg);
        }

        private static object ArithmeticOperation(object leftUnconverted, object rightUnconverted, string op,
                                           Func<dynamic, dynamic, dynamic> checkedOperation)
        {
            object left, right;
            if (!LanguagePrimitives.UsualArithmeticConversion(leftUnconverted, rightUnconverted, 
                                                              out left, out right))
            {
                ThrowInvalidArithmeticOperationException(leftUnconverted, rightUnconverted, op);
            }
            dynamic result;
            if (left is int && right is int)
            {
                if (TryRunCheckedOperationCheckOverflow(checkedOperation, left, right, out result))
                {
                    return result;
                }
                left = (long)((int) left); // int cast -> object to int, then to long
                right = (long)((int) right);
            }
            if ((left is int || left is long) && (right is int || right is long))
            {
                if (TryRunCheckedOperationCheckOverflow(checkedOperation, left, right, out result))
                {
                    return result;
                }
                // first dynamic cast, as the objects might be ints or longs
                left = (double) ((dynamic) left);
                right = (double) ((dynamic)right);
            }
            // otherwise its float, double, decimal, which cannot overflow
            TryRunCheckedOperationCheckOverflow(checkedOperation, left, right, out result);
            return result;
        }

        static private bool TryRunCheckedOperationCheckOverflow(Func<dynamic, dynamic, dynamic> checkedOperation,
                                                                dynamic left, dynamic right, out dynamic result)
        {
            result = null;
            try
            {
                result = checkedOperation(left, right);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (DivideByZeroException)  // PS throws a RuntimeException instead
            {
                throw new RuntimeException("Attempted to divide by zero");
            }
        }
    }
}

