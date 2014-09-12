// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using Microsoft.PowerShell.Commands;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Runspaces
{
    public class InitialSessionState
    {
        PSLanguageMode langmode;
        InitialSessionStateEntryCollection<SessionStateCommandEntry> _commandEntries;
        InitialSessionStateEntryCollection<SessionStateProviderEntry> sessionstatprovider;
        InitialSessionStateEntryCollection<SessionStateVariableEntry> variables;
        List<ModuleSpecification> modules = new List<ModuleSpecification>();

        protected InitialSessionState()
        {
            _commandEntries = new InitialSessionStateEntryCollection<SessionStateCommandEntry>();
            sessionstatprovider = new InitialSessionStateEntryCollection<SessionStateProviderEntry>();
            variables = new InitialSessionStateEntryCollection<SessionStateVariableEntry>();
        }

        #region Properties

        public ApartmentState ApartmentState
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateAssemblyEntry> Assemblies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual AuthorizationManager AuthorizationManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateCommandEntry> Commands
        {
            get
            {
                return _commandEntries;
            }
        }

        public bool DisableFormatUpdates
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateFormatEntry> Formats
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PSLanguageMode LanguageMode
        {
            get
            {
                return langmode;
            }
            set
            {
                langmode = value;
            }
        }

        public ReadOnlyCollection<ModuleSpecification> Modules
        {
            get
            {
                return modules.AsReadOnly();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateProviderEntry> Providers
        {
            get
            {
                return sessionstatprovider;
            }
        }

        public PSThreadOptions ThreadOptions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public bool ThrowOnRunspaceOpenError
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateTypeEntry> Types
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool UseFullLanguageModeInDebugger
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateVariableEntry> Variables
        {
            get
            {
                return variables;
            }
        }

        #endregion

        public InitialSessionState Clone()
        {
            throw new NotImplementedException();
        }

        public static InitialSessionState Create()
        {
            return new InitialSessionState();
        }

        public static InitialSessionState Create(string snapInName)
        {
            return new InitialSessionState();
        }

        public static InitialSessionState Create(string[] snapInNameCollection, out PSConsoleLoadException warning)
        {
            warning = null;
            return new InitialSessionState();
        }

        public static InitialSessionState CreateDefault()
        {
            InitialSessionState initialSessionState = new InitialSessionState();

            // TODO: I think this is also the correct location for importing the default snapins, not in SessionStateGlobal

            AddDefaultVariables(initialSessionState);
            AddDefaultCommands(initialSessionState);

            return initialSessionState;
        }

        public static InitialSessionState CreateDefault2()
        {
            return CreateDefault();
        }

        static private void AddDefaultVariables(InitialSessionState initialSessionState)
        {
            initialSessionState.Variables.Add(new SessionStateVariableEntry("true", true, "", ScopedItemOptions.Constant));
            initialSessionState.Variables.Add(new SessionStateVariableEntry("false", false, "", ScopedItemOptions.Constant));
            initialSessionState.Variables.Add(new SessionStateVariableEntry("null", null, "", ScopedItemOptions.Constant));
            initialSessionState.Variables.Add(new SessionStateVariableEntry("Error", new Collection<ErrorRecord>(), "Last errors", ScopedItemOptions.Constant));
            initialSessionState.Variables.Add(new SessionStateVariableEntry("?", true, "Last command success", ScopedItemOptions.Constant));
        }

        static private void AddDefaultCommands(InitialSessionState initialSessionState)
        {
            // Add Default Commands
            initialSessionState.Commands.Add(new SessionStateApplicationEntry("*"));
            initialSessionState.Commands.Add(new SessionStateScriptEntry("*"));

            // Default Functions
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("A:", "Set-Location A:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("B:", "Set-Location B:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("C:", "Set-Location C:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("D:", "Set-Location D:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("E:", "Set-Location E:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("F:", "Set-Location F:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("G:", "Set-Location G:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("H:", "Set-Location H:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("I:", "Set-Location I:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("J:", "Set-Location J:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("K:", "Set-Location K:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("L:", "Set-Location L:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("M:", "Set-Location M:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("N:", "Set-Location N:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("O:", "Set-Location O:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("P:", "Set-Location P:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Q:", "Set-Location Q:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("R:", "Set-Location R:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("S:", "Set-Location S:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("T:", "Set-Location T:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("U:", "Set-Location U:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("V:", "Set-Location V:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("W:", "Set-Location W:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("X:", "Set-Location X:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Y:", "Set-Location Y:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Z:", "Set-Location Z:"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("cd..", "Set-Location .."));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("cd\\", "Set-Location \\"));


            /* not yet working
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("prompt", "$(if (test-path variable:/PSDebugContext) { '[DBG]: ' } else { '' }) + 'PS ' + $(Get-Location) + $(if ($nestedpromptlevel -ge 1) { '>>' }) + '> '"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("ImportSystemModules", "\r\n $SnapIns = @(Get-PSSnapin -Registered -ErrorAction SilentlyContinue)"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Clear-Host", "$space = New-Object System.Management.Automation.Host.BufferCell\n$space.Character = ' '\n$space.ForegroundColor = $host.ui.rawui.ForegroundColor\n$space.BackgroundColor = $host.ui.rawui.BackgroundColor\n$rect = New-Object System.Management.Automation.Host.Rectangle\n$rect.Top = $rect.Bottom = $rect.Right = $rect.Left = -1\n$origin = New-Object System.Management.Automation.Host.Coordinates\n$Host.UI.RawUI.CursorPosition = $origin\n$Host.UI.RawUI.SetBufferContents($rect, $space)\n"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("more", "param([string[]]$paths)\n\n$OutputEncoding = [System.Console]::OutputEncoding\n\nif($paths)\n{\n    foreach ($file in $paths)\n    {\n        Get-Content $file | more.com\n    }\n}\nelse\n{\n    $input | more.com\n}\n"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Get-Verb", "\r\nparam(\r\n    [Parameter(ValueFromPipeline=$true)]\r\n    [string[]]\r\n    $verb = '*'\r\n)\r\nbegin {\r\n    $allVerbs = [PSObject].Assembly.GetTypes() |\r\n        Where-Object {$_.Name -match '^Verbs.'} |\r\n        Get-Member -type Properties -static |\r\n        Select-Object @{\r\n            Name='Verb'\r\n            Expression = {$_.Name}\r\n        }, @{\r\n            Name='Group'\r\n            Expression = {\r\n                $str = \"$($_.TypeName)\"\r\n                $str.Substring($str.LastIndexOf('Verbs') + 5)\r\n            }                \r\n        }        \r\n}\r\nprocess {\r\n    foreach ($v in $verb) {\r\n        $allVerbs | Where-Object { $_.Verb -like $v }\r\n    }       \r\n}\r\n"));
            */
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Prompt", "'PASH ' + (Get-Location) + '> '"));
            // TODO:
            // "TabExpansion"
            // "help"
            // "mkdir"
            // "Disable-PSRemoting"

            // Default Aliases
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ac", "Add-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("asnp", "Add-PSSnapIn", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cat", "Get-Content", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cd", "Set-Location", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("chdir", "Set-Location", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cp", "Copy-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("clc", "Clear-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("clear", "Clear-Host", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("clhy", "Clear-History", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cli", "Clear-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("clp", "Clear-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cls", "Clear-Host", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("clv", "Clear-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("compare", "Compare-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("copy", "Copy-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cpi", "Copy-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cpp", "Copy-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("cvpa", "Convert-Path", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("dbp", "Disable-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("del", "Remove-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("dir", "Get-ChildItem", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("diff", "Compare-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ebp", "Enable-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("echo", "Write-Output", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("epal", "Export-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("epcsv", "Export-Csv", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("erase", "Remove-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("epsn", "Export-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("etsn", "Enter-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("exsn", "Exit-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("fc", "Format-Custom", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("fl", "Format-List", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("foreach", "ForEach-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("%", "ForEach-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ft", "Format-Table", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("fw", "Format-Wide", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gal", "Get-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gbp", "Get-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gc", "Get-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gci", "Get-ChildItem", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gcm", "Get-Command", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gdr", "Get-PSDrive", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gcs", "Get-PSCallStack", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ghy", "Get-History", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gi", "Get-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gjb", "Get-Job", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gl", "Get-Location", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gm", "Get-Member", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gmo", "Get-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gp", "Get-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gps", "Get-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("group", "Group-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gsn", "Get-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gsv", "Get-Service", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gsnp", "Get-PSSnapIn", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gu", "Get-Unique", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gv", "Get-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("gwmi", "Get-WmiObject", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("h", "Get-History", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("history", "Get-History", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("icm", "Invoke-Command", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("iex", "Invoke-Expression", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ihy", "Invoke-History", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ii", "Invoke-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ipmo", "Import-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ipsn", "Import-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ise", "powershell_ise.exe", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("iwmi", "Invoke-WMIMethod", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ipal", "Import-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ipcsv", "Import-Csv", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("kill", "Stop-Process", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("lp", "Out-Printer", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ls", "Get-ChildItem", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("man", "help", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("md", "mkdir", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("measure", "Measure-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("mi", "Move-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("mount", "New-PSDrive", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("move", "Move-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("mp", "Move-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("mv", "Move-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("nal", "New-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ndr", "New-PSDrive", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ni", "New-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("nv", "New-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("nmo", "New-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("nsn", "New-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("oh", "Out-Host", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ogv", "Out-GridView", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("popd", "Pop-Location", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ps", "Get-Process", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("pushd", "Push-Location", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("pwd", "Get-Location", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("r", "Invoke-History", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rbp", "Remove-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rcjb", "Receive-Job", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rd", "Remove-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rdr", "Remove-PSDrive", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ren", "Rename-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("ri", "Remove-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rjb", "Remove-Job", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rm", "Remove-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rmdir", "Remove-Item", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rni", "Rename-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rnp", "Rename-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rp", "Remove-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rmo", "Remove-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rsn", "Remove-PSSession", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rsnp", "Remove-PSSnapin", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rv", "Remove-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rwmi", "Remove-WMIObject", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("rvpa", "Resolve-Path", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sajb", "Start-Job", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sal", "Set-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sasv", "Start-Service", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sbp", "Set-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sc", "Set-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("select", "Select-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("set", "Set-Variable", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("si", "Set-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sl", "Set-Location", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("swmi", "Set-WMIInstance", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sleep", "Start-Sleep", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sort", "Sort-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sp", "Set-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("spjb", "Stop-Job", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("saps", "Start-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("start", "Start-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("spps", "Stop-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("spsv", "Stop-Service", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("sv", "Set-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("tee", "Tee-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("type", "Get-Content", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("where", "Where-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("wjb", "Wait-Job", "", ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("write", "Write-Output", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
            initialSessionState.Commands.Add(new SessionStateAliasEntry("?", "Where-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope));
        }

        public static InitialSessionState CreateFrom(string snapInPath, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return new InitialSessionState();
        }
        public static InitialSessionState CreateFrom(string[] snapInPathCollection, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return new InitialSessionState();
        }

        public static InitialSessionState CreateRestricted(SessionCapabilities sessionCapabilities)
        {
            InitialSessionState initialSessionState = new InitialSessionState();

            return initialSessionState;
        }

        public void ImportPSModule(string[] name)
        {
            var specifications = from string moduleName in name
                                select new ModuleSpecification(moduleName);

            modules.AddRange(specifications);
        }

        public void ImportPSModulesFromPath(string path)
        {
            throw new NotImplementedException();
        }

        public PSSnapInInfo ImportPSSnapIn(string name, out PSSnapInException warning)
        {
            throw new NotImplementedException();
        }


    }
}
