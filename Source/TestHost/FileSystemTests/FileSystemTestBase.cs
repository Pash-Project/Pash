// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            // We could still be in one of the created directories, so that it
            // cannot be deleted, at least on Windows. Thus, change the
            // current directory to somewhere entirely else.
            Environment.CurrentDirectory = "/";

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
                        System.IO.Directory.Delete(path, true);
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
                    System.IO.File.WriteAllText(fullPath, "");
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
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            var content = isWindows ?
                "@echo off\necho {0}\n" :
                "#!/bin/sh\necho {0}\n";
            content = string.Format(content, result);

            return SetupExecutable(fileName, content);
        }

        protected string SetupExecutable(string fileName, string content)
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

            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            File.WriteAllText(filePath, content);
            _filesCreated.Add(filePath);

            if (!isWindows)
            {
                Process.Start("chmod", string.Format("+x \"{0}\"", filePath)).WaitForExit();
            }

            return root;
        }
    }
}

