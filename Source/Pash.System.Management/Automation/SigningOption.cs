// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
	/// <summary>
	/// Options for accepting signed Pash objects.
	/// </summary>
    public enum SigningOption
    {
        AddFullCertificateChain = 1,
        AddFullCertificateChainExceptRoot = 2,
        AddOnlyCertificate = 0,
        Default = 2
    }
}

