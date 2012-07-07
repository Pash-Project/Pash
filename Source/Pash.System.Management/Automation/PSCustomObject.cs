namespace System.Management.Automation
{
    public class PSCustomObject
    {
        internal static PSCustomObject Instance;

        static PSCustomObject()
        {
            Instance = new PSCustomObject();
        }

        private PSCustomObject()
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}