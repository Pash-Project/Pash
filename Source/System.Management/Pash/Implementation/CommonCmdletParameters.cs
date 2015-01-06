using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;

namespace Pash.Implementation
{
    static class CommonCmdletParameters
    {
        static readonly CmdletInfo commonCmdletInfo = new CmdletInfo("Common-Commands", typeof(CommonParametersCmdlet), null);

        static CommonCmdletParameters()
        {
            CommonParameterSetInfo = commonCmdletInfo.GetDefaultParameterSet();
        }

        internal static CommandParameterSetInfo CommonParameterSetInfo { get; private set; }

        internal static ReadOnlyCollection<CommandParameterSetInfo> AddCommonParameters(ReadOnlyCollection<CommandParameterSetInfo> parameterSets)
        {
            var updatedParameterSets = new List<CommandParameterSetInfo>();

            foreach (CommandParameterSetInfo parameterSet in parameterSets)
            {
                List<CommandParameterInfo> updatedParameters = parameterSet.Parameters.ToList();
                updatedParameters.AddRange(CommonParameterSetInfo.Parameters);

                updatedParameterSets.Add(new CommandParameterSetInfo(
                    parameterSet.Name,
                    parameterSet.IsDefault,
                    new Collection<CommandParameterInfo>(updatedParameters)));
            }

            return new ReadOnlyCollection<CommandParameterSetInfo>(updatedParameterSets);
        }
    }
}
