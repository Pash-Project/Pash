// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Management.Automation
{
    public abstract class PSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable where T : PSMemberInfo
    {
        internal PSMemberInfoCollection()
        {
        }

        public abstract T this[string name] { get; }

        public abstract void Add(T member);
        public abstract ReadOnlyPSMemberInfoCollection<T> Match(string name);
        public abstract ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes);
        public abstract void Remove(string name);

        #region IEnumerable<T> Members

        public abstract IEnumerator<T> GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
