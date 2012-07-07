using System.Collections;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ValidateEnumeratedArgumentsAttribute : ValidateArgumentsAttribute
    {
        protected ValidateEnumeratedArgumentsAttribute()
        {
            
        }

        protected sealed override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            if (arguments == null)
            {
                throw new NullReferenceException("Validate arguments can't be null.");
            }

            if (arguments is IEnumerable)
            {
                ValidateElement(arguments);
            }
            else
            {
                foreach (object obj in (IEnumerable)arguments)
                {
                    ValidateElement(obj);
                }
            }
        }

        protected abstract void ValidateElement(object element);
    }
}