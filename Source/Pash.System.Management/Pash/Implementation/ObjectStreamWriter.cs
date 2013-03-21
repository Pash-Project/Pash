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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Management.Automation.Runspaces;

namespace Pash.Implementation
{
    class ObjectStreamWriter : PipelineWriter
    {
        ObjectStream _stream;

        public ObjectStreamWriter(ObjectStream stream)
        {
            _stream = stream;
        }

        public override int Count
        {
            get { return _stream.Count; }
        }

        public override bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxCapacity
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Threading.WaitHandle WaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Write(object obj)
        {
            _stream.Write(obj);

            return 1;
        }

        public override int Write(object obj, bool enumerateCollection)
        {
            int numWritten = 0;
            if (!enumerateCollection)
            {
                Write(obj);
                numWritten = 1;
            }
            else
            {
                IEnumerator enumerator = GetEnumerator(obj);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        Write(enumerator.Current);
                        numWritten++;
                    }
                }
                else
                {
                    Write(obj);
                    numWritten = 1;
                }
            }

            return numWritten;
        }

        private IEnumerator GetEnumerator(object obj)
        {
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            IEnumerator enumerator = obj as IEnumerator;
            if (obj != null)
            {
                return enumerator;
            }

            return null;
        }

        public override string ToString()
        {
            return "Writer: Count=" + Count.ToString();
        }
    }
}
