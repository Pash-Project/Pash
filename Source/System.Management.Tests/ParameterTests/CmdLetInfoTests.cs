// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Implement, may require runtime improvements.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using NUnit.Framework;

namespace System.Management.Tests
{
    [TestFixture]
    [Cmdlet("Test", "ParameterReflection")]
    public sealed class CmdLetInfoTests : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "File")]
        public string FilePath { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Variable")]
        public string Variable { get; set; }

        [Parameter]
        [Alias(new string[] { "FullName","fn" })]
        public string Name;

        // ValueFromPipeline need public getter so this should be skipped
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "TheseShouldBeSkipped")]
        public int Length { private get; set; }

        // All properties require a public setter so this should be skipped
        [Parameter(ParameterSetName = "TheseShouldBeSkipped")]
        public int Height { get; private set; }

        // Fields must be public so this should be skipped
        [Parameter]
        private int Age;
        // To avoid generating warning CS0414
        void Ignore() { this.Age++; }

        private CmdletInfo info = null;

        [SetUp]
        public void LoadCmdInfo()
        {
            Age = 0;
            info = new CmdletInfo("Test-ParameterReflectionCommand", this.GetType(), "", null, null);
        }

        [Test]
        public void Parameters()
        {
            Assert.AreEqual(3, info.ParameterSets.Count);

            CommandParameterSetInfo allSet = info.GetParameterSetByName(ParameterAttribute.AllParameterSets);
            CommandParameterSetInfo fileSet = info.GetParameterSetByName("File");
            CommandParameterSetInfo variableSet = info.GetParameterSetByName("Variable");

            Assert.AreEqual(2, allSet.Parameters.Count);
            Assert.AreEqual(3, fileSet.Parameters.Count);
            Assert.AreEqual(3, variableSet.Parameters.Count);

            Assert.IsNotNull(allSet);
            Assert.IsNotNull(fileSet);
            Assert.IsNotNull(variableSet);
            Assert.IsNull(info.GetParameterSetByName("TheseShouldBeSkipped"));

            CheckForAllSetsParameters(allSet);
            CheckForAllSetsParameters(fileSet);
            CheckForAllSetsParameters(variableSet);

            CommandParameterInfo filePathParam = fileSet.GetParameterByName("FilePath");
            Assert.IsNotNull(filePathParam);
            Assert.AreEqual("FilePath", filePathParam.Name);
            Assert.AreEqual(0, filePathParam.Position);
            Assert.AreEqual(true, filePathParam.IsMandatory);

            CommandParameterInfo variableParam = variableSet.GetParameterByName("Variable");
            Assert.IsNotNull(variableParam);
            Assert.AreEqual("Variable", variableParam.Name);
            Assert.AreEqual(-1, variableParam.Position);
            Assert.AreEqual(true, variableParam.IsMandatory);

            CommandParameterInfo ageParam = fileSet.GetParameterByName("Age");
            Assert.IsNull(ageParam);

        }

        private void CheckForAllSetsParameters(CommandParameterSetInfo set)
        {
            CommandParameterInfo inputObjectParam = set.GetParameterByName("InputObject");
            Assert.IsNotNull(inputObjectParam);
            Assert.AreEqual("InputObject", inputObjectParam.Name);
            Assert.AreEqual(-1, inputObjectParam.Position);
            Assert.AreEqual(false, inputObjectParam.IsMandatory);

            CommandParameterInfo nameParam = set.GetParameterByName("Name");
            Assert.IsNotNull(nameParam);
            Assert.AreEqual("Name", nameParam.Name);
            Assert.AreEqual(-1, nameParam.Position);
            Assert.AreEqual(false, nameParam.IsMandatory);
        }

        [Test, Explicit("Scanning for aliases not yet implemented, see: CommandParameterInfo.CommandParameterInfo()")]
        public void Aliases()
        {
            Assert.AreEqual(info.ParameterSets.Count, 3);

            CommandParameterSetInfo fileSet = info.GetParameterSetByName("File");
            CommandParameterSetInfo allSet = info.GetParameterSetByName(ParameterAttribute.AllParameterSets);

            Assert.IsNotNull(allSet);

            var nameParam = allSet.GetParameterByName("Name");
            Assert.IsNotNull(nameParam);

            Assert.IsNotNull(nameParam.Aliases);
            Assert.AreEqual(2, nameParam.Aliases.Count);
            Assert.Contains("FullName", nameParam.Aliases);
            Assert.Contains("fn", nameParam.Aliases);
        }

        [Test,Explicit("Attributes collection not yet being populated, see: CommandParameterInfo.CommandParameterInfo()")]
        public void Attributes()
        {
            Assert.AreEqual(info.ParameterSets.Count, 3);

            CommandParameterSetInfo allSet = info.GetParameterSetByName(ParameterAttribute.AllParameterSets);

            Assert.IsNotNull(allSet);

            var nameParam = allSet.GetParameterByName("Name");
            Assert.IsNotNull(nameParam);

            Assert.IsNotNull(nameParam.Aliases);
            Assert.AreEqual(2, nameParam.Attributes.Count);
            Assert.AreEqual(1, nameParam.Attributes.Where( a => a is AliasAttribute).Count());
            Assert.AreEqual(1, nameParam.Attributes.Where( a => a is ParameterAttribute).Count());
        }
	}
}
