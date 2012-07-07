using System;

namespace System.Management.Automation.Host
{
    public struct KeyInfo
    {
        public KeyInfo(int virtualKeyCode, char ch, ControlKeyStates controlKeyState, bool keyDown) { throw new NotImplementedException(); }

        public static bool operator !=(KeyInfo first, KeyInfo second) { throw new NotImplementedException(); }
        public static bool operator ==(KeyInfo first, KeyInfo second) { throw new NotImplementedException(); }

        public char Character { get; set; }
        public ControlKeyStates ControlKeyState { get; set; }
        public bool KeyDown { get; set; }
        public int VirtualKeyCode { get; set; }

        public override bool Equals(object obj) { throw new NotImplementedException(); }
        public override int GetHashCode() { throw new NotImplementedException(); }
        public override string ToString() { throw new NotImplementedException(); }
    }
}
