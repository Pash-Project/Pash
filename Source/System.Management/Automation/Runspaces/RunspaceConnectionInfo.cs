// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace System.Management.Automation.Runspaces
{
    public abstract class RunspaceConnectionInfo
    {
        public abstract string ComputerName
        {
            get;
        }

        public abstract PSCredential Credential
        {
            get;
        }

        public abstract AuthenticationMechanism AuthenticationMechanism
        {
            get;
            set;
        }

        public abstract string CertificateThumbprint
        {
            get;
        }

        public CultureInfo Culture
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public CultureInfo UICulture
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int OpenTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int CancelTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int OperationTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int IdleTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
