using System;
using System.Management.Automation;
using System.Collections;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet(VerbsCommon.Add, "Member", DefaultParameterSetName="TypeNameSet"
            /*HelpUri="http://go.microsoft.com/fwlink/?LinkID=113280",
                     RemotingCapability=RemotingCapability.None*/)] 
    public class AddMemberCommand : ObjectCommandBase
    {
        [Parameter(ParameterSetName="MemberSet")] 
        [Parameter(ParameterSetName="NotePropertySingleMemberSet")] 
        [Parameter(ParameterSetName="NotePropertyMultiMemberSet")] 
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="TypeNameSet")] 
        [Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="NotePropertyMultiMemberSet")] 
        [Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="MemberSet")] 
        [Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="NotePropertySingleMemberSet")] 
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="MemberSet")] 
        [Alias(new string[] { "Type" })]
        public PSMemberTypes MemberType { get; set; }

        [Parameter(Mandatory=true, Position=1, ParameterSetName="MemberSet")] 
        public string Name { get; set; }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="NotePropertyMultiMemberSet")] 
        [ValidateNotNullOrEmpty] 
        public IDictionary NotePropertyMembers { get; set; }

        [ValidateNotNullOrEmpty] 
        [Parameter(Mandatory=true, Position=0, ParameterSetName="NotePropertySingleMemberSet")] 
        public string NotePropertyName { get; set; }

        [Parameter(Mandatory=true, Position=1, ParameterSetName="NotePropertySingleMemberSet")] 
        [AllowNull] 
        public Object NotePropertyValue { get; set; }

        [Parameter(ParameterSetName="MemberSet")] 
        [Parameter(ParameterSetName="TypeNameSet")] 
        [Parameter(ParameterSetName="NotePropertySingleMemberSet")] 
        [Parameter(ParameterSetName="NotePropertyMultiMemberSet")] 
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position=3, ParameterSetName="MemberSet")] 
        public Object SecondValue { get; set; }

        [Parameter(ParameterSetName="NotePropertyMultiMemberSet")] 
        [ValidateNotNullOrEmpty] 
        [Parameter(ParameterSetName="NotePropertySingleMemberSet")] 
        [Parameter(Mandatory=true, ParameterSetName="TypeNameSet")] 
        [Parameter(ParameterSetName="MemberSet")] 
        public string TypeName { get; set; }

        [Parameter(Position=2, ParameterSetName="MemberSet")] 
        public Object Value { get; set; }

        protected override void ProcessRecord()
        {
            if (ParameterSetName.Equals("MemberSet"))
            {
                switch (MemberType)
                {
                    case PSMemberTypes.NoteProperty:
                        var member = new PSNoteProperty(Name, Value);
                        AddMemberToCollection(InputObject.Properties, member, Force.IsPresent);
                        AddMemberToCollection(InputObject.Members, member, Force.IsPresent);
                        break;

                    default:
                        var msg = String.Format("The member type '{0}' is currently not supported", MemberType.ToString());
                        throw new NotImplementedException(msg);
                }
            }
            else
            {
                var msg = String.Format("The parameter set '{0}' is currently not supported", ParameterSetName);
                throw new NotImplementedException(msg);
            }

            if (PassThru.IsPresent)
            {
                WriteObject(InputObject);
            }
        }       
    }
}

