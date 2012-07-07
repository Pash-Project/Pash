namespace System.Management.Automation.Internal
{
    internal interface ISupportShouldProcess
    {
        bool SupportsShouldProcess { get; }
    }
}