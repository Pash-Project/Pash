// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
