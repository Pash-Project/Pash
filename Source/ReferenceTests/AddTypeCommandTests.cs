// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class AddTypeCommandTests : ReferenceTestBase
    {
        [Test]
        public void AddTypeAddsAssemblyToCurrentAppDomain()
        {
            string result = ReferenceHost.Execute(
@"Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
[System.AppDomain]::CurrentDomain.GetAssemblies() | Foreach-Object { $_.fullname.split(',')[0] }
");

            StringAssert.Contains("Microsoft.Build" + Environment.NewLine, result);
        }

        [Test]
        public void TypesFromAddedAssemblyAvailable()
        {
            string result = ReferenceHost.Execute(
@"Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
[Microsoft.Build.Evaluation.Project].FullName
");

            StringAssert.Contains("Microsoft.Build.Evaluation.Project" + Environment.NewLine, result);
        }
    }
}
