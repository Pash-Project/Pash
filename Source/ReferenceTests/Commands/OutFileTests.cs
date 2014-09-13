using System;
using NUnit.Framework;
using System.IO;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class OutFileTests : ReferenceTestBase
    {

        [Test]
        public void OutFileCreatesFile()
        {
            var fname = Path.Combine(Path.GetTempPath(), "__outfileTest");
            AddCleanupFile(fname);
            ReferenceHost.Execute("'foobar' | Out-File " + fname);
            Assert.AreEqual(NewlineJoin("foobar"), NewlineJoin(ReadLinesFromFile(fname)));
        }

        [Test]
        public void OutFileCanAppendToFile()
        {
            var fname = Path.Combine(Path.GetTempPath(), "__outfileTest");
            AddCleanupFile(fname);
            ReferenceHost.Execute("'foobar' | Out-File " + fname);
            ReferenceHost.Execute("'baz' | Out-File " + fname + " -Append");
            Assert.AreEqual(NewlineJoin("foobar", "baz"), NewlineJoin(ReadLinesFromFile(fname)));
        }

        [Test]
        public void OutFileOverwritesByDefault()
        {
            var fname = Path.Combine(Path.GetTempPath(), "__outfileTest");
            AddCleanupFile(fname);
            ReferenceHost.Execute("'foobar' | Out-File " + fname);
            ReferenceHost.Execute("'baz' | Out-File " + fname);
            Assert.AreEqual("baz", String.Join("\n", ReadLinesFromFile(fname)));
        }

        [Test]
        public void OutFileWithNoClobberThrowsWhenOverwritingDefault()
        {
            var fname = Path.Combine(Path.GetTempPath(), "__outfileTest");
            AddCleanupFile(fname);
            // no clobber in first call to make sure it can create a file with arg
            ReferenceHost.Execute("'foobar' | Out-File " + fname + " -NoClobber");
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute("'baz' | Out-File " + fname + " -NoClobber");
            });
        }
    }
}

