// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Exception thrown when attempting to write to a closed pipeline.
    /// </summary>
    [Serializable]
    public class PipelineClosedException : RuntimeException
    {
        public PipelineClosedException()
        {
        }

        public PipelineClosedException(string message)
            : base(message)
        {
        }

        public PipelineClosedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PipelineClosedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

