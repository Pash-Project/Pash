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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
    public class PSDriveInfo : IComparable
    {
        // TODO: drive can be hidden
        public string Name { get; private set; }
        public ProviderInfo Provider { get; private set; }
        public string Root { get; internal set; }
        public string Description { get; set; }
        public PSCredential Credential { get; private set; }
        public string CurrentLocation { get; set; }
        internal bool RemovableDrive { get; set; }

        protected PSDriveInfo(PSDriveInfo driveInfo)
            : this(driveInfo.Name, driveInfo.Provider, driveInfo.Root, driveInfo.Description, driveInfo.Credential)
        {
            CurrentLocation = driveInfo.CurrentLocation;
        }

        public PSDriveInfo(string name, ProviderInfo provider, string root = null, string description = null, PSCredential credential = null)
        {
            Name = name;
            Provider = provider;
            Root = root ?? string.Empty;
            Description = description ?? string.Empty;
            Credential = credential;
            CurrentLocation = string.Empty;
        }

        public static bool operator !=(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            return !(drive1 == drive2);
        }

        public static bool operator <(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object objDrive1 = drive1;
            object objDrive2 = drive2;
            if ((objDrive1 == null) != (objDrive2 == null))
            {
                return false;
            }
            if (objDrive1 != null)
            {
                return drive1.CompareTo(drive2) < 0;
            }
            return true;
        }

        public static bool operator ==(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object objDrive1 = drive1;
            object objDrive2 = drive2;
            if ((objDrive1 == null) != (objDrive2 == null))
            {
                return false;
            }
            if (objDrive1 != null)
            {
                return drive1.Equals(drive2);
            }
            return true;
        }

        public static bool operator >(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object objDrive1 = drive1;
            object objDrive2 = drive2;
            if ((objDrive1 == null) != (objDrive2 == null))
            {
                return false;
            }
            if (objDrive1 != null)
            {
                return drive1.CompareTo(drive2) > 0;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is PSDriveInfo)
                return CompareTo(obj as PSDriveInfo) == 0;

            throw new InvalidOperationException("Can compare only to PSDriveInfo");
        }

        public bool Equals(PSDriveInfo drive)
        {
            return (CompareTo(drive) == 0);
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Provider.FullName, Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #region IComparable Members

        public int CompareTo(PSDriveInfo drive)
        {
            return string.Compare(Name, drive.Name, true, CultureInfo.CurrentUICulture);
        }

        public int CompareTo(object obj)
        {
            if (obj is PSDriveInfo)
                return CompareTo(obj as PSDriveInfo);

            throw new InvalidOperationException("Can compare only to PSDriveInfo");
        }

        #endregion
    }
}
