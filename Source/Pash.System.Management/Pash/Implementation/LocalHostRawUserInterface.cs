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
