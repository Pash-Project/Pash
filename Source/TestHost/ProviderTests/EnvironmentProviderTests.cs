// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using NUnit.Framework;

namespace TestHost.ProviderTests
{
    [TestFixture]
    class EnvironmentProviderTests
    {
        const string PashTestEnvironmentVariableName = "PashEnvironmentProviderTest";
        const string PashTestEnvironmentVariableName2 = "PashEnvironmentProviderTest2";

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, null);
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName2, null);
        }

        [Test]
        public void EnvironmentVariableValueCanBeRetrieved()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, "TestValue");
            string command = string.Format("$env:{0}", PashTestEnvironmentVariableName);
            string result = TestHost.Execute(true, command);

            Assert.AreEqual("TestValue" + Environment.NewLine, result);
        }

        [Test]
        public void EnvironmentVariableValueCanBeSet()
        {
            string command = string.Format("$env:{0} = 'AnotherValue'", PashTestEnvironmentVariableName);
            TestHost.Execute(true, command);
            string result = Environment.GetEnvironmentVariable(PashTestEnvironmentVariableName);

            Assert.AreEqual("AnotherValue", result);
        }

        [Test]
        public void GetChildItemsOfEnvironmentDriveReturnsAllEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, "TestValue");
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName2, "TestValue2");
            string result = TestHost.Execute(true, "get-childitem env: | foreach-object { $_.value }");

            StringAssert.Contains("TestValue" + Environment.NewLine, result);
            StringAssert.Contains("TestValue2" + Environment.NewLine, result);
        }

        [Test]
        public void GetChildItemsOfEnvironmentDriveFollowedBySlashReturnsAllEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, "TestValue");
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName2, "TestValue2");
            string command = string.Format("get-childitem env:{0} | foreach-object {{ $_.value }}", Path.DirectorySeparatorChar);
            string result = TestHost.Execute(true, command);

            StringAssert.Contains("TestValue" + Environment.NewLine, result);
            StringAssert.Contains("TestValue2" + Environment.NewLine, result);
        }

        [Test]
        public void GetSingleEnviromentVariableFromEnvironmentDrive()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, "TestValue");
            string command = string.Format("get-childitem env:{0} | foreach-object {{ $_.value }}", PashTestEnvironmentVariableName);
            string result = TestHost.Execute(true, command);

            StringAssert.Contains("TestValue" + Environment.NewLine, result);
        }

        [Test]
        public void GetSingleEnviromentVariableFromEnvironmentDriveWithSlashAfterDrive()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, "TestValue");
            string command = string.Format("get-childitem env:{0}{1} | foreach-object {{ $_.value }}", Path.DirectorySeparatorChar, PashTestEnvironmentVariableName);
            string result = TestHost.Execute(true, command);

            StringAssert.Contains("TestValue" + Environment.NewLine, result);
        }
    }
}
