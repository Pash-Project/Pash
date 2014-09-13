// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using NUnit.Framework;
using TestPSSnapIn;

namespace ReferenceTests.API
{
    [TestFixture]
    public class TypeConverterTests : ReferenceTestBase
    {
        [Test]
        public void ConvertFromStringToTypeWithTypeConverterUsesTypeConverter()
        {
            string cmdletName = CmdletName(typeof(TestParameterUsesCustomTypeWithTypeConverterCommand));
            string command = string.Format("{0} Parameter1", cmdletName);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("CustomType.Id='Parameter1'" + Environment.NewLine, result);
        }

        [Test]
        public void LanguagePrimitivesConvertToTypeWithTypeConverter()
        {
            var result = LanguagePrimitives.ConvertTo("abc", typeof(Custom));
        
            Assert.IsInstanceOf(typeof(Custom), result);
            Assert.AreEqual("abc", ((Custom)result).Id);
        }
    }
}
