using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Collections.Generic;

namespace Pash.Implementation
{
    internal class CmdletIntrinsics : SessionStateIntrinsics<CmdletInfo>
    {
        internal CmdletIntrinsics(SessionStateScope<CmdletInfo> scope) : base(scope, false)
        {
        }

        public void Set(CmdletInfo cmdlet)
        {
            // manually remove the cmdlet if already imported, because the normale "overwrite" flag doesn't work,
            // as the CmdletInfo doesn't support ItemOptions
            if (Scope.HasLocal(cmdlet))
            {
                Scope.RemoveLocal(cmdlet.ItemName);
            }
            Scope.SetLocal(cmdlet, false);
        }

        public void Remove(string name)
        {
            Scope.Remove(name, false);
        }

        public void LoadCmdletsFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo)
        {
            LoadCmdletsFromAssembly(assembly, snapinInfo, null);
        }

        public void LoadCmdletsFromAssembly(Assembly assembly, PSModuleInfo moduleInfo)
        {
            LoadCmdletsFromAssembly(assembly, null, moduleInfo);
        }

        // private because one should only load cmdlets from a module OR snapin, not both. but it's handy
        private void LoadCmdletsFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo, PSModuleInfo moduleInfo)
        {
            var cmdlets = from Type type in assembly.GetTypes()
                    where type.IsSubclassOf(typeof(Cmdlet))
                    from CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof(CmdletAttribute), true)
                    select new CmdletInfo(cmdletAttribute.FullName, type, null, snapinInfo, moduleInfo);
            foreach (CmdletInfo curCmdlet in cmdlets)
            {
                Scope.SetLocal(curCmdlet, false);
            }
        }

        public void RemoveAll(PSSnapInInfo snapin)
        {
            foreach (var pair in Scope.GetAll())
            {
                var cmdletSnapin = pair.Value.PSSnapIn;
                // check if loaded by this snapin and remove if it is
                if (cmdletSnapin != null && cmdletSnapin.Equals(snapin))
                {
                    Scope.Remove(pair.Key, false);
                }
            }
        }
    }
}

