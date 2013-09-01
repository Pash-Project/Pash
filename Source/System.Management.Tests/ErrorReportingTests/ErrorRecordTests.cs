// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;
using System.Management.Automation;

namespace System.Management.Tests.ErrorReportingTests
{
    [TestFixture]
    public class ErrorRecordTests
    {
        [Test]
        public void ErrorRecordCreatedWithOperationTimeoutCategoryShouldHaveCategoryInfo()
        {
            var expectedCategory = ErrorCategory.OperationTimeout;
            var exception = new ApplicationException("Test");
            var errorRecord = new ErrorRecord(exception, "ErrorId", expectedCategory, null);

            ErrorCategoryInfo categoryInfo = errorRecord.CategoryInfo;

            Assert.AreEqual(expectedCategory, categoryInfo.Category);
            Assert.AreEqual(string.Empty, categoryInfo.Activity);
            Assert.AreEqual(string.Empty, categoryInfo.TargetName);
            Assert.AreEqual(string.Empty, categoryInfo.TargetType);
            Assert.AreEqual("ApplicationException", categoryInfo.Reason);
        }
    }
}
