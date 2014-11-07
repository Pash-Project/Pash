using System;
using NUnit.Framework;
using System.Management.Automation.Language;
using System.Management.Automation;

namespace ReferenceTests.Parsing
{
    [TestFixture]
    public class CommandParsingTests : ReferenceTestBase
    {

        [Test]
        public void ParseExplicitlySetArgument()
        {
            // the following command should parse "-bar:" as the value of the -foo parameter
            var cmd = "{x -foo: -bar:}.Ast.EndBlock.Statements[0].PipelineElements[0].CommandElements";
            var result = ReferenceHost.RawExecute(cmd);
            Assert.That(result.Count, Is.EqualTo(2));
            var cmdParamAst = result[1].BaseObject as CommandParameterAst;
            Assert.That(cmdParamAst, Is.Not.Null);
            Assert.That(cmdParamAst.ParameterName, Is.EqualTo("foo"));
            Assert.That(cmdParamAst.Argument.ToString(), Is.EqualTo("-bar:"));
        }

        [TestCase("")] // no argument
        [TestCase("2>1", Ignore = true, IgnoreReason = "Currently not correctly parsed by Pash")] // redirection
        public void ParseExplicitlySetArgumentThrowsWithInvalidArgument(string arg)
        {
            Assert.Throws<ParseException>(delegate {
                ReferenceHost.RawExecute("{x -foo: " + arg + "}.Ast");
            });
        }
    }
}

