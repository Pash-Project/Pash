// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Management.Automation;
using System.Linq;
using TestPSSnapIn;

namespace ReferenceTests.Language
{
    class CmdletDynamicParameterTests : ReferenceTestBaseWithTestModule
    {
        private string _cmdletName = CmdletName(typeof(TestDynamicParametersConditionally));

        [Test]
        public void NoDynamicParametersIsNoProblem()
        {
            ExecuteAndCompareTypedResult(_cmdletName, false, null);
        }

        [Test]
        public void DynamicParametersNoMandatoriesThrows()
        {
            Assert.Throws<ParameterBindingException>(delegate {
                ReferenceHost.Execute(_cmdletName + " -UseParameters");
            });
        }

        [Test]
        public void DynamicParametersNotNeededThrows()
        {
            Assert.Throws<ParameterBindingException>(delegate {
                ReferenceHost.Execute(_cmdletName + " -MessageOne foo");
            });
        }

        [Test]
        public void DynamicParametersCanTakeOnlyMandatory()
        {
            var parameters = new TestDynamicParameters("foo", null);
            ExecuteAndCompareTypedResult(_cmdletName + " -MessageOne foo -UseParameters", true, parameters);
        }

        [Test]
        public void DynamicParametersCanUsePositionalParameters()
        {
            var parameters = new TestDynamicParameters("foo", null);
            ExecuteAndCompareTypedResult(_cmdletName + " foo -UseParameters", true, parameters);
        }

        [Test]
        public void DynamicParametersCanTakeOptionalParameters()
        {
            var parameters = new TestDynamicParameters("foo", "bar");
            ExecuteAndCompareTypedResult(_cmdletName + " foo -UseParameters -MessageTwo 'bar' ", true, parameters);
        }
    }
}
