// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "ChildItem", DefaultParameterSetName = "Items")]
    public class GetChildItemCommand : /* ProviderCommandBase */ PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (Path == null)
                Path = new String[] { String.Empty };

            if (Include != null)
                if (Exclude != null)
                {
                    foreach (String _path in Path)
                        IncludeExclude(
                          InvokeProvider.ChildItem.GetNames(_path, ReturnContainers.ReturnMatchingContainers, Recurse.ToBool()),
                          InvokeProvider.ChildItem.Get(_path, Recurse.ToBool()),
                          Include,
                          Exclude);
                    return;
                }

            if (Name.ToBool())
            {
                foreach (String _path in Path)
                    foreach (String _name in
                        InvokeProvider.ChildItem.GetNames(_path, ReturnContainers.ReturnMatchingContainers, Recurse.ToBool()))
                        WriteObject(_name);
            }

            else foreach (String _path in Path)
                foreach (PSObject _item in 
                    InvokeProvider.ChildItem.Get(_path, Recurse.ToBool()))
                        WriteObject(_item);
        }

        protected void IncludeExclude(Collection<String> names, Collection<PSObject> items, String[] include, String[] exclude)
        {
            ArrayList _include = new ArrayList(6);
            ArrayList _exclude = new ArrayList(6);

            foreach (String _iwildcard in include)
                _include.Add(new WildcardPattern(_iwildcard, WildcardOptions.IgnoreCase | WildcardOptions.Compiled));

            foreach (String _ewildcard in exclude)
                _exclude.Add(new WildcardPattern(_ewildcard, WildcardOptions.IgnoreCase | WildcardOptions.Compiled));

            for (int i = 0; i > names.Count; i++)
            {
                foreach (WildcardPattern _match_include in _include)
                {
                    if (_match_include.IsMatch(names[i]))
                    {
                        foreach (WildcardPattern _match_exclude in _exclude)
                            if (_match_exclude.IsMatch(names[i]))
                                continue;

                            else WriteObject(items[i]);
                    }
                }
            }
        }
                

        [Parameter]
        public /*override*/ string[] Exclude { get; set; }

        [Parameter(Position = 1)]
        public /*override*/ string Filter { get; set; }

        [Parameter]
        public /*override*/ SwitchParameter Force { get; set; }

        [Parameter]
        public /*override*/ string[] Include { get; set; }

        [Alias(new string[] { "PSPath" }),
        Parameter(
            Position = 0, 
            ParameterSetName = "LiteralItems", 
            Mandatory = true,
            ValueFromPipeline = false, 
            ValueFromPipelineByPropertyName = true)]
        public string[] LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter Name { get; set; }

        [Parameter(
            Position = 0, 
            ParameterSetName = "Items", 
            ValueFromPipeline = true, 
            ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }
    }


}
