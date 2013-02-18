using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Extensions.Reflection
{
    static class _
    {
        public static T GetValue<T>(this FieldInfo fieldInfo, object obj = null)
        {
            return (T)fieldInfo.GetValue(obj);
        }

        public static bool IsAssignableFrom<T>(this Type @this, T t)
        {
            return @this.IsAssignableFrom(t.GetType());
        }
    }
}
