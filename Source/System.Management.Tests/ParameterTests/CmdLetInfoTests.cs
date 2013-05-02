// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using NUnit.Framework;

namespace System.Management.Tests.ParameterTests
{
    [TestFixture]
    public sealed class CmdLetInfoTests
    {
        private CmdletInfo info = null;

        [SetUp]
        public void LoadCmdInfo()
        {
            info = TestParameterCommand.CreateCmdletInfo();
        }

        [Test]
        public void ParameterSetCount()
        {
            Assert.AreEqual(3, info.ParameterSets.Count);
        }

        [Test]
        public void Parameters()
        {
            CommandParameterSetInfo allSet = info.GetParameterSetByName(ParameterAttribute.AllParameterSets);
            CommandParameterSetInfo fileSet = info.GetParameterSetByName("File");
            CommandParameterSetInfo variableSet = info.GetParameterSetByName("Variable");

            Assert.AreEqual(3, allSet.Parameters.Count);
            Assert.AreEqual(4, fileSet.Parameters.Count);
            Assert.AreEqual(4, variableSet.Parameters.Count);

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

            CommandParameterInfo recurseParam = set.GetParameterByName("Recurse");
            Assert.IsNotNull(recurseParam);
            Assert.AreEqual("Recurse", recurseParam.Name);
            Assert.AreEqual(-1, recurseParam.Position);
            Assert.AreEqual(false, recurseParam.IsMandatory);
        }

        [Test]
        public void Aliases()
        {
            CommandParameterSetInfo allSet = info.GetParameterSetByName(ParameterAttribute.AllParameterSets);

            Assert.IsNotNull(allSet);

            var nameParam = allSet.GetParameterByName("Name");
            Assert.IsNotNull(nameParam);

            Assert.IsNotNull(nameParam.Aliases);
            Assert.AreEqual(2, nameParam.Aliases.Count);
            Assert.Contains("FullName", nameParam.Aliases);
            Assert.Contains("fn", nameParam.Aliases);
        }

        [Test]
        public void Attributes()
        {
            CommandParameterSetInfo allSet = info.GetParameterSetByName(ParameterAttribute.AllParameterSets);

            Assert.IsNotNull(allSet);

            var nameParam = allSet.GetParameterByName("Name");
            Assert.IsNotNull(nameParam);

            Assert.IsNotNull(nameParam.Aliases);
            Assert.AreEqual(2, nameParam.Attributes.Count);
            Assert.AreEqual(1, nameParam.Attributes.Where(a => a is AliasAttribute).Count());
            Assert.AreEqual(1, nameParam.Attributes.Where(a => a is ParameterAttribute).Count());
        }
    }
}
