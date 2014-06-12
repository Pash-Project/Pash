// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Member")]
    public class GetMemberCommand : PSCmdlet
    {
        private HashSet<string> _displayedTypes;

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Alias(new string[] { "Type" }), Parameter]
        public PSMemberTypes MemberType { get; set; }

        [Parameter(Position = 0)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter Static { get; set; }

        public GetMemberCommand()
        {
            MemberType = PSMemberTypes.All;
            _displayedTypes = new HashSet<string>();
        }

        protected override void ProcessRecord()
        {
            if (InputObject == null || InputObject.BaseObject == null)
            {
                return;
            }

            string fullName = InputObject.TypeNames.Count != 0 ? this.InputObject.TypeNames[0] : "<null>";

            if (_displayedTypes.Contains(fullName))
            {
                return;
            }

            _displayedTypes.Add(fullName);

            if (Name == null)
            {
                Name = new string[] { "*" }; // select all members
            }

            foreach (string name in Name)
            {
                ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos;

                if (Static.IsPresent)
                {
                    infos = InputObject.StaticMembers.Match(name, this.MemberType);
                }
                else
                {
                    infos = InputObject.Members.Match(name, this.MemberType);
                }

                List<MemberDefinition> members = new List<MemberDefinition>();
                foreach (PSMemberInfo info in infos)
                {
                    members.Add(new MemberDefinition(fullName, info.Name, info.MemberType, info.ToString()));
                }

                members.Sort((def1, def2) =>
                {
                    int diff = string.Compare(def1.MemberType.ToString(), def2.MemberType.ToString(),
                                       StringComparison.CurrentCultureIgnoreCase);
                    if (diff != 0)
                    {
                        return diff;
                    }
                    return string.Compare(def1.Name, def2.Name, StringComparison.CurrentCultureIgnoreCase);
                });

                foreach (MemberDefinition definition in members)
                {
                    WriteObject(definition);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (_displayedTypes.Count == 0)
            {
                WriteError(new ErrorRecord(new InvalidOperationException("No input object specified"),
                                           "NoObjectInGetMember", ErrorCategory.CloseError, null));
            }
        }
    }
}
