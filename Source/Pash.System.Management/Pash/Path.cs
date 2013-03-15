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

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                return _rawPath.Equals(obj);
            }

            var objPath = obj as Path;
            if (objPath != null)
            {
                return _rawPath.Equals(objPath._rawPath);
            }
            
            return base.Equals(obj);
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

        public Path GetParentPath(Path root)
        {
            var path = this;

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

        public Path Combine(Path child)
        {
            var parent = this;

            if (string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
            {
                return child;
            }

            if (string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
            {
                return child.NormalizeSlashes();
            }
            
            parent = parent.NormalizeSlashes();
            
            if (!string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
            {
                if (parent.EndsWithSlash())
                {
                    return parent;
                } else
                {
                    return parent.AppendSlashAtEnd();
                }
            }
            
            child = child.NormalizeSlashes();
            var builder = new System.Text.StringBuilder(parent);
            
            if (!parent.EndsWithSlash())
                builder.Append(CorrectSlash);
            
            // Make sure we do not add two \
            if (child.StartsWithSlash())
            {
                builder.Append(child, 1, child.Length - 1);
            } else
            {
                builder.Append(child);
            }
            
            return builder.ToString();

        }

        public bool StartsWithSlash()
        {
            if (this.NormalizeSlashes()._rawPath.StartsWith(CorrectSlash))
            {
                return true;
            }
            return false;
        }
        public bool EndsWithSlash()
        {
            if (this.NormalizeSlashes()._rawPath.EndsWith(CorrectSlash))
            {
                return true;
            }
            return false;
        }
//        public bool EndsWith(string value)
//        {
//            return _rawPath.EndsWith(value);
//        }

        public Path AppendSlashAtEnd()
        {
            if (this.EndsWithSlash())
            {
                return this;
            }

            return this + CorrectSlash;
        }

        public bool StartsWith(string value)
        {
            return _rawPath.StartsWith(value);
        }

        public bool IsRootPath()
        {
            if (_rawPath == CorrectSlash)
            {
                return true;
            }
            return false;
        }

        public string GetDrive()
        {
            if (IsRootPath())
            {
                return this;
            }
            
            int iDelimiter = _rawPath.IndexOf(':');
            
            if (iDelimiter == -1)
                return null;
            
            return _rawPath.Substring(0, iDelimiter);
        }
        public int Length { get { return _rawPath.Length; } }
    }

    public static partial class _
    {
        public static Path AsPath(this string value)
        {
            return (Path)value;
        }
    }

}

