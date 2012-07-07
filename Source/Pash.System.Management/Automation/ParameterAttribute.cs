using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ParameterAttribute : ParsingBaseAttribute
    {
        public const string AllParameterSets = "__AllParameterSets";

        public ParameterAttribute() { }

        public string HelpMessage { get; set; }
        public string HelpMessageBaseName { get; set; }
        public string HelpMessageResourceId { get; set; }
        public bool Mandatory { get; set; }
        public string ParameterSetName { get; set; }
        public int Position { get; set; }
        public bool ValueFromPipeline { get; set; }
        public bool ValueFromPipelineByPropertyName { get; set; }
        public bool ValueFromRemainingArguments { get; set; }
    }
}
