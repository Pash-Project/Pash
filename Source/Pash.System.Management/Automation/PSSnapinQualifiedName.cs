namespace System.Management.Automation
{
    internal class PSSnapinQualifiedName
    {
        internal string ShortName { get; private set; }
        internal string FullName { get; private set; }
        internal string PSSnapInName { get; private set; }

        private PSSnapinQualifiedName(string name)
        {
            string[] parts = name.Split('\\');
            if ((parts.Length < 1) && (parts.Length > 2))
            {
                throw new Exception("Invalid PSSnapin name format");
            }
            if (parts.Length == 1)
            {
                ShortName = parts[0];
            }
            else
            {
                if (!string.IsNullOrEmpty(parts[0]))
                {
                    PSSnapInName = parts[0];
                }
                ShortName = parts[1];
            }
            if (string.IsNullOrEmpty(PSSnapInName))
            {
                FullName = ShortName;
            }
            else
            {
                FullName = string.Format(@"{0}\{1}", PSSnapInName, ShortName);
            }

        }

        public override string ToString()
        {
            return FullName;
        }
    }
}