using System;
using NUnit.Framework;
using System.Management;

namespace TestHost
{
    [TestFixture]
    public class PathNavigationTests
    {
        [Test]
        // Windows tests
        [TestCase("\\", "C:\\", "", "C:\\", "empty command should do nothing")]
        [TestCase("\\", "C:\\", "/", "C:\\", "slash should return to root drive")]
        [TestCase("\\", "C:\\", ".", "C:\\", "single dot is ignored")]
        [TestCase("\\", "C:\\foo\\bar",".\\.\\.","C:\\foo\\bar", "single dot is ignored")]
        [TestCase("\\", "C:\\","C:\\","C:\\", "Root should return root")]
        [TestCase("\\", "C:/","","C:\\", "empty command with wrong input slashes should fix slashes")]
        [TestCase("\\", "C:","","C:\\", "empty command should do nothing")]
        [TestCase("\\", "C:\\foo\\","..","C:\\", "nav up one dir")]
        [TestCase("\\", "C:\\","C:\\foo","C:\\foo", "change dir given full path")]
        [TestCase("\\", "C:\\","C:\\foo\\","C:\\foo", "change dir given full path and extra slash")]
        [TestCase("\\", "C:\\foo\\","..","C:\\", "nav up one dir")]
        [TestCase("\\", "C:\\foo\\bar","..","C:\\foo", "nav up one dir")]
        [TestCase("\\", "C:\\foo\\bar","..\\..","C:\\", "nav up two dirs")]
        [TestCase("\\", "C:\\foo\\bar\\baz","..\\..\\..","C:\\", "nav up three dirs")]
        [TestCase("\\", "C:\\foo\\bar\\baz","boo","C:\\foo\\bar\\baz\\boo", "down one dir")]

        // Unix tests
        [TestCase("/", "/","","/", "empty command should do nothing")]
        [TestCase("/", "/",".","/", "single dot is ignored")]
        [TestCase("/", "/","./.\\.","/", "single dot is ignored")]
        [TestCase("/", "/","/","/", "Root should return root")]
        [TestCase("/", "/","\\","/", "Root should return root (even if wrong slash)")]
        [TestCase("/", "/foo","..","/", "nav up one dir")]
        [TestCase("/", "/","/foo","/foo", "change dir given full path")]
        [TestCase("/", "/foo/","..","/", "nav up one dir")]
        [TestCase("/", "/foo/bar","..","/foo", "nav up one dir")]
        [TestCase("/", "\\foo\\bar","..","/foo", "nav up one dir (even if current location has wrong slashes)")]
        [TestCase("/", "/foo/bar","../..","/", "nav up two dirs")]
        [TestCase("/", "/foo/bar/baz","../../..","/", "nav up three dirs")]
        [TestCase("/", "/foo/bar/baz","boo","/foo/bar/baz/boo", "down one dir")]
        public void ShouldApplyNavigation(string normalSlash, string currentLocation, string changeCommand, string expectedFullPath, string errorMessage)
        {
            var currLocation = new Path(normalSlash, normalSlash == "\\" ? "/" : "\\", currentLocation);
            var changePath = new Path(normalSlash, normalSlash == "\\" ? "/" : "\\", changeCommand);

            var result = PathNavigation.CalculateFullPath(currLocation, changePath);
            
            result.ShouldEqual(expectedFullPath, errorMessage);
            
        }
    }

}

