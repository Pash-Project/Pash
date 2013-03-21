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

namespace System.Management.Automation.Host
{
    public abstract class PSHostRawUserInterface
    {
        protected PSHostRawUserInterface() { throw new NotImplementedException(); }

        public abstract ConsoleColor BackgroundColor { get; set; }
        public abstract Size BufferSize { get; set; }
        public abstract Coordinates CursorPosition { get; set; }
        public abstract int CursorSize { get; set; }
        public abstract ConsoleColor ForegroundColor { get; set; }
        public abstract bool KeyAvailable { get; }
        public abstract Size MaxPhysicalWindowSize { get; }
        public abstract Size MaxWindowSize { get; }
        public abstract Coordinates WindowPosition { get; set; }
        public abstract Size WindowSize { get; set; }
        public abstract string WindowTitle { get; set; }

        public abstract void FlushInputBuffer();
        public abstract BufferCell[,] GetBufferContents(Rectangle rectangle);
        public virtual int LengthInBufferCells(char source) { throw new NotImplementedException(); }
        public virtual int LengthInBufferCells(string source) { throw new NotImplementedException(); }
        public BufferCell[,] NewBufferCellArray(Size size, BufferCell contents) { throw new NotImplementedException(); }
        public BufferCell[,] NewBufferCellArray(int width, int height, BufferCell contents) { throw new NotImplementedException(); }
        public BufferCell[,] NewBufferCellArray(string[] contents, ConsoleColor foregroundColor, ConsoleColor backgroundColor) { throw new NotImplementedException(); }
        public KeyInfo ReadKey() { throw new NotImplementedException(); }
        public abstract KeyInfo ReadKey(ReadKeyOptions options);
        public abstract void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill);
        public abstract void SetBufferContents(Coordinates origin, BufferCell[,] contents);
        public abstract void SetBufferContents(Rectangle rectangle, BufferCell fill);
    }
}
