// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Management.Automation
{
    public class ReadOnlyPSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable where T : PSMemberInfo
    {
        private IEnumerable<T> _members;
        public int Count {
            get
            {
                return _members.Count();
            }
        }

        public T this[int index]
        {
            get
            {
                return _members.ElementAt(index);
            }
        }

        public T this[string name] {
            get
            {
                return (from value in _members 
                        where String.Equals(value.Name, name, StringComparison.CurrentCultureIgnoreCase)
                        select value).FirstOrDefault() as T;
            }
        }

        public ReadOnlyPSMemberInfoCollection<T> Match(string name)
        {
            return Match(name, PSMemberTypes.All);
        }

        public ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
        {
            WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
            return new ReadOnlyPSMemberInfoCollection<T>(
                from value in _members
                where (value.MemberType & memberTypes) != 0 && pattern.IsMatch(value.Name)
                select value
                );
        }

        internal ReadOnlyPSMemberInfoCollection(IEnumerable<T> members)
        {
            _members = members;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
