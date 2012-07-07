namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateRangeAttribute : ValidateEnumeratedArgumentsAttribute
    {
        public object MaxRange { get; private set; }
        public object MinRange { get; private set; }

        public ValidateRangeAttribute(object minRange, object maxRange)
        {
            // TODO: implement this
            MinRange = minRange;
            MaxRange = maxRange;
        }
        protected override void ValidateElement(object element)
        {
            // TODO: implement this
        }
    }
}