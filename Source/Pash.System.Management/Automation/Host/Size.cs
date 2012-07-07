using System;

namespace System.Management.Automation.Host
{
    public struct Size
    {
        public Size(int width, int height) { throw new NotImplementedException(); }

        public static bool operator !=(Size first, Size second) { throw new NotImplementedException(); }
        public static bool operator ==(Size first, Size second) { throw new NotImplementedException(); }

        public int Height { get; set; }
        public int Width { get; set; }

        public override bool Equals(object obj) { throw new NotImplementedException(); }
        public override int GetHashCode() { throw new NotImplementedException(); }
        public override string ToString() { throw new NotImplementedException(); }
    }
}
