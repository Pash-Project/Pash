// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using Microsoft.PowerShell.Commands;

namespace System.Management.Automation.Runspaces
{
    public class InitialSessionState
    {
        PSLanguageMode langmode;
        InitialSessionStateEntryCollection<SessionStateCommandEntry> sessionstatentry;
        InitialSessionStateEntryCollection<SessionStateProviderEntry> sessionstatprovider;

        protected InitialSessionState()
        {
            sessionstatentry = new InitialSessionStateEntryCollection<SessionStateCommandEntry>();
            sessionstatprovider = new InitialSessionStateEntryCollection<SessionStateProviderEntry>();
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
                return sessionstatentry;
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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

            AddDefaultCommands(initialSessionState);

            return initialSessionState;
        }

        public static InitialSessionState CreateDefault2()
        {
            InitialSessionState initialSessionState = new InitialSessionState();

            AddDefaultCommands(initialSessionState);

            return initialSessionState;
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
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("prompt", "$(if (test-path variable:/PSDebugContext) { '[DBG]: ' } else { '' }) + 'PS ' + $(Get-Location) + $(if ($nestedpromptlevel -ge 1) { '>>' }) + '> '"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("ImportSystemModules", "\r\n $SnapIns = @(Get-PSSnapin -Registered -ErrorAction SilentlyContinue)"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Clear-Host", "$space = New-Object System.Management.Automation.Host.BufferCell\n$space.Character = ' '\n$space.ForegroundColor = $host.ui.rawui.ForegroundColor\n$space.BackgroundColor = $host.ui.rawui.BackgroundColor\n$rect = New-Object System.Management.Automation.Host.Rectangle\n$rect.Top = $rect.Bottom = $rect.Right = $rect.Left = -1\n$origin = New-Object System.Management.Automation.Host.Coordinates\n$Host.UI.RawUI.CursorPosition = $origin\n$Host.UI.RawUI.SetBufferContents($rect, $space)\n"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("more", "param([string[]]$paths)\n\n$OutputEncoding = [System.Console]::OutputEncoding\n\nif($paths)\n{\n    foreach ($file in $paths)\n    {\n        Get-Content $file | more.com\n    }\n}\nelse\n{\n    $input | more.com\n}\n"));
            initialSessionState.Commands.Add(new SessionStateFunctionEntry("Get-Verb", "\r\nparam(\r\n    [Parameter(ValueFromPipeline=$true)]\r\n    [string[]]\r\n    $verb = '*'\r\n)\r\nbegin {\r\n    $allVerbs = [PSObject].Assembly.GetTypes() |\r\n        Where-Object {$_.Name -match '^Verbs.'} |\r\n        Get-Member -type Properties -static |\r\n        Select-Object @{\r\n            Name='Verb'\r\n            Expression = {$_.Name}\r\n        }, @{\r\n            Name='Group'\r\n            Expression = {\r\n                $str = \"$($_.TypeName)\"\r\n                $str.Substring($str.LastIndexOf('Verbs') + 5)\r\n            }                \r\n        }        \r\n}\r\nprocess {\r\n    foreach ($v in $verb) {\r\n        $allVerbs | Where-Object { $_.Verb -like $v }\r\n    }       \r\n}\r\n"));
            // TODO:
            // "TabExpansion"
            // "help"
            // "mkdir"
            // "Disable-PSRemoting"
            
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
            throw new NotImplementedException();
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
