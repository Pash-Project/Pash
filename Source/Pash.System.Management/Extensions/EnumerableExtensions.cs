using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extensions.Enumerable
{
    static class _
    {
        internal static IEnumerable<T> Generate<T>(this T start, Func<T, T> next, T end)
            where T : struct
        {
            for (var item = start; !object.Equals(item, end); item = next(item))
            {
                yield return item;
            }

            yield return end;
        }

        internal static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this) action(item);
        }
    }
}
