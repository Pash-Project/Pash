using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Collections;
using System.Collections.Generic;

namespace System.Management.Pash.Implementation
{
    public class SettableIndexExpression : SettableExpression
    {
        private IndexExpressionAst _expressionAst;

        private bool _targetIsEvaluated;
        private object _targetValue;
        public object EvaluatedTargetValue
        {
            get
            {
                return GetEventuallyEvaluatedValue(_expressionAst.Target, ref _targetIsEvaluated, ref _targetValue);
            }
        }

        private bool _indexIsEvaluated;
        private object _indexValue;
        public object EvaluatedIndexValue
        {
            get
            {
                return GetEventuallyEvaluatedValue(_expressionAst.Index, ref _indexIsEvaluated, ref _indexValue);
            }
        }

        internal SettableIndexExpression(IndexExpressionAst expressionAst, ExecutionVisitor currentExecution)
            : base (currentExecution)
        {
            _expressionAst = expressionAst;
        }

        public override object GetValue()
        {
            object targetValue = PSObject.Unwrap(EvaluatedTargetValue);
            object index = PSObject.Unwrap(EvaluatedIndexValue);

            // Check dicts/hashtables first
            if (targetValue is IDictionary)
            {
                var dictTarget = (IDictionary) targetValue;
                var idxobjects = index is object[] ? (object[])index : new object[] { index };
                return (from idxobj in idxobjects select dictTarget[PSObject.Unwrap(idxobj)]).ToArray();
            }

            // otherwise we need int indexing
            var indices = LanguagePrimitives.ConvertTo<int[]>(index);

            // check for real multidimensional array access first
            var arrayTarget = targetValue as Array;
            if (arrayTarget != null && arrayTarget.Rank > 1)
            {
                int[] convIndices = ConvertNegativeIndicesForMultidimensionalArray(arrayTarget, indices);
                try
                {
                    return arrayTarget.GetValue(convIndices);
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
            }

            // if we have a string, we need to access it as an char[]
            if (targetValue is string)
            {
                targetValue = ((string)targetValue).ToCharArray();
            }

            // now we can access arrays, list, and string (charArrays) all the same by using the IList interface
            var iListTarget = targetValue as IList;
            if (iListTarget == null)
            {
                var msg = String.Format("Cannot index an object of type '{0}'.", targetValue.GetType().FullName);
                throw new PSInvalidOperationException(msg);
            }

            var numItems = iListTarget.Count;
            // now get all elements from the index list
            List<object> values = new List<object>();
            foreach (int curIdx in indices)
            {
                // support negative indices
                var idx = curIdx < 0 ? curIdx + numItems : curIdx;
                // check if it's a valid index, otherwise ignore
                if (idx >= 0 && idx < numItems)
                {
                    values.Add(iListTarget[idx]);
                }
            }
            return values.ToArray();
        }

        public override void SetValue(object value)
        {
            var target = PSObject.Unwrap(EvaluatedTargetValue);
            var index = PSObject.Unwrap(EvaluatedIndexValue);
            var arrayTarget = target as Array;
            if (arrayTarget != null && arrayTarget.Rank > 1)
            {
                var rawIndices = (int[])LanguagePrimitives.ConvertTo(index, typeof(int[]));
                var indices = ConvertNegativeIndicesForMultidimensionalArray(arrayTarget, rawIndices);
                var convertedValue = LanguagePrimitives.ConvertTo(value, arrayTarget.GetType().GetElementType());
                arrayTarget.SetValue(convertedValue, indices);
                return;
            }
            // we don't support assignment to slices (, yet?), so throw an error
            if (index is Array && ((Array)index).Length > 1)
            {
                throw new PSInvalidOperationException("Assignment to slices is not supported");
            }
            // otherwise do assignments
            var iListTarget = target as IList;
            if (iListTarget != null) // covers also arrays
            {
                var intIdx = (int)LanguagePrimitives.ConvertTo(index, typeof(int));
                intIdx = intIdx < 0 ? intIdx + iListTarget.Count : intIdx;
                iListTarget[intIdx] = value;
            }
            else if (target is IDictionary)
            {
                ((IDictionary)target)[index] = value;
            }
            else
            {
                var msg = String.Format("Cannot set index for type '{0}'", target.GetType().FullName);
                throw new PSInvalidOperationException(msg);
            }
        }

        private int[] ConvertNegativeIndicesForMultidimensionalArray(Array array, int[] rawIndices)
        {
            if (array.Rank != rawIndices.Length)
            {
                var msg = String.Format("The index [{0}] is invalid to access an {1}-dimensional array",
                                        String.Join(",", rawIndices), array.Rank);
                throw new PSInvalidOperationException(msg);
            }
            var convIndices = new int[array.Rank];
            for (int i = 0; i < rawIndices.Length; i++)
            {
                convIndices[i] = rawIndices[i] < 0 ? rawIndices[i] + array.GetLength(i) : rawIndices[i];
            }
            return convIndices;
        }
    }
}

