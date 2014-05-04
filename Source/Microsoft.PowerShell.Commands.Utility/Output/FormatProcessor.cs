using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class FormatProcessor
    {
        private FormattingState _state;
        private static FormatProcessor _lastProcessor;

        internal OutputWriter OutputWriter { get; private set; }
        internal FormatShape Shape { get; private set; }

        protected FormatProcessor(FormatShape shape, OutputWriter outputWriter)
        {
            Shape = shape;
            OutputWriter = outputWriter;
            _state = FormattingState.FormatEnd;
        }

        public static FormatProcessor Get(OutputWriter outputWriter, FormatShape shape)
        {
            // we use cached instances as the instance is able then to continue old output
            if (_lastProcessor != null && _lastProcessor.Shape.Equals(shape) &&
                _lastProcessor.OutputWriter.Equals(outputWriter))
            {
                return _lastProcessor;
            }
            switch (shape)
            {
                case FormatShape.List:
                    _lastProcessor = new ListFormatProcessor(outputWriter);
                    break;
                case FormatShape.Table:
                    _lastProcessor = new TableFormatProcessor(outputWriter);
                    break;
                default:
                    throw new PSInvalidOperationException("Cannot get FormatProcessor for undefined shape");
                    break;
            }
            return _lastProcessor;
        }

        public void ProcessPayload(FormatData data)
        {
            if (data is FormatStartData)
            {
                VerifyState(FormattingState.FormatEnd, data);
                ProcessFormatStart((FormatStartData)data);
                _state = FormattingState.FormatStart;
            }
            else if (data is GroupStartData)
            {
                var groupData = (GroupStartData)data;
                VerifyState(FormattingState.FormatStart | FormattingState.GroupEnd, data);
                ProcessGroupStart(groupData);
                _state = FormattingState.GroupStart;
            }
            else if (data is FormatEntryData)
            {
                VerifyState(FormattingState.GroupStart, data);
                ProcessFormatEntry((FormatEntryData)data);
            }
            else if (data is GroupEndData)
            {
                VerifyState(FormattingState.GroupStart, data);
                ProcessGroupEnd((GroupEndData)data);
                _state = FormattingState.GroupEnd;
            }
            else if (data is FormatEndData)
            {
                VerifyState(FormattingState.GroupEnd, data);
                ProcessFormatEnd((FormatEndData)data);
                _state = FormattingState.GroupEnd;
            }
            else
            {
                var msg = String.Format("The format data of type {0} is unknown and cannot be processed",
                                        data.GetType().Name);
                throw new InvalidOperationException();
            }
        }

        protected virtual void ProcessFormatStart(FormatStartData data)
        {
        }

        protected virtual void ProcessGroupStart(GroupStartData data)
        {
        }

        protected abstract void ProcessFormatEntry(FormatEntryData data);

        protected virtual void ProcessGroupEnd(GroupEndData data)
        {
        }

        protected virtual void ProcessFormatEnd(FormatEndData data)
        {
        }

        private void VerifyState(FormattingState allowedStates, FormatData currentData)
        {
            if (!allowedStates.HasFlag(_state))
            {
                var msg = String.Format("The format data is invalid. Format data of type {0} is not allowed in state {1}",
                                        currentData.GetType().Name, _state);
                throw new InvalidOperationException(msg);
            }
        }
    }
}

