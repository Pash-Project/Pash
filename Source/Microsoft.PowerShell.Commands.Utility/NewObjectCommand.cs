// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet("New", "Object", DefaultParameterSetName = "Net")]
    public sealed class NewObjectCommand : ObjectCommandBase
    {
        [Parameter(ParameterSetName = "Net", Mandatory = false, Position = 1)]
        public object[] ArgumentList { get; set; }

        [Parameter(ParameterSetName = "Com", Mandatory = true, Position = 0)]
        public string ComObject { get; set; }

        [Parameter]
        public IDictionary Property { get; set; }

        [Parameter(ParameterSetName = "Com")]
        public SwitchParameter Strict { get; set; }

        [Parameter(ParameterSetName = "Net", Mandatory = true, Position = 0)]
        public string TypeName { get; set; }

        protected override void ProcessRecord()
        {
            Type type = new TypeName(this.TypeName).GetReflectionType();

            var result = PSObject.AsPSObject(Activator.CreateInstance(type, GetArguments()));

            if (Property != null)
            {
                AddProperties(result);
            }

            WriteObject(result);
        }

        private object[] GetArguments()
        {
            if (ArgumentList == null)
            {
                return new object[0];
            }

            return (from arg in ArgumentList
                    select PSObject.Unwrap(arg)).ToArray();
        }

        private void AddProperties(PSObject psobj)
        {
            foreach (var keyObj in Property.Keys)
            {
                var key = keyObj.ToString(); // should be a string anyway
                var member = psobj.Members[key];
                if (member == null)
                {
                    if (psobj.BaseObject is PSCustomObject)
                    {
                        var noteProperty = new PSNoteProperty(key, Property[key]);
                        AddMemberToCollection(psobj.Properties, noteProperty, false);
                        AddMemberToCollection(psobj.Members, noteProperty, false);
                    }
                    else
                    {
                        var msg = String.Format("A member with the name {0} doesn't exist", key);
                        WriteError(new PSInvalidOperationException(msg).ErrorRecord);
                    }
                }
                else if (member is PSMethodInfo)
                {
                    var method = member as PSMethodInfo;
                    method.Invoke(Property[key]);
                }
                else if (member is PSPropertyInfo)
                {
                    var psproperty = member as PSPropertyInfo;
                    psproperty.Value = Property[key];
                }
            }
        }
    }
}
