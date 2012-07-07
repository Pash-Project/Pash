using System;

namespace System.Management.Automation.Host
{
    public struct Rectangle
    {
        public Rectangle(Coordinates upperLeft, Coordinates lowerRight) { throw new NotImplementedException(); }
        public Rectangle(int left, int top, int right, int bottom) { throw new NotImplementedException(); }

        public static bool operator !=(Rectangle first, Rectangle second) { throw new NotImplementedException(); }
        public static bool operator ==(Rectangle first, Rectangle second) { throw new NotImplementedException(); }

        public int Bottom { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }

        public override bool Equals(object obj) { throw new NotImplementedException(); }
        public override int GetHashCode() { throw new NotImplementedException(); }
        public override string ToString() { throw new NotImplementedException(); }
    }
}
