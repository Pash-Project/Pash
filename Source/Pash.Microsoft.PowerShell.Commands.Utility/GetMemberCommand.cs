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
        private HybridDictionary _membersCollection;

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
            _membersCollection = new HybridDictionary();
        }

        protected override void ProcessRecord()
        {
            // MUST: implement the InputObject binding in the pipe
            if ((InputObject == null) || (InputObject == AutomationNull.Value))
                return;
            
            string fullName;
            
            // TODO: deal with Static
            
            if (InputObject.TypeNames.Count != 0)
            {
                fullName = this.InputObject.TypeNames[0];
            }
            else
            {
                fullName = "<null>";
            }

            if (! _membersCollection.Contains(fullName))
            {
                _membersCollection.Add(fullName, "");

                foreach (string name in Name)
                {
                    ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos;

                    // TODO: deal with Static members
                    infos = InputObject.Members.Match(name, this.MemberType);
                    
                    List<MemberDefinition> members = new List<MemberDefinition>();
                    foreach (PSMemberInfo info in infos)
                    {
                        members.Add( new MemberDefinition(fullName, info.Name, info.MemberType, info.ToString()) );
                    }

                    members.Sort((def1, def2) =>
                    {
                        int diff =
                            string.Compare(def1.MemberType.ToString(), def2.MemberType.ToString(),
                                           StringComparison.CurrentCultureIgnoreCase);
                        if (diff != 0)
                        {
                            return diff;
                        }
                        return
                            string.Compare(def1.Name, def2.Name,
                                           StringComparison.CurrentCultureIgnoreCase);
                    });

                    foreach (MemberDefinition definition in members)
                    {
                        WriteObject(definition);
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            if (_membersCollection.Count == 0)
            {
                // TODO: WriteError
                throw new Exception("No object specified");
            }
        }
    }
}