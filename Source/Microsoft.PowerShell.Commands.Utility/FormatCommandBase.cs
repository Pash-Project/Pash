using System;
using System.Management.Automation;
using System.Configuration;
using System.Xml;

namespace Microsoft.PowerShell.Commands.Utility
{
    public class FormatCommandBase : PSCmdlet
    {
        private FormattingState _state;
        private FormatGenerator _generator;

        internal FormatShape Shape { get; private set; }
        internal FormatGeneratorOptions Options { get; set; }
        
        [ParameterAttribute(ValueFromPipeline=true)] 
        public PSObject InputObject { get; set; }

        internal FormatCommandBase(FormatShape shape)
        {
            Shape = shape;
            Options = new FormatGeneratorOptions();
            _state = FormattingState.FormatEnd;
        }

        protected override void ProcessRecord()
        {
            if (InputObject.BaseObject is FormatData)
            {
                WriteObject(InputObject);
                return;
            }
            if (_generator == null)
            {
                if (Shape.Equals(FormatShape.Undefined))
                {
                    Shape = FormatShapeHelper.SelectByData(InputObject);
                }
                _generator = FormatGenerator.Get(Shape, Options);
            }
            if (_state.Equals(FormattingState.FormatEnd))
            {
                WriteObject(_generator.GenerateFormatStart());
                _state = FormattingState.FormatStart;
            }
            if (_state.Equals(FormattingState.GroupStart) && ShouldChangeGroup(InputObject))
            {
                WriteObject(_generator.GenerateGroupEnd());
                _state = FormattingState.GroupEnd;
            }
            if (_state.Equals(FormattingState.GroupEnd) || _state.Equals(FormattingState.FormatStart))
            {
                WriteObject(_generator.GenerateGroupStart(InputObject));
                _state = FormattingState.GroupStart;
            }
            // we have to be in the state GroupStart where we can write the data itself
            WriteObject(_generator.GenerateFormatEntry(InputObject));
        }

        protected override void EndProcessing()
        {
            if (_state.Equals(FormattingState.GroupStart))
            {
                // generator cannot be null
                WriteObject(_generator.GenerateGroupEnd());
                _state = FormattingState.GroupEnd;
            }
            if (_state.Equals(FormattingState.GroupEnd) || _state.Equals(FormattingState.FormatStart))
            {
                WriteObject(_generator.GenerateFormatEnd());
                _state = FormattingState.FormatEnd;
            }
        }

        private bool ShouldChangeGroup(PSObject obj)
        {
            return false; // TODO: sburnicki
        }
    }
}

