using System;

namespace System.Management.Automation.Host
{
    public struct Coordinates
    {
        public Coordinates(int x, int y) { throw new NotImplementedException(); }

        public static bool operator !=(Coordinates first, Coordinates second) { throw new NotImplementedException(); }
        public static bool operator ==(Coordinates first, Coordinates second) { throw new NotImplementedException(); }

        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj) { throw new NotImplementedException(); }
        public override int GetHashCode() { throw new NotImplementedException(); }
        public override string ToString() { throw new NotImplementedException(); }
    }
}
