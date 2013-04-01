using System;
using System.Management.Automation;
using Pash.Implementation;

namespace System.Management
{

    /// <summary>
    /// Imutable class that acts like a string, but provides many options around manipulating a powershell 'path'.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Path:{_rawPath}")]
    public class Path
    {
        private readonly string _rawPath;
        static readonly string _predeterminedCorrectSlash;
        static readonly string _predeterminedWrongSlash;

        static Path()
        {
            if (System.IO.Path.DirectorySeparatorChar.Equals('/'))
            {
                _predeterminedCorrectSlash = "/";
                _predeterminedWrongSlash = "\\";
            }
            else
            {
                _predeterminedCorrectSlash = "\\";
                _predeterminedWrongSlash = "/";
            }
        }

        public Path(string rawPath)
            : this(_predeterminedCorrectSlash, _predeterminedWrongSlash, rawPath)
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

        public override int GetHashCode()
        {
            return _rawPath.GetHashCode();
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
                }
                else
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
            }
            else
            {
                builder.Append(child);
            }

            return builder.ToString();

        }

        public Path GetFullPath(string driveName, string currentLocation, bool isFileSystemProvider)
        {
            if (this.IsRootPath())
            {
                return this.MakePath(driveName);
            }

            if (isFileSystemProvider)
            {
                Path combinedPath;

                if (this.HasDrive())
                {
                    combinedPath = this;
                }
                else
                {
                    combinedPath = ((Path)currentLocation).Combine(this);
                }

                return System.IO.Path.GetFullPath(combinedPath);
            }


            // TODO: this won't work with non-file-system paths that use "../" navigation or other "." navigation.
            return ((Path)currentLocation).Combine(this);
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

            if (this.StartsWithSlash())
            {
                return CorrectSlash;
            }

            int iDelimiter = _rawPath.IndexOf(':');

            if (iDelimiter == -1)
                return null;

            return _rawPath.Substring(0, iDelimiter);
        }

        public bool TryGetDriveName(out string driveName)
        {
            driveName = GetDrive();
            if (string.IsNullOrEmpty(driveName))
            {
                return false;
            }
            return true;
        }

        public bool HasDrive()
        {
            var drive = GetDrive();

            if (string.IsNullOrEmpty(drive))
            {
                return false;
            }
            return true;
        }

        public Path RemoveDrive()
        {
            string drive;
            if (this.TryGetDriveName(out drive))
            {
                var newPath = _rawPath.Substring(drive.Length);
                if (newPath.StartsWith(":"))
                    return newPath.Substring(1);
                return newPath;
            }

            return this;
        }

        public Path MakePath(string driveName)
        {
            Path fullPath;
            if (driveName == CorrectSlash)
            {
                string preSlash = this.StartsWithSlash() ? string.Empty : CorrectSlash;

                fullPath = new Path(CorrectSlash, WrongSlash, string.Format("{0}{1}", preSlash, this));
            }
            else
            {
                if (this.HasDrive())
                {
                    return this;
                }

                //TODO: should this take a "current path" parameter? EX: {drive}:{currentPath??}/{this}
                string preSlash = this.StartsWithSlash() ? string.Empty : CorrectSlash;

                fullPath = new Path(CorrectSlash, WrongSlash, string.Format("{0}:{1}{2}", driveName, preSlash, this));
            }

            return fullPath.NormalizeSlashes();
        }

        public override string ToString()
        {
            return _rawPath;
        }

        public int Length { get { return _rawPath.Length; } }
    }

    public static partial class _
    {
        public static Path AsPath(this string value)
        {
            return (Path)value;
        }

        public static string NormalizeSlashes(this string value)
        {
            return ((Path)value).NormalizeSlashes();
        }
    }

}

