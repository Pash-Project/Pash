using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    internal class PSMemberInfoCollectionImplementation<T> : PSMemberInfoCollection<T>, IEnumerable<T>, IEnumerable where T : PSMemberInfo
    {
        private Collection<PSMemberInfo> _collection;
        private PSObject _owner;

        public PSMemberInfoCollectionImplementation(object owner)
        {
            // TODO: allow to provide an owner's reference Collection<PSMemberInfo> collection

            _owner = owner as PSObject;
            _collection = new Collection<PSMemberInfo>();
        }

        public override void Add(T member)
        {
            PSMemberInfo mbrInfo = member.Copy();
            _collection.Add(mbrInfo);
        }

        public override T this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        // MUST: implement this to do the PSObject ValueFromPipelineByPropertyName 
        public override ReadOnlyPSMemberInfoCollection<T> Match(string name)
        {
            throw new NotImplementedException();
        }

        // MUST: implement this to do the PSObject ValueFromPipelineByPropertyName 
        public override ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
        {
            throw new NotImplementedException();
        }

        public override void Remove(string name)
        {
            throw new NotImplementedException();
        }
    }
}