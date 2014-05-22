// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Management.Automation
{
    internal class PSMemberInfoCollectionImplementation<T> : PSMemberInfoCollection<T>, IEnumerable<T>, IEnumerable where T : PSMemberInfo
    {
        private Collection<T> _collection;
        private PSObject _owner;

        public PSMemberInfoCollectionImplementation(object owner)
        {
            // TODO: allow to provide an owner's reference Collection<PSMemberInfo> collection

            _owner = owner as PSObject;
            _collection = new Collection<T>();
        }

        public override void Add(T member)
        {
            _collection.Add(member);
        }

        public override T this[string name]
        {
            get
            {
                return ( from info in _collection
                         where String.Equals(info.Name, name, StringComparison.CurrentCultureIgnoreCase)
                         select info
                ).FirstOrDefault() as T;
            }
        }

        public override ReadOnlyPSMemberInfoCollection<T> Match(string name)
        {
            return Match(name, PSMemberTypes.All);
        }

        public override ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
        {
            WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
            return new ReadOnlyPSMemberInfoCollection<T>(
                from value in _collection
                where (value.MemberType & memberTypes) != 0 && pattern.IsMatch(value.Name)
                select value
            );
        }

        public override void Remove(string name)
        {
            var item = this[name];
            _collection.Remove(item);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
