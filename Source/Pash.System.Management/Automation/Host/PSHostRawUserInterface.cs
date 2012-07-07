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
