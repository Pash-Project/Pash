using System;
using System.Collections.Generic;

namespace Extensions.Queue
{
    static class _
    {
        public static void EnqueueAll<T>(this Queue<T> @this, IEnumerable<T> items)
        {
            foreach (var item in items) @this.Enqueue(item);
        }
    }
}
