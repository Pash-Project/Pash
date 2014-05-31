// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;

namespace TestHost.ProviderTests
{
    [TestFixture]
    class EnvironmentProviderTests
    {
        const string PashTestEnvironmentVariableName = "PashEnvironmentProviderTest";

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable(PashTestEnvironmentVariableName, null);
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
    }
}
