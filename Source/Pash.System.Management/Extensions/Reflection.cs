using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Extensions.Reflection
{
    static class Extensions
    {
        public static T GetValue<T>(this FieldInfo fieldInfo, object obj = null) { return (T)fieldInfo.GetValue(obj); }
    }
}
