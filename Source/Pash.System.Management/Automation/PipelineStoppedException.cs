// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PipelineStoppedException : RuntimeException
    {
        public PipelineStoppedException()
        {
            base.Id = "PipelineStopped";
            base.Category = ErrorCategory.OperationStopped;
        }

        public PipelineStoppedException(string message)
            : base(message)
        {
        }

        protected PipelineStoppedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public PipelineStoppedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

