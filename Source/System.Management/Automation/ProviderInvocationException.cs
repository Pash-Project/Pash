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

        public ProviderInvocationException(string message)
            : base(message)
        {
            _message = message;
        }

        public ProviderInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
            _message = message;
        }

        internal ProviderInvocationException(ErrorRecord errorRecord)
            : base(errorRecord.Exception.Message, errorRecord.Exception)
        {
            _errorRecord = errorRecord;
        }

        protected ProviderInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _errorRecord = null;
        }

        //todo: implement
        private ErrorRecord _errorRecord;
        public override ErrorRecord ErrorRecord
        {
            get
            {
                return _errorRecord;
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

