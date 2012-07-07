using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class PSMemberSet : PSMemberInfo
    {
        public bool InheritMembers { get; private set; }
        public PSMemberInfoCollection<PSMemberInfo> Members { get; private set; }
        public override PSMemberTypes MemberType { get { return PSMemberTypes.MemberSet; } }
        public PSMemberInfoCollection<PSMethodInfo> Methods { get; private set; }
        public PSMemberInfoCollection<PSPropertyInfo> Properties { get; private set; }

        public override object Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new InvalidOperationException("Can't change this value.");
            }
        }

        internal bool inheritMembers;
        internal PSMemberInfoCollectionImplementation<PSMemberInfo> internalMembers;

        public PSMemberSet(string name)
        {
            Name = name;
            InheritMembers = true;
            Members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            Methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            Properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
        }

        public PSMemberSet(string name, IEnumerable<PSMemberInfo> members)
            : this(name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Name can't be empty");

            if (members == null)
                throw new NullReferenceException("Members collection can't be null");
        }

        internal PSMemberSet(string name, PSObject obj)
            : this(name)
        {
            
        }

        public override PSMemberInfo Copy()
        {
            PSMemberSet outVal = new PSMemberSet(Name);
            foreach (PSMemberInfo member in Members)
            {
                outVal.Members.Add(member);
            }
            CopyProperties(outVal);
            return outVal;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(" {");

            foreach (PSMemberInfo info in Members)
            {
                builder.Append(info.Name);
                builder.Append(", ");
            }

            // remove the last coma
            if (builder.Length > 2)
            {
                builder.Remove(builder.Length - 2, 2);
            }

            builder.Insert(0, Name);
            builder.Append("}");

            return builder.ToString();

        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSMemberSet).FullName;
            }
        }
    }
}