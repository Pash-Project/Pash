// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.API
{
    [TestFixture]
    public class ErrorRecordTests
    {
        [Test]
        public void TargetObjectIsString()
        {
            var ex = new ApplicationException("Exception error message");
            var error = new ErrorRecord(ex, "errorId", ErrorCategory.AuthenticationError, "targetObject");

            Assert.AreEqual("Exception error message", error.ToString());
            Assert.AreEqual("errorId", error.FullyQualifiedErrorId);
            Assert.IsNull(error.ErrorDetails);
            Assert.AreEqual(ex, error.Exception);
            Assert.AreEqual("targetObject", error.TargetObject);
            Assert.AreEqual("", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.AuthenticationError, error.CategoryInfo.Category);
            Assert.AreEqual("ApplicationException", error.CategoryInfo.Reason);
            Assert.AreEqual("targetObject", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
            Assert.AreEqual("AuthenticationError: (targetObject:String) [], ApplicationException", error.CategoryInfo.ToString());
            Assert.AreEqual("AuthenticationError: (targetObject:String) [], ApplicationException", error.CategoryInfo.GetMessage());
        }

        [Test]
        public void TargetObjectIsUri()
        {
            var ex = new ApplicationException("Exception error message");
            var uri = new Uri("http://microsoft.com/");
            var error = new ErrorRecord(ex, "errorId", ErrorCategory.AuthenticationError, uri);

            Assert.AreEqual(uri, error.TargetObject);
            Assert.AreEqual("http://microsoft.com/", error.CategoryInfo.TargetName);
            Assert.AreEqual("Uri", error.CategoryInfo.TargetType);
        }

        [Test]
        public void TargetObjectIsXmlDocument()
        {
            var ex = new ApplicationException("Exception error message");
            var doc = new System.Xml.XmlDocument();
            var error = new ErrorRecord(ex, "errorId", ErrorCategory.AuthenticationError, doc);

            Assert.AreEqual(doc, error.TargetObject);
            Assert.AreEqual("System.Xml.XmlDocument", error.CategoryInfo.TargetName);
            Assert.AreEqual("XmlDocument", error.CategoryInfo.TargetType);
        }

        [Test]
        public void TargetObjectIsNull()
        {
            var ex = new ApplicationException("Exception error message");
            var error = new ErrorRecord(ex, "errorId", ErrorCategory.AuthenticationError, null);

            Assert.AreEqual(null, error.TargetObject);
            Assert.AreEqual("", error.CategoryInfo.TargetName);
            Assert.AreEqual("", error.CategoryInfo.TargetType);
            Assert.AreEqual("AuthenticationError: (:) [], ApplicationException", error.CategoryInfo.ToString());
            Assert.AreEqual("AuthenticationError: (:) [], ApplicationException", error.CategoryInfo.GetMessage());
        }

        [Test]
        public void ReasonIsSet()
        {
            var ex = new ApplicationException("Exception error message");
            var error = new ErrorRecord(ex, "errorId", ErrorCategory.AuthenticationError, "foo");
            error.CategoryInfo.Reason = "Reason";

            Assert.AreEqual("AuthenticationError: (foo:String) [], Reason", error.CategoryInfo.ToString());
        }

        [Test]
        public void ActivityIsSet()
        {
            var ex = new ApplicationException("Exception error message");
            var error = new ErrorRecord(ex, "errorId", ErrorCategory.AuthenticationError, "foo");
            error.CategoryInfo.Activity = "Activity";

            Assert.AreEqual("AuthenticationError: (foo:String) [Activity], ApplicationException", error.CategoryInfo.ToString());
        }

        [Test]
        public void NullException()
        {
            var ex = Assert.Throws<PSArgumentNullException>(delegate
            {
                var error = new ErrorRecord(null, "errorId", ErrorCategory.AuthenticationError, "foo");
            });

            Assert.AreEqual("Cannot process argument because the value of argument \"exception\" is null. Change the value of argument \"exception\" to a non-null value.", ex.Message);
        }
    }
}
