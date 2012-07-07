using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Management.Automation
{
    public class ReadOnlyPSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable where T : PSMemberInfo
    {
        //public int Count { get; }

        //public T this[int index] { get; }
        //public T this[string name] { get; }

        //public ReadOnlyPSMemberInfoCollection<T> Match(string name);
        //public ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes);

        //internal ReadOnlyPSMemberInfoCollection(PSMemberInfoInternalCollection<T> members);

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
