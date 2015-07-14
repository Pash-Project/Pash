// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Net;
using System.Security;

namespace System.Management.Automation
{
    public sealed class PSCredential
    {
		string username;
		SecureString password;

		public string UserName
		{
			get
			{
				return this.username;
			}
		}

		public SecureString Password
		{
			get
			{
				return this.password;
			}
		}

        private static PSCredential _emptyCredential = new PSCredential();
		public static PSCredential Empty
		{
			get
			{
                return _emptyCredential;
			}
		}

        private PSCredential()
        {
        }

		public PSCredential(string userName, SecureString password)
		{
			this.username = userName;
			this.password = password;
		}

        public PSCredential(PSCredential credential)
        {
            username = credential.username;
            password = credential.Password.Copy();
        }

		public NetworkCredential GetNetworkCredential()
		{
			throw new NotImplementedException ();
		}

		public static explicit operator NetworkCredential(PSCredential credential)
		{
			throw new NotImplementedException ();
		}
    }
}
