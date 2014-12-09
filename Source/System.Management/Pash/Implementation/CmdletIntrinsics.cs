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

        public void Set(CmdletInfo cmdlet)
        {
            _scope.SetLocal(cmdlet, false);
        }

        public void Remove(string name)
        {
            _scope.Remove(name, false);
        }

        public Dictionary<string, CmdletInfo> GetAllLocal()
        {
            return new Dictionary<string, CmdletInfo>(_scope.Items);
        }

        public Dictionary<string, CmdletInfo> Find(string name)
        {
            return _scope.Find(name, false);
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
                curCmdlet.AddCommonParameters();
                _scope.SetLocal(curCmdlet, false);
            }
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

