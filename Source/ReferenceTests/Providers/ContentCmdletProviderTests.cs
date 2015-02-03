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
            string result = ReferenceHost.Execute("Set-Content -value 'abc' -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "ClearContent foo",
                "GetContentWriter foo",
                "ContentWriter.Write(abc)",
                "ContentWriter.Close()");
        }

        [Test]
        public void SetContentWithTwoItems()
        {
            string result = ReferenceHost.Execute("Set-Content -value '1','2' -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "ClearContent foo",
                "GetContentWriter foo",
                "ContentWriter.Write(1,2)",
                "ContentWriter.Close()");
        }

        [Test]
        public void GetContent()
        {
            string result = ReferenceHost.Execute("Get-Content -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual(
                "GetContentReader foo",
                "ContentReader.Read(1)",
                "ContentReader.Close()");
        }

        [Test]
        public void ClearContent()
        {
            string result = ReferenceHost.Execute("Clear-Content -path TestContentCmdletProvider::foo");

            AssertMessagesAreEqual("ClearContent foo");
        }
    }
}
