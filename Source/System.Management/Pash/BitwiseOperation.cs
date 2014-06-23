using System;
using System.Linq;
using System.Management.Automation;

namespace Pash
{
    class BitwiseOperation
    {
        object leftOperand;
        object rightOperand;
        int? leftOperandInt;
        int? rightOperandInt;
        long? leftOperandLong;
        long? rightOperandLong;

        private BitwiseOperation(object leftOperand, object rightOperand)
        {
            this.leftOperand = leftOperand;
            this.rightOperand = rightOperand;
        }

        public static object And(object leftOperand, object rightOperand)
        {
            return new BitwiseOperation(leftOperand, rightOperand).And();
        }

        private object And()
        {
            ConvertOperandsToIntegers();

            if (UseLongs())
            {
                return leftOperandLong & rightOperandLong;
            }

            return leftOperandInt.Value & rightOperandInt.Value;
        }

        private void ConvertOperandsToIntegers()
        {
            leftOperandInt = TryConvertTo<int>(leftOperand);
            rightOperandInt = TryConvertTo<int>(rightOperand);
            if (leftOperandInt.HasValue && rightOperandInt.HasValue)
                return;

            leftOperandLong = TryConvertTo<long>(leftOperand);
            rightOperandLong = TryConvertTo<long>(rightOperand);
            if (leftOperandLong.HasValue && rightOperandLong.HasValue)
                return;

            throw new NotImplementedException();
        }

        private bool UseLongs()
        {
            return leftOperandLong.HasValue || rightOperandLong.HasValue;
        }

        private T? TryConvertTo<T>(object valueToConvert)
            where T : struct
        {
            T value;
            if (LanguagePrimitives.TryConvertTo<T>(valueToConvert, out value))
            {
                return value;
            }
            return null;
        }

        public static object Or(object leftOperand, object rightOperand)
        {
            return new BitwiseOperation(leftOperand, rightOperand).Or();
        }

        private object Or()
        {
            ConvertOperandsToIntegers();

            if (UseLongs())
            {
                return leftOperandLong | rightOperandLong;
            }

            return leftOperandInt.Value | rightOperandInt.Value;
        }

        public static object Xor(object leftOperand, object rightOperand)
        {
            return new BitwiseOperation(leftOperand, rightOperand).Xor();
        }

        private object Xor()
        {
            ConvertOperandsToIntegers();

            if (UseLongs())
            {
                return leftOperandLong ^ rightOperandLong;
            }

            return leftOperandInt.Value ^ rightOperandInt.Value;
        }
    }
}
