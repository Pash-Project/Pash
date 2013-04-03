// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when there is an error invoking a provider.
    /// </summary>
    [Serializable]
    public class ProviderInvocationException : RuntimeException
    {
        public ProviderInvocationException()
        {
        }

        public ProviderInvocationException(string message) : base(message)
        {
            _message = message;
        }

        public ProviderInvocationException(string message, Exception innerException) 
            : base(message, innerException)
        {
            _message = message;
        }

        protected ProviderInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //todo: implement
        private ErrorRecord errorRecord;
        public override ErrorRecord ErrorRecord
        {
            get
            {
                return errorRecord;
            }
        }

        private string _message;
        public override string Message
        {
            get
            {
                if (_message == null)
                    return base.Message;
                return _message;
            }
        }

        public ProviderInfo ProviderInfo { get; private set; }
    }
}

