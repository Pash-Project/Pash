using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Core.Extensibility;

[NUnitAddin]
public class NUnitAssertHandlerAddin : IAddin
{
    public bool Install(IExtensionHost host)
    {
        Debug.Listeners.Clear();
        Debug.Listeners.Add(new AssertFailTraceListener());
        Console.WriteLine("Addin: NUnitAssertHandlerAddin installed.");

        return true;
    }

    private class AssertFailTraceListener : DefaultTraceListener
    {
        public override void Fail(string message, string detailMessage)
        {
            Assert.Fail("Assertion failure: " + message);
        }

        public override void Fail(string message)
        {
            Assert.Fail("Assertion failure: " + message);
        }
    }
}