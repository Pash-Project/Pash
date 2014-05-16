using System;
using System.Management.Automation.Host;

namespace TestHost
{
    public class TestHostRawUserInterface : PSHostRawUserInterface
    {
        private const int ARTIFICIAL_WINDOW_WIDTH = 120;
        private const int ARTIFICIAL_WINDOW_HEIGHT = 120;

        public override ConsoleColor BackgroundColor
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override Size BufferSize
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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
            get;
            set;
        }

        public override ConsoleColor ForegroundColor
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override bool KeyAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxWindowSize
        {
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override Size WindowSize
        {
            get { return new Size(ARTIFICIAL_WINDOW_WIDTH, ARTIFICIAL_WINDOW_HEIGHT); }
            set { throw new NotImplementedException(); }
        }

        public override string WindowTitle
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}

