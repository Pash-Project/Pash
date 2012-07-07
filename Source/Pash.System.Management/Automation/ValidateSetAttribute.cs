using System.Collections.Generic;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateSetAttribute : ValidateEnumeratedArgumentsAttribute
    {
        public bool IgnoreCase { get; set; }
        public IList<string> ValidValues { get; private set; }

        public ValidateSetAttribute(params string[] validValues)
        {
            // TODO: validate input
            ValidValues = validValues;
        }

        protected override void ValidateElement(object element)
        {
            throw new NotImplementedException();
        }
    }
}