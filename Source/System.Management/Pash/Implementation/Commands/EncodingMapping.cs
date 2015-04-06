// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Globalization;
using System.Text;

namespace System.Management.Pash.Implementation
{
    static internal class EncodingMapping
    {
        public static Encoding GetEncoding(string encoding)
        {
            if (string.IsNullOrEmpty(encoding))
            {
                return Encoding.UTF8;
            }

            switch (encoding.ToUpperInvariant())
            {
                case "UNICODE":
                    return Encoding.Unicode;
                case "UTF7":
                    return Encoding.UTF7;
                case "UTF8":
                    return Encoding.UTF8;
                case "UTF32":
                    return Encoding.UTF32;
                case "ASCII":
                    return Encoding.ASCII;
                case "BIGENDIANUNICODE":
                    return Encoding.BigEndianUnicode;
                case "DEFAULT":
                    return Encoding.Default;
                case "OEM":
                    return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                default:
                    throw new NotImplementedException(encoding);
            }
        }
    }
}
