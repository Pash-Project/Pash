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
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    public sealed class ProviderIntrinsics
    {
        public ChildItemCmdletProviderIntrinsics ChildItem { get; private set; }

        public ContentCmdletProviderIntrinsics Content { get; private set; }

        public ItemCmdletProviderIntrinsics Item { get; private set; }

        public PropertyCmdletProviderIntrinsics Property { get; private set; }

        public SecurityDescriptorCmdletProviderIntrinsics SecurityDescriptor { get; private set; }

        // internals
        //internal ProviderIntrinsics(SessionStateInternal sessionState);

        private InternalCommand _cmdlet;

        internal ProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw new NullReferenceException("Cmdlet can't be null.");
            }

            _cmdlet = cmdlet;
            ChildItem = new ChildItemCmdletProviderIntrinsics(cmdlet);
            Content = new ContentCmdletProviderIntrinsics(cmdlet);
            Item = new ItemCmdletProviderIntrinsics(cmdlet);
            Property = new PropertyCmdletProviderIntrinsics(cmdlet);
            SecurityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(cmdlet);
        }
    }
}
