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
            else if (data is SimpleFormatEntryData)
            {
                // may occur in any state
                ProcessFormatEntry((SimpleFormatEntryData)data);
            }
            else if (data is ErrorFormatEntryData)
            {
                // may occur in any state
                ProcessFormatEntry((ErrorFormatEntryData)data);
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
                throw new InvalidOperationException(msg);
            }
        }

        protected virtual void ProcessFormatStart(FormatStartData data)
        {
        }

        protected virtual void ProcessGroupStart(GroupStartData data)
        {
        }

        protected virtual void ProcessFormatEntry(SimpleFormatEntryData data)
        {
            OutputWriter.WriteToErrorStream = data.WriteToErrorStream;
            if (!String.IsNullOrEmpty(data.Value))
            {
                OutputWriter.WriteLine(data.Value);
            }
        }

        protected virtual void ProcessFormatEntry(ErrorFormatEntryData data)
        {
            OutputWriter.WriteToErrorStream = data.WriteToErrorStream;
            OutputWriter.WriteLine(data.Message);
            OutputWriter.WriteLine("  +CategoryInfo: " + data.CategoryInfo);
            OutputWriter.WriteLine("  +FullyQualifiedErrorId: " + data.FullyQualifiedErrorId);
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
                var msg = String.Format("The format data is invalid. Did you modify the output of an Format-* command?",
                                        currentData.GetType().Name, _state);
                throw new InvalidOperationException(msg);
            }
        }
    }
}

