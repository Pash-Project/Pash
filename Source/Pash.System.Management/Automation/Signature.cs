// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

// Todo: Needs an implementation

using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace System.Management.Automation
{
    public sealed class Signature
    {
        public string Path { get; private set; }

        public X509Certificate2 SignerCertificate { get; private set; }

        public SignatureStatus Status { get; private set; }

        public string StatusMessage { get; private set; }

        public X509Certificate2 TimeStamperCertificate { get; private set; }  
    }
}

