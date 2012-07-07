namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateNotNullAttribute : ValidateArgumentsAttribute
    {
        public ValidateNotNullAttribute()
        {
            
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            throw new NotImplementedException();
        }
    }
}