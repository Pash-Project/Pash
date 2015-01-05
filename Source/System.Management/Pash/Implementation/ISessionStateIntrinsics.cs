using System;
using System.Collections.Generic;

namespace Pash.Implementation
{
    internal interface ISessionStateIntrinsics<T> where T : IScopedItem
    {
        bool SupportsScopedName { get; }

        T Get(string name);

        Dictionary<string, T> GetAll();

        T GetAtScope(string name, string scope);

        Dictionary<string, T> GetAllLocal();

        Dictionary<string, T> Find(string name);
    }

}

