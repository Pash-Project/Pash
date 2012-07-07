namespace System.Management.Automation
{
    public abstract class PSMemberInfo
    {
        protected PSMemberInfo()
        {
            IsInstance = true;
        }

        public bool IsInstance { get; internal set; }
        public string Name { get; internal set; }

        public abstract PSMemberTypes MemberType { get; }
        public abstract string TypeNameOfValue { get; }
        public abstract object Value { get; set; }

        public abstract PSMemberInfo Copy();

        // internals
        //internal void ReplicateInstance(PSObject particularInstance);
        //internal void SetValueNoConversion(object setValue);
        //internal bool IsHidden { get; }
        //internal bool IsReservedMember { get; }
        //internal bool ShouldSerialize { set; get; }
        internal PSObject _instance;
        //internal bool isHidden;
        //internal bool isInstance;
        //internal bool isReservedMember;
        //internal string name;
        //internal bool shouldSerialize;

        internal void CopyProperties(PSMemberInfo toObj)
        {
            toObj.Name = Name;
            toObj.IsInstance = IsInstance;
            toObj._instance = _instance;
        }
    }
}
