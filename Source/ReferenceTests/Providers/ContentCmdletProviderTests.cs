using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{
    [TestFixture]
    public class ContentCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        [SetUp]
        public void Init()
        {
            TestContentCmdletProvider.Messages.Clear();
        }

        void AssertMessagesAreEqual(params string[] expected)
        {
            CollectionAssert.AreEqual(expected, TestContentCmdletProvider.Messages);
        }

        [Test]
        public void SetContent()
        {
            ReferenceHost.Execute("Set-Content -value 'abc' -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "ClearContent foo",
                "GetContentWriter foo",
                "ContentWriter.Write(abc)",
                "ContentWriter.Close()");
        }

        [Test]
        public void SetContentWithTwoItems()
        {
            ReferenceHost.Execute("Set-Content -value '1','2' -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "ClearContent foo",
                "GetContentWriter foo",
                "ContentWriter.Write(1,2)",
                "ContentWriter.Close()");
        }

        [Test]
        public void SetContentFromPipeline()
        {
            ReferenceHost.Execute("'a','b','c' | Set-Content -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "ClearContent foo",
                "GetContentWriter foo",
                "ContentWriter.Write(a)",
                "ContentWriter.Write(b)",
                "ContentWriter.Write(c)",
                "ContentWriter.Close()");
        }

        [Test]
        public void GetContent()
        {
            ReferenceHost.Execute("Get-Content -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "GetContentReader foo",
                "ContentReader.Read(1)",
                "ContentReader.Close()");
        }

        [Test]
        public void ClearContent()
        {
            ReferenceHost.Execute("Clear-Content -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual("ClearContent foo");
        }

        [Test]
        public void AddContent()
        {
            ReferenceHost.Execute("Add-Content -value 'abc' -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "GetContentWriter foo",
                "ContentWriter.Seek(0, End)",
                "ContentWriter.Write(abc)",
                "ContentWriter.Close()");
        }

        [Test]
        public void AddContentWithTwoItems()
        {
            ReferenceHost.Execute("Add-Content -value '1','2' -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "GetContentWriter foo",
                "ContentWriter.Seek(0, End)",
                "ContentWriter.Write(1,2)",
                "ContentWriter.Close()");
        }

        [Test]
        public void AddContentFromPipeline()
        {
            ReferenceHost.Execute("'a','b','c' | Add-Content -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "GetContentWriter foo",
                "ContentWriter.Seek(0, End)",
                "ContentWriter.Write(a)",
                "ContentWriter.Write(b)",
                "ContentWriter.Write(c)",
                "ContentWriter.Close()");
        }
    }
}
