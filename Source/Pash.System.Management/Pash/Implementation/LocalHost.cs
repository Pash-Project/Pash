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
using System.Globalization;
using System.Management.Automation;

namespace Pash.Implementation
{
    public class LocalHost : PSHost
    {
        private CultureInfo originalCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
        private CultureInfo originalUICultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return originalCultureInfo; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return originalUICultureInfo; }
        }

        public override void EnterNestedPrompt()
        {
            // $host.EnterNestedPrompt()
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        Guid instanceId = Guid.NewGuid();

        public override Guid InstanceId
        {
            get { return instanceId; }
        }

        public override string Name
        {
            get { return "LocalHost"; }
        }

        public override void NotifyBeginApplication()
        {
            return;  // Do nothing...
        }

        public override void NotifyEndApplication()
        {
            return;  // Do nothing...
        }

        public int ExitCode { get; private set; }
        public bool ShouldExit { get; private set; }

        public override void SetShouldExit(int exitCode)
        {
            ExitCode = exitCode;
            ShouldExit = true;
        }

        public override PSHostUserInterface UI
        {
            get { return new LocalHostUserInterface(); }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        // TODO: what options are needed?
        object Options = new object();

        public override PSObject PrivateData
        {
            get
            {
                return PSObject.AsPSObject(Options);
            }
        }
    }
}
