using System;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal interface IScopedItem
    {
        string ItemName { get; }
        ScopedItemOptions ItemOptions { get; set; }
    }
}

