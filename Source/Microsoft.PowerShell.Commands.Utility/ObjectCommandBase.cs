using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    public class ObjectCommandBase : PSCmdlet
    {
        
        protected void AddMemberToCollection<T>(PSMemberInfoCollection<T> collection, T member, bool force) where T : PSMemberInfo
        {
            var existingValue = collection[member.Name];
            if (existingValue != null)
            {
                if (force)
                {
                    collection.Remove(member.Name);
                }
                else
                {
                    var msg = String.Format("Member '{0}' already exists. Use force to overwrite.", member.Name);
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(msg), "MemberAlreadyExists", 
                                                          ErrorCategory.InvalidArgument, member));
                }
            }
            collection.Add(member);
        }
    }
}

