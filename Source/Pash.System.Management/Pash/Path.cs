using System;

namespace System.Management
{

    //TODO: decide if this class should be imutable(sp?) (currently not) currently mutates.
    public class Path
    {
        private string _rawPath;
        static string _predeterminedCorrectSlash;
        static string _predeterminedWrongSlash;

        static Path()
        {
            if (System.IO.Path.DirectorySeparatorChar.Equals('/'))
            {
                _predeterminedCorrectSlash = "/";
                _predeterminedWrongSlash = "\\";
            } else
            {
                _predeterminedCorrectSlash = "\\";
                _predeterminedWrongSlash = "/";
            }
        }
               
        public Path(string rawPath) : this(_predeterminedCorrectSlash, _predeterminedWrongSlash, rawPath)
        {
        }

        public Path(string correctSlash, string wrongSlash, string rawPath)
        {
            _rawPath = rawPath ?? string.Empty;
            CorrectSlash = correctSlash;
            WrongSlash = wrongSlash;
        }

        public string CorrectSlash { get; set; }
        public string WrongSlash { get; set; }

        public static implicit operator string(Path path)
        {
            return path._rawPath;
        }
        
        public static implicit operator Path(string path)
        {
            return new Path(path);
        }
        



        public Path NormalizeSlashes()
        {
            _rawPath = _rawPath.Replace(WrongSlash, CorrectSlash);
            return this;
        }

        public Path TrimEnd(params char[] trimChars)
        {
            _rawPath = _rawPath.TrimEnd(trimChars);
            return this;
        }

        public int LastIndexOf(char value)
        {
            return _rawPath.LastIndexOf(value);
        }

        public int IndexOf(char value)
        {
            return _rawPath.IndexOf(value);
        }

        public string Substring(int startIndex)
        {
            return _rawPath.Substring(startIndex);
        }

        public string Substring(int startIndex, int length)
        {
            return _rawPath.Substring(startIndex, length);
        }
        public bool EndsWith(string value)
        {
            return _rawPath.EndsWith(value);
        }
        public bool StartsWith(string value)
        {
            return _rawPath.StartsWith(value);
        }

        public int Length { get { return _rawPath.Length; } }
    }
}

