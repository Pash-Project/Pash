// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class FunctionTests : ReferenceTestBase
    {
        [Test]
        public void FunctionDeclarationWithoutParameterList()
        {
            Assert.DoesNotThrow(
                delegate() { ReferenceHost.Execute("function f() { 'x' }"); }
            );
        }

        [Test]
        public void FunctionParamsDefaultValueCanBeExpression()
        {
            var cmd = NewlineJoin(
                "function f { param($test=(get-location).path)",
                "$test",
                "}",
                "f");
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(Environment.CurrentDirectory), result);
        }
    }
}

