// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Path = System.Management.Path;

namespace TestHost.FileSystemTests
{
    public class FileSystemTestBase
    {
        List<Path> _pathsCreated = new List<Path>();
        private readonly List<Path> _filesCreated = new List<Path>();

        [TearDown]
        public void CleanupTempFiles()
        {
            try
            {
                foreach (var fileName in _filesCreated)
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }

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

        /// <summary>
        /// Set up the executable file with specified name. When invoked, it will echo the specified result string.
        /// </summary>
        /// <param name="fileName">Executable file name relative to test environment root.</param>
        /// <param name="result">Result string.</param>
        /// <returns>Test environment root path.</returns>
        protected string SetupExecutableWithResult(string fileName, string result)
        {
            var directory = System.IO.Path.GetDirectoryName(fileName);
            var subPath = string.IsNullOrEmpty(directory)
                ? Enumerable.Empty<string>()
                : new[]
                {
                    System.IO.Path.DirectorySeparatorChar + directory + System.IO.Path.DirectorySeparatorChar
                };
            var root = SetupFileSystemWithStructure(subPath);
            var filePath = System.IO.Path.Combine(root, fileName);
            File.WriteAllText(filePath, string.Format(@"
@echo off
echo {0}
", result));
            // TODO: make executable for Linux
            _filesCreated.Add(filePath);
            return root;
        }
    }
}

