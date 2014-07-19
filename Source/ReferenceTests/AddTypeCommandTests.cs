// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class AddTypeCommandTests : ReferenceTestBase
    {
        string tempFileName;

        [TearDown]
        public void RemoveTempFile()
        {
            if (tempFileName != null && File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }

        private string CreateTempFile(string fileName, string contents)
        {
            string directory = Path.GetTempPath();
            tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(tempFileName, contents);
            return tempFileName;
        }

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

        [Test]
        public void AddTypeDefinition()
        {
            string result = ReferenceHost.Execute(
@"$source = 'public class AddTypeDefinitionTestClass { }'
Add-Type -typedefinition $source
[AddTypeDefinitionTestClass].FullName"
);
            StringAssert.Contains("AddTypeDefinitionTestClass" + Environment.NewLine, result);
        }

        [Test]
        public void AddTypeDefinitionWithInvalidCSharpCode()
        {
            Exception ex = Assert.Throws(Is.InstanceOf(typeof(Exception)), () => {
                ReferenceHost.RawExecute("Add-Type -TypeDefinition 'public class ErrorTest --'");
            });
            Assert.AreEqual("Cannot add type. There were compilation errors.", ex.Message);
            // TODO: Exception should be CmdletInvocationException
            // TODO: Does not work with pash. Pash does not have the error records in the pipeline.
            // They are in a nested pipeline but do not reach the main pipeline
            //ErrorRecord[] errorRecords = ReferenceHost.GetLastRawErrorRecords();
            //Assert.AreEqual(3, errorRecords.Length, "Should be 3 compiler errors");
        }

        [Test]
        public void AddTypeDefinitionAndCallMethodOnNewInstance()
        {
            string result = ReferenceHost.Execute(
@"$source = 'public class AddTypeDefinitionAndCallMethodOnNewInstanceTestClass { public string WriteLine() { return ""Test""; } }'
Add-Type -typedefinition $source
$obj = New-Object AddTypeDefinitionAndCallMethodOnNewInstanceTestClass
$obj.WriteLine()"
);
            StringAssert.Contains("Test" + Environment.NewLine, result);
        }

        [Test]
        public void AddTypeDefinitionWithReferencedAssemblies()
        {
            string result = ReferenceHost.Execute(
@"$source = 'public class AddTypeDefinitionWithReferencedAssembliesTestClass { public string WriteLine() { return ""Name="" + typeof(System.Xml.XmlDocument).Name; } }'
Add-Type -typedefinition $source -ReferencedAssemblies 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
$obj = New-Object AddTypeDefinitionWithReferencedAssembliesTestClass
$obj.WriteLine()"
);
            StringAssert.Contains("Name=XmlDocument" + Environment.NewLine, result);
        }

        [Test]
        public void AddTypeDefinitionWithReferencedAssembliesByFileName()
        {
            string fileName = Assembly.ReflectionOnlyLoad("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Location;
            string result = ReferenceHost.Execute(
                NewlineJoin(
"$source = 'public class AddTypeDefinitionWithReferencedAssembliesByFileNameTestClass { public string WriteLine() { return \"Name=\" + typeof(System.Xml.XmlDocument).Name; } }'",
"Add-Type -typedefinition $source -ReferencedAssemblies '" + fileName + "'",
"$obj = New-Object AddTypeDefinitionWithReferencedAssembliesByFileNameTestClass",
"$obj.WriteLine()")
);
            StringAssert.Contains("Name=XmlDocument" + Environment.NewLine, result);
        }

        [Test]
        public void AddMemberDefinitionWithoutSpecifyingNamespace()
        {
            string result = ReferenceHost.Execute(
@"$method = 'public string WriteLine() { return ""Test""; }'
Add-Type -MemberDefinition $method -Name ""AddMemberDefinitionUsingDefaultNamespaceTestClass""
$obj = New-Object Microsoft.PowerShell.Commands.AddType.AutoGeneratedTypes.AddMemberDefinitionUsingDefaultNamespaceTestClass
$obj.WriteLine()
"
);
            StringAssert.Contains("Test" + Environment.NewLine, result);
        }

        [Test]
        public void AddMemberDefinitionSpecifyingTypeNamespace()
        {
            string result = ReferenceHost.Execute(
@"$method = 'public string WriteLine() { return ""Test""; }'
Add-Type -MemberDefinition $method -Name ""AddMemberDefinitionSpecifyingTypeNamespaceTestClass"" -Namespace ""AddTypeCommandNamespace""
$obj = New-Object AddTypeCommandNamespace.AddMemberDefinitionSpecifyingTypeNamespaceTestClass
$obj.WriteLine()
"
);
            StringAssert.Contains("Test" + Environment.NewLine, result);
        }

        [Test]
        public void AddMemberDefinitionReferencesSystemAndSystemRuntimeInteropServicesByDefault()
        {
            string result = ReferenceHost.Execute(
@"$method = 'public String WriteLine() { return typeof(DllImportAttribute).Name; }'
Add-Type -MemberDefinition $method -Name ""AddMemberDefinitionUsingNamespaceReferencesTestClass"" -Namespace ""AddTypeCommandNamespace""
$obj = New-Object AddTypeCommandNamespace.AddMemberDefinitionUsingNamespaceReferencesTestClass
$obj.WriteLine()
"
);
            StringAssert.Contains("DllImportAttribute" + Environment.NewLine, result);
        }
        
        [Test]
        public void AddMemberDefinitionImportingNamespace()
        {
            string result = ReferenceHost.Execute(
@"$method = 'public String WriteLine() { return typeof(Debugger).FullName; }'
Add-Type -MemberDefinition $method -UsingNamespace System.Diagnostics -Name ""AddMemberDefinitionImportingNamespaceTestClass"" -Namespace ""AddTypeCommandNamespace""
$obj = New-Object AddTypeCommandNamespace.AddMemberDefinitionImportingNamespaceTestClass
$obj.WriteLine()
"
);
            StringAssert.Contains("System.Diagnostics.Debugger" + Environment.NewLine, result);
        }

        [Test]
        public void AddMemberDefinitionWithInvalidCSharp()
        {
            Exception ex = Assert.Throws(Is.InstanceOf(typeof(Exception)), () => {
                ReferenceHost.RawExecute("add-type -name Test -memberdefinition 'public WriteLine() ---'");
            });
            Assert.AreEqual("Cannot add type. There were compilation errors.", ex.Message);
            // TODO: Exception should be CmdletInvocationException
            // TODO: Does not work with pash. Pash does not have the error records in the pipeline.
            // They are in a nested pipeline but do not reach the main pipeline
            //ErrorRecord[] errorRecords = ReferenceHost.GetLastRawErrorRecords();
            //Assert.AreEqual(2, errorRecords.Length, "Should be 2 compiler errors");
        }

        [Test]
        public void AddTypeFromCSharpSourceFile()
        {
            string fileName = CreateTempFile("AddTypeFromCSharpSourceFile.cs",
@"namespace AddTypeCommandTests
{
    public class AddTypeFromCSharpSourceFileTestClass
    {
        public string WriteLine() { return ""Test""; }
    }
}");
            string result = ReferenceHost.Execute(
                NewlineJoin(
"Add-Type -Path '" + fileName + "'",
"$obj = New-Object AddTypeCommandTests.AddTypeFromCSharpSourceFileTestClass",
 "$obj.WriteLine()"));

            StringAssert.Contains("Test" + Environment.NewLine, result);
        }

        [Test]
        public void AddTypeFromCSharpSourceFileWithInvalidCode()
        {
            string fileName = CreateTempFile("AddTypeFromCSharpSourceFileWithInvalidCode.cs", @"public class ErrorTest --");
            Exception ex = Assert.Throws(Is.InstanceOf(typeof(Exception)), () =>
            {
                ReferenceHost.RawExecute("Add-Type -Path '" + fileName + "'");
            });
            Assert.AreEqual("Cannot add type. There were compilation errors.", ex.Message);
        }

        [Test]
        public void AddTypeFromAssemblyUsingFullPathToAssembly()
        {
            Assembly assembly = Assembly.ReflectionOnlyLoad("Microsoft.Build.Engine, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            string result = ReferenceHost.Execute(
                NewlineJoin(
                "Add-Type -Path '" + assembly.Location + "'",
                "[Microsoft.Build.BuildEngine.Project].FullName"));

            StringAssert.Contains("Microsoft.Build.BuildEngine.Project" + Environment.NewLine, result);
        }

        [Test]
        public void AddTypeUsingAssemblyNameWithPassThru()
        {
            string result = ReferenceHost.Execute(
                NewlineJoin(
@"$acctype = add-type -assemblyname ""accessibility, version=4.0.0.0, culture=neutral,publickeytoken=b03f5f7f11d50a3a"",""Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" -passthru",
"$acctype | foreach-object { $_.FullName }",
"'$acctype.GetType().FullName=' + $acctype.GetType().FullName",
"'$acctype[0].GetType().FullName=' + $acctype[0].GetType().FullName"));

            StringAssert.Contains("$acctype.GetType().FullName=System.Object[]" + Environment.NewLine, result);
            StringAssert.Contains("$acctype[0].GetType().FullName=System.RuntimeType" + Environment.NewLine, result);
            StringAssert.Contains("Microsoft.CSharp.RuntimeBinder.Binder" + Environment.NewLine, result);
            StringAssert.Contains("Accessibility.IAccessible" + Environment.NewLine, result);
        }

        [Test]
        public void AddTypeFromAssemblyUsingFullPathToAssemblyWithPassThruShouldNotReturnAnyTypes()
        {
            Assembly assembly = Assembly.ReflectionOnlyLoad("Microsoft.Build.Engine, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            string result = ReferenceHost.Execute(
                    "Add-Type -Path '" + assembly.Location + "' -PassThru");

            StringAssert.Contains(String.Empty, result);
        }

        [Test]
        public void AddSingleTypeDefinitionWithPassThru()
        {
            string result = ReferenceHost.Execute(
                NewlineJoin(
"$source = 'public class AddTypeDefinitionWithPassThruTestClass { }'",
"$type = Add-Type -passthru -typedefinition $source",
"'$type.GetType().FullName=' + $type.GetType().FullName",
"'$type.FullName=' + $type.FullName"));

            StringAssert.Contains("$type.GetType().FullName=System.RuntimeType" + Environment.NewLine, result);
            StringAssert.Contains("$type.FullName=AddTypeDefinitionWithPassThruTestClass" + Environment.NewLine, result);
        }

        [Test]
        public void AddTwoTypeDefinitionsWithPassThru()
        {
            string result = ReferenceHost.Execute(
                NewlineJoin(
"$source = 'public class AddTwoTypeDefinitionsWithPassThruTestClass1 { } public class AddTwoTypeDefinitionsWithPassThruTestClass2 { }'",
"$types = Add-Type -Passthru -typedefinition $source",
"$types | foreach-object { $_.FullName }",
"'$types.GetType().FullName=' + $types.GetType().FullName",
"'$types[0].GetType().FullName=' + $types[0].GetType().FullName"));

            StringAssert.Contains("$types.GetType().FullName=System.Object[]" + Environment.NewLine, result);
            StringAssert.Contains("$types[0].GetType().FullName=System.RuntimeType" + Environment.NewLine, result);
            StringAssert.Contains("AddTwoTypeDefinitionsWithPassThruTestClass1" + Environment.NewLine, result);
            StringAssert.Contains("AddTwoTypeDefinitionsWithPassThruTestClass2" + Environment.NewLine, result);
        }

        [Test]
        public void AddMemberDefinitionWithPassThru()
        {
            string result = ReferenceHost.Execute(
                NewlineJoin(
"$method = 'public string WriteLine() { return \"Test\"; }'",
"$type = Add-Type -PassThru -MemberDefinition $method -Name 'AddMemberDefinitionSpecifyingTypeNamespaceTestClass' -Namespace 'AddTypeCommandNamespace'",
"'$type.GetType().FullName=' + $type.GetType().FullName",
"'$type.FullName=' + $type.FullName"));

            StringAssert.Contains("$type.GetType().FullName=System.RuntimeType" + Environment.NewLine, result);
            StringAssert.Contains("$type.FullName=AddTypeCommandNamespace.AddMemberDefinitionSpecifyingTypeNamespaceTestClass" + Environment.NewLine, result);
        }
    }
}
