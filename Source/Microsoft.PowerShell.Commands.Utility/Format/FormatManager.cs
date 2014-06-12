using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Pash.Implementation;
using Extensions.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class FormatManager
    {
        private FormattingState _state;
        private FormatGenerator _generator;
        private ExecutionContext _executionContext;

        internal FormatShape Shape { get; private set; }
        internal FormatGeneratorOptions Options { get; set; }

        internal FormatManager(FormatShape shape, ExecutionContext context)
        {
            Shape = shape;
            Options = new FormatGeneratorOptions();
            _state = FormattingState.FormatEnd;
        }

        public Collection<FormatData> Process(PSObject psobj)
        {
            // unroll one level. But more than LanguagePrimitives would unroll!
            var enumerator = LanguagePrimitives.GetEnumerator(psobj);
            if (enumerator == null)
            {
                var baseObj = PSObject.Unwrap(psobj);
                var enumerable = baseObj as IEnumerable;
                if (!(baseObj is string) && enumerable != null)
                {
                    enumerator = enumerable.GetEnumerator();
                }
            }
            if (enumerator == null)
            {
                return ProcessObject(psobj);
            }
            var results = new List<FormatData>();
            while (enumerator.MoveNext())
            {
                var curPSobj = PSObject.AsPSObject(enumerator.Current);
                results.AddRange(ProcessObject(curPSobj));
            }
            return new Collection<FormatData>(results);
        }

        public Collection<FormatData> End()
        {
            var formatData = new Collection<FormatData>();
            if (_state.Equals(FormattingState.GroupStart))
            {
                // generator cannot be null
                formatData.Add(_generator.GenerateGroupEnd());
                _state = FormattingState.GroupEnd;
            }
            if (_state.Equals(FormattingState.GroupEnd) || _state.Equals(FormattingState.FormatStart))
            {
                formatData.Add(_generator.GenerateFormatEnd());
                _state = FormattingState.FormatEnd;
            }
            return formatData;
        }

        public void SetExecutionContext(ExecutionContext context)
        {
            _executionContext = context;
        }

        private bool ShouldChangeGroup(PSObject obj)
        {
            // TODO: implement grouping
            return false;
        }

        private Collection<FormatData> ProcessObject(PSObject psobj)
        {
            var formatData = new Collection<FormatData>();
            if (psobj.BaseObject is FormatData)
            {
                formatData.Add((FormatData)psobj.BaseObject);
                return formatData;
            }
            if (_generator == null)
            {
                if (Shape.Equals(FormatShape.Undefined))
                {
                    Shape = FormatShapeHelper.SelectByData(psobj);
                }
                _generator = FormatGenerator.Get(_executionContext, Shape, Options);
            }
            // check if we have a simple type or an error type. if yes, then we don't need a document structure
            if (psobj.BaseObject == null || psobj.BaseObject.GetType().IsSimple())
            {
                formatData.Add(_generator.GenerateSimpleFormatEntry(psobj));
                return formatData;
            }
            if (psobj.BaseObject is ErrorRecord || psobj.BaseObject is Exception)
            {
                formatData.Add(_generator.GenerateErrorFormatEntry(psobj));
                return formatData;
            }

            // object to be printed get a complete document structure
            if (_state.Equals(FormattingState.FormatEnd))
            {
                formatData.Add(_generator.GenerateFormatStart());
                _state = FormattingState.FormatStart;
            }
            if (_state.Equals(FormattingState.GroupStart) && ShouldChangeGroup(psobj))
            {
                formatData.Add(_generator.GenerateGroupEnd());
                _state = FormattingState.GroupEnd;
            }
            if (_state.Equals(FormattingState.GroupEnd) || _state.Equals(FormattingState.FormatStart))
            {
                formatData.Add(_generator.GenerateGroupStart(psobj));
                _state = FormattingState.GroupStart;
            }
            // we have to be in the state GroupStart where we can write the data itself
            formatData.Add(_generator.GenerateObjectFormatEntry(psobj));
            return formatData;
        }
    }
}

