using System;
using System.Management.Automation;

namespace ReferenceTests
{
    public class ReferenceTestBase
    {

        public static string CmdletName(Type cmdletType)
        {
            var attribute = System.Attribute.GetCustomAttribute(cmdletType, typeof(CmdletAttribute))
                            as CmdletAttribute;
            return string.Format("{0}-{1}", attribute.VerbName, attribute.NounName);
        }

        public static string OutputString(string[] parts)
        {
            return String.Join(Environment.NewLine, parts) + Environment.NewLine;
        }
    }
}

