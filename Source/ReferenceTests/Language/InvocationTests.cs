using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    class InvocationTests : ReferenceTestBase
    {
        /// <summary>
        /// This was throwing an index out of range exception.
        /// </summary>
        [Test]
        public void InvokeGetMethodsWithOneParameter()
        {
            string result = ReferenceHost.Execute(
@"$type = [System.Object]
$type.GetMethods(20) | Foreach-Object { $_.name }"
);

            StringAssert.Contains("GetHashCode" + Environment.NewLine, result);
        }
    }
}
