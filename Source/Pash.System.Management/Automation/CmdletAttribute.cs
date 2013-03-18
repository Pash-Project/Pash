﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CmdletAttribute : CmdletMetadataAttribute
    {
        public CmdletAttribute(string verbName, string nounName)
        {
            VerbName = verbName;
            NounName = nounName;
        }

        public ConfirmImpact ConfirmImpact { get; set; }
        public string DefaultParameterSetName { get; set; }
        public string NounName { get; private set; }
        public bool SupportsShouldProcess { get; set; }
        public string VerbName { get; private set; }

        internal string FullName
        {
            get
            {
                return
                    string.Format("{0}-{1}", this.VerbName, this.NounName);
            }
        }

        public override string ToString()
        {
            return this.FullName;
        }
    }
}
