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
using System.Text;
using System.Management.Automation.Host;

namespace Pash.Implementation
{
    class LocalHostRawUserInterface : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }

        public override Size BufferSize
        {
            get { return new Size(Console.BufferWidth, Console.BufferHeight); }
            set { Console.SetBufferSize(value.Width, value.Height); }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int CursorSize
        {
            // TODO: what is the CursorSize?
            /*get { return Console.CursorSize; }
            set { Console.CursorSize = value; }*/
            get;
            set;
        }

        public override ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public override bool KeyAvailable
        {
            get { return Console.KeyAvailable; }
        }

        public override Size MaxPhysicalWindowSize
        {
            // TODO: what is the MaxPhysicalWindowSize?
            /*get { return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight); }*/
            get { return new Size(80, 60); }
        }

        public override Size MaxWindowSize
        {
            // TODO: what is the MaxWindowSize?
            /*get { return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight); }*/
            get { return new Size(80, 60); }
        }

        public override void FlushInputBuffer()
        {
            ;  //Do nothing...
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override Coordinates WindowPosition
        {
            get { return new Coordinates(Console.WindowLeft, Console.WindowTop); }
            set { Console.SetWindowPosition(value.X, value.Y); }
        }

        public override Size WindowSize
        {
            get { return new Size(Console.WindowWidth, Console.WindowHeight); }
            set { Console.SetWindowSize(value.Width, value.Height); }
        }

        public override string WindowTitle
        {
            get { return Console.Title; }
            set { Console.Title = value; }
        }
    }
}
