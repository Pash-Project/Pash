// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using Microsoft.CSharp;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "Type", DefaultParameterSetName = "FromSource"
        /*HelpUri="http://technet.microsoft.com/en-us/library/hh849914.aspx",
                 RemotingCapability=RemotingCapability.None*/)]
    [OutputType(typeof(Type))]
    public class AddTypeCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            ParameterSetName = "FromAssemblyName")] 
        public string[] AssemblyName { get; set; }

        [Parameter(ParameterSetName = "FromSource")] 
        [Parameter(ParameterSetName = "FromMember")] 
        public CodeDomProvider CodeDomProvider { get; set; }

        [Parameter(ParameterSetName = "FromSource")]
        [Parameter(ParameterSetName = "FromMember")]
        [Parameter(ParameterSetName = "FromPath")]
        [Parameter(ParameterSetName = "FromLiteralPath")] 
        public CompilerParameters CompilerParameters { get; set; }

        [Parameter]
        public SwitchParameter IgnoreWarnings { get; set; }

        [Parameter(ParameterSetName = "FromMember")]
        [Parameter(ParameterSetName = "FromSource")] 
        public Language Language { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "FromLiteralPath")] 
        public string[] LiteralPath { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1,
            ParameterSetName = "FromMember")]
        public string[] MemberDefinition { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = "FromMember")] 
        public string Name { get; set; }

        [Parameter(ParameterSetName = "FromMember")] 
        [AllowNull]
        public string Namespace { get; set; }

        [Parameter(ParameterSetName = "FromMember")]
        [Parameter(ParameterSetName = "FromPath")]
        [Parameter(ParameterSetName = "FromLiteralPath")]
        [Parameter(ParameterSetName = "FromSource")]
        public string OutputAssembly { get; set; }

        [Parameter(ParameterSetName = "FromLiteralPath")] 
        [Parameter(ParameterSetName = "FromMember")] 
        [Parameter(ParameterSetName = "FromPath")] 
        [Parameter(ParameterSetName = "FromSource")] 
        public OutputAssemblyType OutpuType { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = "FromPath")]
        public string[] Path { get; set; }

        [Parameter(ParameterSetName = "FromLiteralPath")]
        [Parameter(ParameterSetName = "FromPath")]
        [Parameter(ParameterSetName = "FromMember")]
        [Parameter(ParameterSetName = "FromSource")]
        public string[] ReferencedAssemblies { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = "FromSource")]
        public string TypeDefinition { get; set; }

        [ParameterAttribute(ParameterSetName="FromMember")]
        public string[] UsingNamespace { get; set; }

        private List<Assembly> assembliesAdded = new List<Assembly>();

        public AddTypeCommand()
        {
            CodeDomProvider = new CSharpCodeProvider();
        }

        protected override void ProcessRecord()
        {
            if (AssemblyName != null)
            {
                AddAssemblies();
            }
            else if (!String.IsNullOrEmpty(TypeDefinition))
            {
                AddTypeDefinition();
            }
            else if (MemberDefinition != null)
            {
                AddMemberDefinition();
            }
            else if (Path != null)
            {
                AddTypeDefinitionFromPath();
            }
        }

        private void AddAssemblies()
        {
            foreach (string name in AssemblyName)
            {
                assembliesAdded.Add(Assembly.Load(name));
            }

            WriteTypesAdded();
        }

        private void AddTypeDefinition()
        {
            CompilerResults results = CodeDomProvider.CompileAssemblyFromSource(GetCompilerParameters(), TypeDefinition);
            ReportResults(results);
            WriteTypesAdded(results);
        }

        private CompilerParameters GetCompilerParameters()
        {
            var parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.AddRange(GetReferencedAssemblies());
            return parameters;
        }

        private void ReportResults(CompilerResults results)
        {
            foreach (CompilerError error in results.Errors)
            {
                WriteError(CreateErrorRecord(error));
            }

            if (results.Errors.HasErrors)
            {
                var exception = new InvalidOperationException("Cannot add type. There were compilation errors.");
                var errorRecord = new ErrorRecord(exception, GetErrorId("COMPILER_ERRORS"), ErrorCategory.InvalidData, null);
                ThrowTerminatingError(errorRecord);
            }
        }

        private string GetErrorId(string id)
        {
            return String.Format("{0},{1}", id, GetType().FullName);
        }

        private ErrorRecord CreateErrorRecord(CompilerError error)
        {
            return new ErrorRecord(
                new Exception(error.ToString()),
                GetErrorId("SOURCE_CODE_ERROR"),
                ErrorCategory.InvalidData,
                null);
        }

        private string[] GetReferencedAssemblies()
        {
            if (ReferencedAssemblies == null)
                return new string[0];

            return (from reference in ReferencedAssemblies
                    select GetAssemblyPath(reference)).ToArray();
        }

        private string GetAssemblyPath(string reference)
        {
            try
            {
                return Assembly.ReflectionOnlyLoad(reference).Location;
            }
            catch
            {
                return Assembly.ReflectionOnlyLoadFrom(reference).Location;
            }
        }

        private void AddMemberDefinition()
        {
            CodeCompileUnit compileUnit = GenerateMemberDefinitionCodeDom();
            CompilerResults results = CodeDomProvider.CompileAssemblyFromDom(GetCompilerParameters(), compileUnit);
            ReportResults(results);

            WriteTypesAdded(results);
        }

        private CodeCompileUnit GenerateMemberDefinitionCodeDom()
        {
            var typeDeclaration = new CodeTypeDeclaration(Name);
            typeDeclaration.Members.AddRange(GetMembersCodeDom());

            var ns = new CodeNamespace(GetNamespace());
            ns.Imports.AddRange(GetMemberDefinitionCodeDomNamespaces());
            ns.Types.Add(typeDeclaration);

            var unit = new CodeCompileUnit();
            unit.Namespaces.Add(ns);

            return unit;
        }

        private CodeTypeMember[] GetMembersCodeDom()
        {
            return (from member in MemberDefinition
                    select new CodeSnippetTypeMember(member)).ToArray();
        }

        private CodeNamespaceImport[] GetMemberDefinitionCodeDomNamespaces()
        {
            var imports = new List<CodeNamespaceImport>();
            imports.Add(new CodeNamespaceImport("System"));
            imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));

            if (UsingNamespace != null)
            {
                imports.AddRange(from ns in UsingNamespace
                                 select new CodeNamespaceImport(ns));
            }

            return imports.ToArray();
        }

        private string GetNamespace()
        {
            if (Namespace != null)
            {
                return Namespace;
            }
            return "Microsoft.PowerShell.Commands.AddType.AutoGeneratedTypes";
        }

        private void AddTypeDefinitionFromPath()
        {
            if (HasAssemblyPaths(Path))
            {
                foreach (string fileName in Path)
                {
                    Assembly.LoadFile(fileName);
                }
            }
            else
            {
                CompilerResults results = CodeDomProvider.CompileAssemblyFromFile(GetCompilerParameters(), Path);
                ReportResults(results);
            }
        }

        private bool HasAssemblyPaths(string[] paths)
        {
            string fileExtension = System.IO.Path.GetExtension(paths[0]);
            return String.Equals(".dll", fileExtension, StringComparison.OrdinalIgnoreCase);
        }

        private void WriteTypesAdded()
        {
            if (!PassThru.IsPresent)
                return;

            object[] typesAdded = GetTypesAdded();
            WriteTypesAdded(typesAdded);

            assembliesAdded.Clear();
        }

        private void WriteTypesAdded(object[] typesAdded)
        {
            if (typesAdded.Length == 1)
            {
                WriteObject(typesAdded[0]);
            }
            else
            {
                WriteObject(typesAdded);
            }
        }

        private object[] GetTypesAdded()
        {
            return (from assembly in assembliesAdded
                    let types = assembly.GetTypes()
                    from type in types
                    select type).ToArray<object>();
        }

        private void WriteTypesAdded(CompilerResults results)
        {
            if (results.Errors.HasErrors || !PassThru.IsPresent)
                return;

            assembliesAdded.Add(results.CompiledAssembly);

            WriteTypesAdded();
        }
    }
}
