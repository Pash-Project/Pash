using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    public class ProviderInfo
    {
        public PSSnapInInfo PSSnapIn { get; private set; }
        public string Name { get; private set; }
        public string Description { get; set; }
        public string Home { get; set; }
        public Type ImplementingType { get; private set; }
        public ProviderCapabilities Capabilities { get; private set; }
        public string HelpFile { get; private set; }
        private SessionState _sessionState;

        public Collection<PSDriveInfo> Drives
        {
            get
            {
                return _sessionState.Drive.GetAllForProvider(FullName);
            }
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string helpFile, PSSnapInInfo psSnapIn)
            : this(sessionState, implementingType, name, string.Empty, string.Empty, helpFile, psSnapIn)
        {
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string description, string home, string helpFile, PSSnapInInfo psSnapIn)
        {
            _sessionState = sessionState;
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
                if (! string.IsNullOrEmpty(PSSnapInName))
                {
                    return string.Format(@"{0}\{1}", PSSnapInName, Name);
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
            catch(Exception ex)
            {
                outException = ex;
            }
            if (objProvider == null)
            {
                if (outException != null)
                {
                    throw new ProviderNotFoundException(Name, SessionStateCategory.CmdletProvider, "ProviderCtorException", outException.Message );
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
            return string.Equals(FullName, providerName, StringComparison.CurrentCultureIgnoreCase);
        }

        //internal bool IsMatch(WildcardPattern namePattern, PSSnapinQualifiedName psSnapinQualifiedName)
        //{
            
        //}
    }
}
