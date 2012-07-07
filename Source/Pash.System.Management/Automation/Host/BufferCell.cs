using System;

namespace System.Management.Automation.Host
{
    public struct BufferCell
    {
        public BufferCell(char character, ConsoleColor foreground, ConsoleColor background, BufferCellType bufferCellType) { throw new NotImplementedException(); }

        public static bool operator !=(BufferCell first, BufferCell second) { throw new NotImplementedException(); }
        public static bool operator ==(BufferCell first, BufferCell second) { throw new NotImplementedException(); }

        public ConsoleColor BackgroundColor { get; set; }
        public BufferCellType BufferCellType { get; set; }
        public char Character { get; set; }
        public ConsoleColor ForegroundColor { get; set; }

        public override bool Equals(object obj) { throw new NotImplementedException(); }
        public override int GetHashCode() { throw new NotImplementedException(); }
        public override string ToString() { throw new NotImplementedException(); }
    }
}
