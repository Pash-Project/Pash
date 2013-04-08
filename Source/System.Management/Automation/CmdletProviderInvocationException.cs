// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// This is thrown when something is wrong with the provider the cmdlet invoked.
    /// </summary>
    [Serializable]
    public class CmdletProviderInvocationException : CmdletInvocationException
    {
        public ProviderInvocationException ProviderInvocationException { get; private set; }

        public CmdletProviderInvocationException()
        {
        }

        public CmdletProviderInvocationException(string message)
            : base(message)
        {
        }

        protected CmdletProviderInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ProviderInvocationException = base.InnerException as ProviderInvocationException;
        }

        public CmdletProviderInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
            ProviderInvocationException = base.InnerException as ProviderInvocationException;
        }

        public System.Management.Automation.ProviderInfo ProviderInfo
        {
            get
            {
                if (ProviderInvocationException != null)
                {
                    return ProviderInvocationException.ProviderInfo;
                }

                return null;
            }
        }
    }
}

