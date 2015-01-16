// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    public class ProviderInfo : IComparable
    {
        public PSSnapInInfo PSSnapIn { get; private set; }
        public string Name { get; private set; }
        public string Description { get; set; }
        public string Home { get; set; }
        public Type ImplementingType { get; private set; }
        public ProviderCapabilities Capabilities { get; private set; }
        public string HelpFile { get; private set; }
        private SessionState _sessionState;
        public PSModuleInfo Module { get; private set; }
        public string ModuleName
        {
            get
            {
                return Module == null ? null : Module.Name;
            }
        }

        public Collection<PSDriveInfo> Drives
        {
            get
            {
                //TODO: only get the ones of the global scope to match PS 2.0 behavior
                return _sessionState.Drive.GetAllForProvider(Name);
            }
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string helpFile, PSSnapInInfo psSnapIn)
            : this(sessionState, implementingType, name, string.Empty, string.Empty, helpFile, psSnapIn)
        {
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string description,
                              string home, string helpFile, PSSnapInInfo psSnapIn)
            : this(sessionState, implementingType, name, description, home, helpFile, psSnapIn, null)
        {
        }

        // for all fields
        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string description, string home,
                              string helpFile, PSSnapInInfo psSnapIn, PSModuleInfo module)

        {
            _sessionState = sessionState;
            Module = module;
            PSSnapIn = psSnapIn;
            Name = name;
            Description = description;
            Home = home;
            ImplementingType = implementingType;
            Capabilities = GetCapabilities(implementingType);
            HelpFile = helpFile;
        }

        protected ProviderInfo(ProviderInfo providerInfo)
        {
            _sessionState = providerInfo._sessionState;
            PSSnapIn = providerInfo.PSSnapIn;
            Name = providerInfo.Name;
            Description = providerInfo.Description;
            Home = providerInfo.Home;
            ImplementingType = providerInfo.ImplementingType;
            Capabilities = providerInfo.Capabilities;
            HelpFile = providerInfo.HelpFile;
            Module = providerInfo.Module;
        }

        public override string ToString()
        {
            return FullName;
        }

        internal string PSSnapInName
        {
            get
            {
                if (PSSnapIn != null)
                {
                    return PSSnapIn.Name;
                }
                return null;
            }
        }

        internal string FullName
        {
            get
            {
                var moudleOrSnapinName = string.IsNullOrEmpty(PSSnapInName) ? ModuleName : PSSnapInName;
                if (!string.IsNullOrEmpty(moudleOrSnapinName))
                {
                    return string.Format(@"{0}\{1}", moudleOrSnapinName, Name);
                }
                return Name;
            }
        }

        internal CmdletProvider CreateInstance()
        {
            Exception outException = null;
            object objProvider = null;
            try
            {
                objProvider = Activator.CreateInstance(ImplementingType);
            }
            catch (Exception ex)
            {
                outException = ex;
            }
            if (objProvider == null)
            {
                if (outException != null)
                {
                    throw new ProviderNotFoundException(Name, SessionStateCategory.CmdletProvider, "ProviderCtorException", outException.Message);
                }
                else
                {
                    throw new ProviderNotFoundException(Name, SessionStateCategory.CmdletProvider, "ProviderNotFoundInAssembly");
                }
            }
            CmdletProvider provider = (CmdletProvider)objProvider;
            provider.SetProviderInfo(this);
            return provider;
        }

        public static ProviderCapabilities GetCapabilities(Type type)
        {
            try
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(CmdletProviderAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length == 1))
                {
                    CmdletProviderAttribute attribute = (CmdletProviderAttribute)customAttributes[0];
                    return attribute.ProviderCapabilities;
                }
            }
            catch
            {
                // TODO: what if the provider has no capabilities?
            }
            return ProviderCapabilities.None;
        }

        // internals
        internal bool IsNameMatch(string providerName)
        {
            if (string.Equals(FullName, providerName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return string.Equals(Name, providerName, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsAnyNameMatch(string[] providerNames)
        {
            foreach (var name in providerNames)
            {
                if (IsNameMatch(name))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {

            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            //toLower makes sure that two equal objects with same name but othe case-sensitivity return the same value
            return FullName.ToLower().GetHashCode();
        }

        #region IComparable members

        public int CompareTo(ProviderInfo provider)
        {
            if (!IsNameMatch(provider.FullName))
            {
                return provider.FullName.CompareTo(provider.FullName); //cannot be 0, otherwise it was a name match
            }
            return 0;
        }

        public int CompareTo(object obj)
        {
            ProviderInfo other = obj as ProviderInfo;
            if (other == null)
            {
                throw new PSInvalidOperationException("Can only compare to ProviderInfo!");
            }
            return CompareTo(other);
        }

        #endregion

        //internal bool IsMatch(WildcardPattern namePattern, PSSnapinQualifiedName psSnapinQualifiedName)
        //{

        //}
    }
}
