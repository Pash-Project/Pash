using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class MemberDefinition
    {
        public string TypeName { get; private set; }
        public string Name { get; private set; }
        public PSMemberTypes MemberType { get; private set; }
        public string Definition { get; private set; }

        public MemberDefinition(string typeName, string name, PSMemberTypes memberType, string definition)
        {
            TypeName = typeName;
            Name = name;
            MemberType = memberType;
            Definition = definition;
        }

        public override string ToString()
        {
            return Definition;
        }
    }
}