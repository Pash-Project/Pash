// Copyright (C) Pash Contributors. All Rights Reserved. See https://github.com/Pash-Project/Pash/

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

using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;
using System.Collections.ObjectModel;
using System.Security.AccessControl;
using Pash.Implementation;

namespace System.Management.Automation.Provider
{
    public abstract class CmdletProvider : IResourceSupplier
    {
        internal ProviderRuntime ProviderRuntime { get; set; }

        // TODO: use CmdletProviderManagementIntrinsics

        protected CmdletProvider()
        {
        }

        public PSCredential Credential { get; private set; }
        protected object DynamicParameters { get; private set; }
        public Collection<string> Exclude { get; private set; }
        public string Filter { get; private set; }
        public SwitchParameter Force { get; private set; }
        public PSHost Host { get; private set; }
        public Collection<string> Include { get; private set; }
        public CommandInvocationIntrinsics InvokeCommand { get; private set; }
        public ProviderIntrinsics InvokeProvider { get; private set; }
        protected internal ProviderInfo ProviderInfo { get; private set; }
        protected PSDriveInfo PSDriveInfo { get; private set; }
        public bool Stopping { get; private set; }

        public SessionState SessionState
        {
            get
            {
                return new SessionState(this.ProviderRuntime.ExecutionContext.SessionState);
            }
        }

        public bool ShouldContinue(string query, string caption) { throw new NotImplementedException(); }
        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) { throw new NotImplementedException(); }
        public bool ShouldProcess(string target) { throw new NotImplementedException(); }
        public bool ShouldProcess(string target, string action) { throw new NotImplementedException(); }
        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) { throw new NotImplementedException(); }
        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason) { throw new NotImplementedException(); }

        protected virtual ProviderInfo Start(ProviderInfo providerInfo)
        {
            return providerInfo;
        }

        internal ProviderInfo Start(ProviderInfo providerInfo, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return Start(providerInfo);
        }

        protected virtual object StartDynamicParameters() { throw new NotImplementedException(); }
        protected virtual void Stop() { throw new NotImplementedException(); }
        protected internal virtual void StopProcessing() { throw new NotImplementedException(); }
        public void ThrowTerminatingError(ErrorRecord errorRecord) { throw new NotImplementedException(); }
        public void WriteDebug(string text) { throw new NotImplementedException(); }
        public void WriteError(ErrorRecord errorRecord) { throw new NotImplementedException(); }

        public void WriteItemObject(object item, string path, bool isContainer)
        {
            PSObject psObject = GetItemAsPSObject(item, path);
            PSNoteProperty member = new PSNoteProperty("PSIsContainer", isContainer);
            psObject.Properties.Add(member);
            ProviderRuntime.WriteObject(psObject);
        }

        public void WriteProgress(ProgressRecord progressRecord) { throw new NotImplementedException(); }
        public void WritePropertyObject(object propertyValue, string path) { throw new NotImplementedException(); }
        public void WriteSecurityDescriptorObject(ObjectSecurity securityDescriptor, string path) { throw new NotImplementedException(); }
        public void WriteVerbose(string text) { throw new NotImplementedException(); }
        public void WriteWarning(string text) { throw new NotImplementedException(); }

        #region IResourceSupplier Members

        public string GetResourceString(string baseName, string resourceId)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal void SetProviderInfo(ProviderInfo providerInfo)
        {
            ProviderInfo = providerInfo;
        }

        private PSObject GetItemAsPSObject(object item, string path)
        {
            if (item == null)
            {
                throw new Exception("Item can't be null");
            }

            PSObject psObject = PSObject.AsPSObject(item);

            PSObject obj3 = item as PSObject;
            if (obj3 != null)
            {
                psObject.TypeNames.Clear();
                foreach (string str in obj3.TypeNames)
                {
                    psObject.TypeNames.Add(str);
                }
            }

            // Add full path as a property
            string providerFullPath = PathIntrinsics.GetFullProviderPath(ProviderInfo, path);
            psObject.Properties.Add(new PSNoteProperty("PSPath", providerFullPath));

            NavigationCmdletProvider provider = this as NavigationCmdletProvider;
            if ((provider != null) && (path != null))
            {
                string parentPath = null;
                if (PSDriveInfo == null)
                {
                    parentPath = provider.GetParentPath(path, string.Empty, ProviderRuntime);
                }
                else
                {
                    parentPath = provider.GetParentPath(path, PSDriveInfo.Root, ProviderRuntime);
                }

                if (!string.IsNullOrEmpty(parentPath))
                {
                    parentPath = PathIntrinsics.GetFullProviderPath(ProviderInfo, parentPath);
                }
                else
                {
                    parentPath = string.Empty;
                }
                psObject.Properties.Add(new PSNoteProperty("PSParentPath", parentPath));

                string childName = provider.GetChildName(path, ProviderRuntime);
                psObject.Properties.Add(new PSNoteProperty("PSChildName", childName));
            }

            if (PSDriveInfo != null)
            {
                psObject.Properties.Add(new PSNoteProperty("PSDrive", PSDriveInfo));
            }

            psObject.Properties.Add(new PSNoteProperty("PSProvider", ProviderInfo));
            return psObject;
        }
    }
}
