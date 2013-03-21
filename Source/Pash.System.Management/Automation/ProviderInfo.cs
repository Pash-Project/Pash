// Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

#region BSD License
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, (either expressed or implied, of the FreeBSD Project.
#endregion

#region GPL License
// This file is part of Pash.
//
// Pash is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Pash is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along
// with Pash.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    public class ProviderInfo
    {
        public PSSnapInInfo PSSnapIn { get; private set; }
        public string Name { get; private set; }
        public string Description { get; set; }
        public string Home { get; set; }
        public Type ImplementingType { get; private set; }
        public ProviderCapabilities Capabilities { get; private set; }
        public string HelpFile { get; private set; }
        private SessionState _sessionState;

        public Collection<PSDriveInfo> Drives
        {
            get
            {
                return _sessionState.Drive.GetAllForProvider(FullName);
            }
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string helpFile, PSSnapInInfo psSnapIn)
            : this(sessionState, implementingType, name, string.Empty, string.Empty, helpFile, psSnapIn)
        {
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string description, string home, string helpFile, PSSnapInInfo psSnapIn)
        {
            _sessionState = sessionState;
            PSSnapIn = psSnapIn;
            Name = name;
            Description = description;
            Home = home;
            ImplementingType = implementingType;
            Capabilities = GetCapabilities(implementingType);
            HelpFile = helpFile;
        }

        protected ProviderInfo(ProviderInfo providerInfo)
        {
            _sessionState = providerInfo._sessionState;
            PSSnapIn = providerInfo.PSSnapIn;
            Name = providerInfo.Name;
            Description = providerInfo.Description;
            Home = providerInfo.Home;
            ImplementingType = providerInfo.ImplementingType;
            Capabilities = providerInfo.Capabilities;
            HelpFile = providerInfo.HelpFile;
        }

        public override string ToString()
        {
            return FullName;
        }

        internal string PSSnapInName
        {
            get
            {
                if (PSSnapIn != null)
                {
                    return PSSnapIn.Name;
                }
                return null;
            }
        }

        internal string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(PSSnapInName))
                {
                    return string.Format(@"{0}\{1}", PSSnapInName, Name);
                }
                return Name;
            }
        }

        internal CmdletProvider CreateInstance()
        {
            Exception outException = null;
            object objProvider = null;
            try
            {
                objProvider = Activator.CreateInstance(ImplementingType);
            }
            catch (Exception ex)
            {
                outException = ex;
            }
            if (objProvider == null)
            {
                if (outException != null)
                {
                    throw new ProviderNotFoundException(Name, SessionStateCategory.CmdletProvider, "ProviderCtorException", outException.Message);
                }
                else
                {
                    throw new ProviderNotFoundException(Name, SessionStateCategory.CmdletProvider, "ProviderNotFoundInAssembly");
                }
            }
            CmdletProvider provider = (CmdletProvider)objProvider;
            provider.SetProviderInfo(this);
            return provider;
        }

        public static ProviderCapabilities GetCapabilities(Type type)
        {
            try
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(CmdletProviderAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length == 1))
                {
                    CmdletProviderAttribute attribute = (CmdletProviderAttribute)customAttributes[0];
                    return attribute.ProviderCapabilities;
                }
            }
            catch
            {
                // TODO: what if the provider has no capabilities?
            }
            return ProviderCapabilities.None;
        }

        // internals
        internal bool IsNameMatch(string providerName)
        {
            return string.Equals(FullName, providerName, StringComparison.CurrentCultureIgnoreCase);
        }

        //internal bool IsMatch(WildcardPattern namePattern, PSSnapinQualifiedName psSnapinQualifiedName)
        //{

        //}
    }
}
