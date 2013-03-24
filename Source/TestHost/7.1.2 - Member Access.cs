// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    [TestFixture]
    public class MemberAccess
    {
        [Test]
        public void InstanceMemberAccess()
        {
            var result = TestHost.Execute(true, @"'xxx'.Length");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        // Taken from the spec, section 7.1.2
        [Test, Explicit]
        public void InstanceProperty()
        {
            var result = TestHost.Execute(true, @"
$a = 10,20,30
a.Length
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        [Test, Explicit]
        public void InstancePropertyNameIsVariable()
        {
            var result = TestHost.Execute(true, @"
(10,20,30).Length
$property = ""Length""
$a.$property				# property name is a variable
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        [Test, Explicit]
        public void Hastable()
        {
            var result = TestHost.Execute(true, @"
                        $h1 = @{ FirstName = ""James""; LastName = ""Anderson""; IDNum = 123 }
                        $h1.FirstName						# designates the key FirstName
                        $h1.Keys								# gets the collection of keys
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        [Test]
        public void StaticProperty()
        {
            var result = TestHost.Execute(true, @"
[int]::MinValue					# get static property
");

            Assert.AreEqual(int.MinValue + Environment.NewLine, result);
        }

        [Test]
        public void StaticProperty2()
        {
            var result = TestHost.Execute(true, @"
[double]::PositiveInfinity		# get static property
");

            Assert.AreEqual(double.PositiveInfinity + Environment.NewLine, result);
        }

        [Test, Explicit]
        public void StaticPropertyNameIsAVariable()
        {
            var result = TestHost.Execute(true, @"
$property = ""MinValue""
[long]::$property					# property name is a variable
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        [Test, Explicit]
        public void StaticPropertyTypeVariable()
        {
            var result = TestHost.Execute(true, @"
foreach ($t in [byte],[int],[long])
{
    $t::MaxValue					# get static property
}
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        [Test, Explicit]
        public void FromArrayElement()
        {
            var result = TestHost.Execute(true, @"
$a = @{ID=1},@{ID=2},@{ID=3}
$a.ID									# get ID from each element in the array 
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }
    }
}
