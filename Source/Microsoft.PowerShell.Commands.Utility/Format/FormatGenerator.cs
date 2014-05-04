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
        public abstract FormatEntryData GenerateFormatEntry(PSObject data);
        public abstract GroupEndData GenerateGroupEnd();

        public virtual FormatEndData GenerateFormatEnd()
        {
            return new FormatEndData(Shape);
        }
    }
}

