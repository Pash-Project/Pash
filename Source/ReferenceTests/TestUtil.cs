using System;
using System.Security;
using System.Runtime.InteropServices;

namespace ReferenceTests
{
    public static class TestUtil
    {
        public class PashFunction
        {
            public readonly string Name;
            public readonly string Body;

            public PashFunction(string name, string body)
            {
                Name = name;
                Body = body;
            }

            public string Call(params string[] args)
            {
                return "& { " + Body + "; " + Name + " " + String.Join(" ", args) + " } ";
            }
        }

        public static readonly PashFunction PashDecodeSecureString = new PashFunction(
            "_testSecureStr2Str",
            @"
            function _testSecureStr2Str($secureStr) {
              $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($secureStr)
              $result = [System.Runtime.InteropServices.Marshal]::PtrToStringUni($ptr)
              [System.Runtime.InteropServices.Marshal]::ZeroFreeCoTaskMemUnicode($ptr)
              return $result;
            }
            "
        );

        public static string DecodeSecureString(SecureString secureStr)
        { 
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureStr);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}

