// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class NewObjectCommandTests
    {
        [Test]
        public void CanCreateTypeWithNoParameters()
        {
            Assert.AreEqual("0.0" + Environment.NewLine, TestHost.Execute("(New-Object System.Version).ToString()"));
        }

        [Test]
        public void SystemPrefixIsOptional()
        {
            Assert.AreEqual("0.0" + Environment.NewLine, TestHost.Execute("(New-Object Version).ToString()"));
        }

        [Test]
        public void CaseInsenstive()
        {
            Assert.AreEqual("0.0" + Environment.NewLine, TestHost.Execute("(New-Object version).ToString()"));
        }

        // this one is not in mscorlib
        [Test]
        public void WebClient()
        {
            Assert.AreEqual(typeof(System.Net.WebClient).Name + Environment.NewLine,
                            TestHost.Execute("(New-Object Net.WebClient).GetType().Name"));
        }

        [Test]
        public void ValueTypes()
        {
            Assert.AreEqual("False" + Environment.NewLine, TestHost.Execute("New-Object Boolean"));
        }

        [Test]
        public void Parameter()
        {
            Assert.AreEqual("3.4.5.6" + Environment.NewLine,
                            TestHost.Execute("(New-Object version \"3.4.5.6\").ToString()"));
        }

        [Test]
        public void BuiltinType()
        {
            Assert.AreEqual("False" + Environment.NewLine, TestHost.Execute("New-Object bool"));
        }
    }
}
