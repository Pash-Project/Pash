// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// The status of a given Pash object's signature.
    /// </summary>
    public enum SignatureStatus
    {
        Valid,
        UnknownError,
        NotSigned,
        HashMismatch,
        NotTrusted,
        NotSupportedFileFormat
    }
}
