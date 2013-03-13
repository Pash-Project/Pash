using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Tests to confirm our external dependencies are working properly. Make sure
// these pass before you investigate any other issues.
namespace Libraries.Tests
{
    [TestFixture]
    public class MonoTests
    {
        // This bug in Mono causes the parer to initialize incorrectly, making
        // the parser fail. Because it's a little subtle, I'm writing this unit
        // test to call it out.
        [Test]
        [Description("https://bugzilla.xamarin.com/show_bug.cgi?id=6541")]
        [Explicit("reenable when https://github.com/JayBazuzi/Pash2/issues/29 is closed.")]
        public void MonoOverloadBug6541Test()
        {
            F("x");
        }

        static void F(string s, params string[] strings)
        {
            Assert.Pass();
        }

        static void F(params string[] strings)
        {
            Assert.Fail("You are hitting Mono bug https://bugzilla.xamarin.com/show_bug.cgi?id=6541");
        }
    }
}
