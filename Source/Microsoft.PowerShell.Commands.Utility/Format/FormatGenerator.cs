using System;
using System.Linq;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Pash.Implementation;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Language;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class FormatGenerator
    {
        internal FormatShape Shape { get; private set; }
        internal FormatGeneratorOptions Options { get; private set; }
        internal ExecutionContext ExecutionContext { get; private set; }

        private static FormatGenerator _lastGenerator;

        protected FormatGenerator(ExecutionContext context, FormatShape shape, FormatGeneratorOptions options)
        {
            Options = options;
            Shape = shape;
            ExecutionContext = context;
        }

        public static FormatGenerator Get(ExecutionContext context, FormatShape shape, FormatGeneratorOptions options)
        {
            if (_lastGenerator != null && _lastGenerator.Shape.Equals(shape) && _lastGenerator.Options.Equals(options))
            {
                return _lastGenerator;
            }
            switch (shape)
            {
                case FormatShape.List:
                    _lastGenerator = new ListFormatGenerator(context, options);
                    break;
                case FormatShape.Table:
                    _lastGenerator = new TableFormatGenerator(context, options);
                    break;
                default:
                    throw new PSInvalidOperationException("Cannot get a FormatGenerator with undefined shape");
            }
            return _lastGenerator;
        }

        public virtual FormatStartData GenerateFormatStart()
        {
            return new FormatStartData(Shape);
        }

        public virtual GroupStartData GenerateGroupStart(PSObject data)
        {
            return new GroupStartData(Shape);
        }

        public virtual SimpleFormatEntryData GenerateSimpleFormatEntry(PSObject data)
        {
            return new SimpleFormatEntryData(Shape, data.ToString()) {
                WriteToErrorStream = data.WriteToErrorStream
            };
        }

        public virtual ErrorFormatEntryData GenerateErrorFormatEntry(PSObject errorData)
        {
            ErrorRecord errorRecord;
            if (errorData.BaseObject is ErrorRecord)
            {
                errorRecord = (ErrorRecord)errorData.BaseObject;
            }
            else if (errorData.BaseObject is IContainsErrorRecord)
            {
                errorRecord = ((IContainsErrorRecord)errorData.BaseObject).ErrorRecord;
            }
            else if (errorData.BaseObject is Exception)
            {
                errorRecord = new ErrorRecord((Exception)errorData.BaseObject, "Exception",
                                              ErrorCategory.NotSpecified, null);
            }
            else
            {
                var msg = "Cannot generate ErrorFormatEntry from non-error. This is a bug, please report this issue!";
                throw new NotImplementedException(msg);
            }
            var message = errorRecord.Exception == null ? "Unknown Error" : errorRecord.Exception.Message;
            var entry = new ErrorFormatEntryData(Shape, message, errorRecord.CategoryInfo.ToString(),
                                            errorRecord.FullyQualifiedErrorId);
            entry.WriteToErrorStream = errorData.WriteToErrorStream;
            return entry;
        }

        public abstract FormatEntryData GenerateObjectFormatEntry(PSObject data);

        public virtual GroupEndData GenerateGroupEnd()
        {
            return new GroupEndData(Shape);
        }

        public virtual FormatEndData GenerateFormatEnd()
        {
            return new FormatEndData(Shape);
        }

        protected Collection<PSPropertyInfo> GetSelectedProperties(PSObject psobj)
        {
            if (Options.Properties == null || Options.Properties.Length < 1)
            {
                var defProps = psobj.GetDefaultDisplayPropertySet();
                return defProps;
            }
            return psobj.SelectProperties(Options.Properties, ExecutionContext);
        }
    }
}

