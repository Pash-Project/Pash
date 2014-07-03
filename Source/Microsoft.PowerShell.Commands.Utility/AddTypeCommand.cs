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
        }

        private void AddAssemblies()
        {
            foreach (string name in AssemblyName)
            {
                Assembly.Load(name);
            }
        }

        private void AddTypeDefinition()
        {
            CompilerResults results = CodeDomProvider.CompileAssemblyFromSource(GetCompilerParameters(), TypeDefinition);
        }

        private CompilerParameters GetCompilerParameters()
        {
            return new CompilerParameters
            {
                GenerateInMemory = true
            };
        }

        private void AddMemberDefinition()
        {
            CodeCompileUnit compileUnit = GenerateMemberDefinitionCodeDom();
            CodeDomProvider.CompileAssemblyFromDom(GetCompilerParameters(), compileUnit);
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
    }
}
