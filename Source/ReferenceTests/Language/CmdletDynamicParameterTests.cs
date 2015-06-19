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
            ExecuteAndCompareTypedResult(_cmdletName, false, null, null);
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
            ExecuteAndCompareTypedResult(_cmdletName + " -MessageOne foo -UseParameters", true, null, parameters);
        }

        [Test]
        public void DynamicParametersCanUsePositionalOnlyAfterNormalParameters()
        {
            var parameters = new TestDynamicParameters("foo", null);
            ExecuteAndCompareTypedResult(_cmdletName + " -DefaultMessage bar -UseParameters foo", true, "bar", parameters);
        }
        

        [Test]
        public void DynamicParametersPositionIsResolvedAfterNormalParameterPosition()
        {
            // although the normal parameter "DefaultMessage" has position 5, and the dynamic parameter has position 0,
            // normal parameters by position are resolved and bound first
            var parameters = new TestDynamicParameters("bar", null);
            ExecuteAndCompareTypedResult(_cmdletName + " foo bar -UseParameters", true, "foo", parameters);
        }

        [Test]
        public void DynamicParametersCanTakeOptionalParameters()
        {
            var parameters = new TestDynamicParameters("foo", "bar");
            ExecuteAndCompareTypedResult(_cmdletName + " -MessageOne foo -UseParameters -MessageTwo 'bar' ", true, null, parameters);
        }
    }
}
