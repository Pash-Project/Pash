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
using System.Collections;
using System.Collections.ObjectModel;

namespace Pash.Implementation
{
    internal class ObjectStream
    {
        // TODO: implement ObjectStream
        private ArrayList _objectsStream;

        internal ObjectStream()
        {
            _objectsStream = new ArrayList();
        }

        internal ObjectStream(IEnumerable input)
            : this()
        {
            foreach (object obj in input)
                _objectsStream.Add(obj);
        }

        internal void Write(object obj)
        {
            _objectsStream.Add(obj);
        }

        internal Collection<object> Read()
        {
            // TODO: (code duplication) does it makes sense to call the Read(count) with "everyting" as a value?

            Collection<object> c = new Collection<object>();

            // nothing to read
            if (_objectsStream.Count < 0)
                return c;

            foreach (object obj in _objectsStream)
            {
                c.Add(obj);
            }

            _objectsStream.Clear();

            return c;
        }

        internal Collection<object> Read(int count)
        {
            Collection<object> c = new Collection<object>();

            // nothing to read
            if (_objectsStream.Count < 0)
                return c;

            // TODO: what should be done if requested more than it's available?
            if (count > _objectsStream.Count)
                count = _objectsStream.Count;

            for (int i = 0; i < count; i++)
            {
                object obj = _objectsStream[i];
                c.Add(obj);
            }
            _objectsStream.RemoveRange(0, count);

            return c;
        }

        internal Collection<object> NonBlockingRead()
        {
            return Read();
        }

        internal Collection<object> NonBlockingRead(int maxRequested)
        {
            return Read(maxRequested);
        }

        internal int Count
        {
            get
            {
                return _objectsStream.Count;
            }
        }
    }
}
