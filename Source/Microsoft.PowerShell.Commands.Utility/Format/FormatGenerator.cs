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
                errorRecord = ((IContainsErrorRecord)errorData).ErrorRecord;
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
                // set for the next items
                Options.Properties = (from prop in defProps select prop.Name).ToArray();
                return defProps;
            }
            var props = new Collection<PSPropertyInfo>();
            foreach (var cur in Options.Properties)
            {
                props.Add(EvaluateProperty(psobj, cur));
            }
            return props;
        }

        private PSPropertyInfo EvaluateProperty(PSObject psobj, object property)
        {
            var propName = property as string;
            if (propName != null)
            {
                var prop = psobj.Properties[propName];
                return prop ?? new PSNoteProperty(propName, "");
            }
            // otherwise it is a scriptblock that can be evaluated
            var scriptBlock = property as ScriptBlock;
            if (scriptBlock == null)
            {
                var msg = String.Format("The property '{0}' couldn't be converted to a string or a ScriptBlock",
                                        property.GetType().Name);
                throw new PSInvalidOperationException(msg);
            }
            propName = scriptBlock.ToString();
            var pipeline = ExecutionContext.CurrentRunspace.CreateNestedPipeline();
            pipeline.Commands.Add(new Command(scriptBlock.Ast as ScriptBlockAst, true));
            // TODO: hack? we need to set the $_ variable
            var oldVar = ExecutionContext.GetVariable("_");
            ExecutionContext.SetVariable("_", psobj);
            Collection<PSObject> results = null;
            try
            {
                results = pipeline.Invoke();
            }
            finally
            {
                ExecutionContext.SessionState.PSVariable.Set(oldVar);
            }
            PSObject value;
            if (results.Count == 0)
            {
                value = PSObject.AsPSObject(null);
            }
            else if (results.Count == 1)
            {
                value = results[1];
            }
            else
            {
                value = PSObject.AsPSObject(new List<PSObject>(results).ToArray());
            }
            return new PSNoteProperty(propName, value);
        }
    }
}

