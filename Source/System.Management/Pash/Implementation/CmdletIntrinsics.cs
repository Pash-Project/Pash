using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Collections.Generic;

namespace Pash.Implementation
{
    public class CmdletIntrinsics
    {
        private SessionStateScope<CmdletInfo> _scope;

        internal CmdletIntrinsics(SessionStateScope<CmdletInfo> scope)
        {
            _scope = scope;
        }

        public CmdletInfo Get(string name)
        {
            return _scope.Get(name, false);
        }

        public Dictionary<string, CmdletInfo> Find(string name)
        {
            return _scope.Find(name, false);
        }

        public void LoadCmdletsFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo)
        {
            var cmdlets = from Type type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(Cmdlet))
                    from CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof(CmdletAttribute), true)
                    select new CmdletInfo(cmdletAttribute.FullName, type, null, snapinInfo);
            foreach (CmdletInfo curCmdlet in cmdlets)
            {
                curCmdlet.AddCommonParameters();
                _scope.SetLocal(curCmdlet, false);
            }
        }

        public void LoadCmdletsFromAssembly(Assembly assembly, PSModuleInfo moduleInfo)
        {
            throw new NotImplementedException("loading for modules not yet implemented");
        }

        public void RemoveAll(PSSnapInInfo snapin)
        {
            foreach (var pair in _scope.GetAll())
            {
                var cmdletSnapin = pair.Value.PSSnapIn;
                // check if loaded by this snapin and remove if it is
                if (cmdletSnapin != null && cmdletSnapin.Equals(snapin))
                {
                    _scope.Remove(pair.Key, false);
                }
            }
        }
    }
}

