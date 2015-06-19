using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;

namespace Pash.Implementation
{
    internal class CommonCmdletParameters
    {
        private static Boolean _initializing = false;
        private static CommonCmdletParameters _instance;

        readonly CmdletInfo _commonCmdletInfo;
        internal CommandParameterSetInfo CommonParameterSetInfo { get; private set; }

        private CommonCmdletParameters()
        {
            _initializing = true;
            _commonCmdletInfo = new CmdletInfo("Common-Commands", typeof(CommonParametersCmdlet), null);
            CommonParameterSetInfo = _commonCmdletInfo.GetDefaultParameterSet();
        }

        /// <summary>
        /// Singleton access to the class. Might return null in case of initialization loop.
        /// </summary>
        internal static CommonCmdletParameters instance()
        {
            if (_instance != null)
            {
                return _instance;
            }
            // Here's a little tricky part. AddCommonParameters is called when initializing a CmdletInfo.
            // Since this function depends on a static CmdletInfo object about the fake cmdlet with the common
            // parameters, we have some kind of loop here.
            // To avoid looping, _initalizing is set to true in the constructor as the very first operation.
            // Doing this we can break the loop
            if (_initializing)
            {
                return null;
            }
            _instance = new CommonCmdletParameters();
            return _instance;
        }

        internal ReadOnlyCollection<CommandParameterSetInfo> AddCommonParameters(ReadOnlyCollection<CommandParameterSetInfo> parameterSets)
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
