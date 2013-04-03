// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;
using System.Management;

namespace TestHost
{
    [TestFixture]
    public class PathTests
    {

        [Test]
        [TestCase("", "", "Should do nothing with an empty string")]
        [TestCase("\\", "/", "Should change the single root path slash")]
        [TestCase("/", "/", "should not change the correct root path slash")]
        [TestCase("\\foo/bar\\baz", "/foo/bar/baz", "Should change all incorrect slashes")]
        public void NormalizePathForUnix(string input, string expected, string failureMessage)
        {
            var inputpath = SetUnixPaths(input);
            var expectedPath = SetUnixPaths(expected);

            inputpath.NormalizeSlashes()
                .ShouldEqual(expectedPath, failureMessage);
        }

        [Test]
        [TestCase("", "", "Should do nothing with an empty string")]
        [TestCase("/", "\\", "Should change the single root path slash")]
        [TestCase("\\", "\\", "should not change the correct root path slash")]
        [TestCase("/foo\\bar/baz", "\\foo\\bar\\baz", "Should change all incorrect slashes")]
        public void NormalizePathForWindows(string input, string expected, string failureMessage)
        {
            var inputPath = SetWindowsPaths(input);
            var expectedPath = SetWindowsPaths(expected);

            inputPath.NormalizeSlashes()
                .ShouldEqual(expectedPath, failureMessage);
        }

        [Test]
        [TestCase("C", "foo\\bar", "C:\\foo\\bar", "")]
        [TestCase("C", "foo/bar", "C:\\foo\\bar", "")]
        public void MakePathForWindows(string driveName, string input, string expected, string failureMessage)
        {
            var inputPath = SetWindowsPaths(input);
            var expectedPath = SetWindowsPaths(expected);

            inputPath.MakePath(driveName)
                .ShouldEqual(expectedPath, failureMessage);
        }


        [Test]
        [TestCase("/", "foo\\bar", "/foo/bar", "")]
        [TestCase("/", "foo/bar", "/foo/bar", "")]
        public void MakePathForUnix(string driveName, string input, string expected, string failureMessage)
        {
            var inputPath = SetUnixPaths(input);
            var expectedPath = SetUnixPaths(expected);

            inputPath.MakePath(driveName)
                .ShouldEqual(expectedPath, failureMessage);
        }



        [Test]
        [TestCase("/", "/foo", "/foo", "")]
        [TestCase("/foo", "/", "/", "")]
        [TestCase("/foo", "..", "/", "")]
        [TestCase("/foo", "../", "/", "")]
        [TestCase("/foo/bar", "../..", "/", "")]
        [TestCase("/foo/bar", "../../", "/", "")]
        [TestCase("/foo/bar", "../baz", "/foo/baz", "")]
        [TestCase("/foo/bar", "../../foo/baz/../bar", "/foo/bar", "")]
        [TestCase("Variable:", "/foo", "Variable:/foo", "")]
        public void GetFullPathForUnix(string currentLocation, string input, string expected, string failureMessage)
        {
            var inputPath = SetUnixPaths(input);
            var expectedPath = SetUnixPaths(expected);

            var isFileSystemProvider = true;
            if (currentLocation.StartsWith("Variable"))
                isFileSystemProvider = false;

            inputPath.GetFullPath("/", currentLocation, isFileSystemProvider)
                .PathShouldEqual(expectedPath, failureMessage);
        }

        private Path SetUnixPaths(Path path)
        {
            path.CorrectSlash = "/";
            path.WrongSlash = "\\";
            return path;
        }
        private Path SetWindowsPaths(Path path)
        {
            path.CorrectSlash = "\\";
            path.WrongSlash = "/";
            return path;
        }

    }
}

