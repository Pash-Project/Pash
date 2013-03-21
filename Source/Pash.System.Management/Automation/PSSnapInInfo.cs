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

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public class PSSnapInInfo
    {
        public string ApplicationBase { get; private set; }
        public string AssemblyName { get; private set; }
        public string Description { get; private set; }
        public Collection<string> Formats { get; private set; }
        public bool IsDefault { get; private set; }
        public bool LogPipelineExecutionDetails { get; set; }
        public string ModuleName { get; private set; }
        public string Name { get; private set; }
        public Version PSVersion { get; private set; }
        public Collection<string> Types { get; private set; }
        public string Vendor { get; private set; }
        public Version Version { get; private set; }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        // internals
        //internal PSSnapInInfo Clone();
        //internal void LoadIndirectResources();
        internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string vendor)
        {
            Name = name;
            IsDefault = isDefault;
            ApplicationBase = applicationBase;
            AssemblyName = assemblyName;
            ModuleName = moduleName;
            PSVersion = psVersion;
            Version = version;
            Types = types;
            Formats = formats;
            Description = description;
            Vendor = vendor;
        }

        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string descriptionIndirect, string vendor, string vendorFallback, string vendorIndirect, string customPSSnapInType);
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string vendor, string vendorFallback, string customPSSnapInType);
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string descriptionFallback, string vendorFallback, string customPSSnapInType);
        //internal static void VerifyPSSnapInFormatThrowIfError(string mshSnapInId);
        //internal string AbsoluteModulePath { get; }
        //internal string CustomPSSnapInType { get; }
    }
}
