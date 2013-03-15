using System;

namespace System.Management
{

    /// <summary>
    /// Imutable class that acts like a string, but provides many options around manipulating a powershell 'path'.
    /// </summary>
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
            return new Path(_rawPath.Replace(WrongSlash, CorrectSlash));
        }

        public Path TrimEnd(params char[] trimChars)
        {
            return new Path(_rawPath.TrimEnd(trimChars));
        }

        public Path TrimEndSlash()
        {
            return new Path(_rawPath.TrimEnd(char.Parse(CorrectSlash)));
        }

        public int LastIndexOf(char value)
        {
            return _rawPath.LastIndexOf(value);
        }

        public int IndexOf(char value)
        {
            return _rawPath.IndexOf(value);
        }

        public Path GetChildNameOrSelfIfNoChild()
        {
            Path path = this.NormalizeSlashes()
                .TrimEndSlash();
            
            int iLastSlash = path.LastIndexOf('\\');
            if (iLastSlash == -1)
            {
                return path;
            }
            
            return path._rawPath.Substring(iLastSlash + 1);
        }

//        public string Substring(int startIndex, int length)
//        {
//            return _rawPath.Substring(startIndex, length);
//        }
        public Path GetParentPath(Path root)
        {
            var path = this;

            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("Path can't be empty");
            
            path = path.NormalizeSlashes();
            path = path.TrimEndSlash();
            
            if (root != null)
            {
                if (string.Equals(path, root, StringComparison.CurrentCultureIgnoreCase))
                {
                    return string.Empty;
                }
            }
            
            int iLastSlash = path._rawPath.LastIndexOf(CorrectSlash);
            
            if (iLastSlash > 0)
                return path._rawPath.Substring(0, iLastSlash);
            
            if (iLastSlash == 1)
                return CorrectSlash;
            
            return string.Empty;

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

