using System;
using System.Management;
using System.Collections.Generic;
using NUnit.Framework;

namespace TestHost.FileSystemTests
{
    public class FileSystemTestBase
    {
        List<Path> _pathsCreated = new List<Path>();

        [TearDown]
        public void CleanupTempFiles()
        {
            try
            {
                foreach (var path in _pathsCreated)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.Delete(path);
                    }
                }
            }
            catch
            {
                // meh couldn't delete temp directory
            }
        }

        /// <summary>
        /// This helper is used to setup a specified directory/file structure starting from a random/temp root path.
        /// 
        /// Directories are specified by a path with a slash at the end.
        /// Files are specified by no path at the end.
        /// 
        /// </summary>
        /// <returns>The file system with structure.</returns>
        /// <param name="paths">Paths.</param>
        public string SetupFileSystemWithStructure(IEnumerable<string> paths)
        {
            Path rootPath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString().Replace("-", "");

            System.IO.Directory.CreateDirectory(rootPath);
            _pathsCreated.Add(rootPath);
            foreach (Path path in paths)
            {
                if (!path.StartsWithSlash())
                {
                    throw new Exception(string.Format("Path [{0}] must start with a slash \\ or /", path.ToString()));
                }

                var fullPath = rootPath.Combine(path);

                if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fullPath)))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
                }

                if (path.EndsWithSlash())
                {
                    System.IO.Directory.CreateDirectory(fullPath);
                }
                else
                {
                    System.IO.File.CreateText(fullPath);
                }
            }

            return rootPath;
        }
    }
}

