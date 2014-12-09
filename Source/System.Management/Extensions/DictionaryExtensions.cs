using System;
using System.Collections.Generic;

namespace Extensions.Dictionary
{
    static class _
    {
        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                return;
            }
            foreach (var item in collection)
            {
                source[item.Key] = item.Value;
            } 
        }

        public static void ReplaceContents<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            source.Clear();
            source.AddRange(collection);
        }
    }
}

