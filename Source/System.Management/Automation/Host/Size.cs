// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Host
{
    public struct Size
    {
        public Size(int width, int height)
            : this()
        {
            Height = height;
            Width = width;
        }

        public static bool operator !=(Size first, Size second)
        {
            return !(first == second);
        }

        public static bool operator ==(Size first, Size second)
        {
            return (first.Height == second.Height) && (first.Width == second.Width);
        }

        public int Height { get; set; }
        public int Width { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Size)
            {
                return this == (Size)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Width ^ Height;
        }

        public override string ToString()
        {
            return String.Format("{0},{1}", Width, Height);
        }
    }
}
