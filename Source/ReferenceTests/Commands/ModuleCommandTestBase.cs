using System;
using TestPSSnapIn;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ReferenceTests
{
    public class ModuleCommandTestBase : ReferenceTestBase
    {
        public string CreateManifest(Dictionary<string, string> args )
        {
            return CreateManifest(null, null, null, null, null, null, null, null, args);
        }

        public string CreateManifest(string rootModule, string author, string company, string version,
            string funsExport = null, string varExport = null, string cmdletsExport = null, string aliasExport = null,
            Dictionary<string, string> args = null)
        {
            if (args == null)
            {
                args = new Dictionary<string, string>();
            }
            AddToDictIfNotExisting(args, "ModuleVersion", version);
            AddToDictIfNotExisting(args, "RootModule", rootModule);
            AddToDictIfNotExisting(args, "Author", author);
            AddToDictIfNotExisting(args, "CompanyName", company);
            AddToDictIfNotExisting(args, "FunctionsToExport", funsExport);
            AddToDictIfNotExisting(args, "CmdletsToExport", cmdletsExport);
            AddToDictIfNotExisting(args, "AliasesToExport", aliasExport);
            AddToDictIfNotExisting(args, "VariablesToExport", varExport);
            var sb = new StringBuilder(" @{ ");
            sb.AppendLine();
            foreach (var pair in args)
            {
                if (pair.Value == null)
                {
                    continue;
                }
                var strVal = pair.Value.StartsWith("@(") ? pair.Value : "'" + pair.Value + "'";
                sb.AppendFormat("{0} = {1}", pair.Key, strVal);
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void AddToDictIfNotExisting(Dictionary<string, string> dict, string key, string value)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                return;
            }
            dict[key] = value;
        }
    }
}

