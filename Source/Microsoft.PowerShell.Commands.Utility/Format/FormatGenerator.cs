using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class FormatGenerator
    {
        internal FormatShape Shape { get; private set; }
        internal FormatGeneratorOptions Options { get; private set; }

        private static FormatGenerator _lastGenerator;

        protected FormatGenerator(FormatShape shape, FormatGeneratorOptions options)
        {
            Options = options;
            Shape = shape;
        }

        public static FormatGenerator Get(FormatShape shape, FormatGeneratorOptions options)
        {
            if (_lastGenerator != null && _lastGenerator.Shape.Equals(shape) && _lastGenerator.Options.Equals(options))
            {
                return _lastGenerator;
            }
            switch (shape)
            {
                case FormatShape.List:
                    _lastGenerator = new ListFormatGenerator(options);
                    break;
                case FormatShape.Table:
                    _lastGenerator = new TableFormatGenerator(options);
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

        public abstract GroupStartData GenerateGroupStart(PSObject data);

        public FormatEntryData GenerateFormatEntry(PSObject data)
        {
            var val = data.BaseObject ?? "";
            var type = val.GetType();
            if (type.IsPrimitive || type == typeof(string))
            {
                return GenerateSimpleFormatEntry(data);
            }

            return GenerateObjectFormatEntry(data);
        }

        public virtual SimpleFormatEntryData GenerateSimpleFormatEntry(PSObject data)
        {
            return new SimpleFormatEntryData(Shape, data.ToString()) {
                WriteToErrorStream = data.WriteToErrorStream
            };
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
    }
}

