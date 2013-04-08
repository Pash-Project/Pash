// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when there are two providers with the same name.
    /// </summary>
    [Serializable]
    public class ProviderNameAmbiguousException : ProviderNotFoundException
    {
        public ReadOnlyCollection<ProviderInfo> PossibleMatches { get; internal set; }

        public ProviderNameAmbiguousException()
        {
        }

        public ProviderNameAmbiguousException(string message)
            : base(message)
        {
        }

        public ProviderNameAmbiguousException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ProviderNameAmbiguousException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

