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

        [Test]
        public void MemberCaseInsensitivy()
        {
            var result = TestHost.Execute(true, @"'xxx'.LENGTH");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        // Taken from the spec, section 7.1.2
        [Test]
        public void InstanceProperty()
        {
            var result = TestHost.Execute(true, @"
$a = 10,20,30
$a.Length
");

            Assert.AreEqual("3" + Environment.NewLine, result);
        }

        [Test]
        public void StaticMethodOnSystemType()
        {
            var result = TestHost.Execute(true, @"[System.Int32]::Parse('7')");

            Assert.AreEqual("7" + Environment.NewLine, result);
        }

        [Test]
        public void StaticMethodOnBuiltinType()
        {
            var result = TestHost.Execute(true, @"[int]::Parse('7')");

            Assert.AreEqual("7" + Environment.NewLine, result);
        }

        [Test]
        public void StaticMethodAccessIsCaseInsensitive()
        {
            var result = TestHost.Execute(true, @"[Math]::abs(-15)");

            Assert.AreEqual("15" + Environment.NewLine, result);
        }

        [Test]
        public void StaticMethodCalledOnTypeReferencedByVariable()
        {
            var result = TestHost.Execute(true, @"
$path = [System.IO.Path]
$path::GetExtension('test.txt')
");

            Assert.AreEqual(".txt" + Environment.NewLine, result);
        }

        [Test]
        public void TwoParameters()
        {
            var result = TestHost.Execute(@"[char]::IsUpper(""AbC"", 1)");
            Assert.AreEqual("False" + Environment.NewLine, result);
            var result2 = TestHost.Execute(@"[char]::IsUpper(""AbC"", 2)");
            Assert.AreEqual("True" + Environment.NewLine, result2);
        }

        [Test]
        public void NoParametersTest()
        {
            var result = TestHost.Execute(@"'a'.GetType().Name");
            Assert.AreEqual(typeof(System.String).Name + Environment.NewLine, result);
        }

        [Test]
        [TestCase(@"[math]::Sqrt(2.0)				# call method with argument 2.0")]
        [TestCase(@"[char]::IsUpper(""a"")			# call method")]
        [TestCase(//@"$b = ""abc#$%XYZabc""",       // This hits some issues in the tokenizer.
                  @"$b = ""abcXYZabc""",            // punting for now
                  @"$b.ToUpper()					# call instance method")]
        [TestCase(@"[math]::Sqrt(2) 				# convert 2 to 2.0 and call method")]
        [TestCase(@"[math]::Sqrt(2D) 				# convert 2D to 2.0 and call method")]
        [TestCase(@"[math]::Sqrt($true) 			# convert $true to 1.0 and call method")]
        [TestCase(@"[math]::Sqrt(""20"") 			# convert ""20"" to 20 and call method")]
        [TestCase(@"$a = [math]::Sqrt				# get method descriptor for Sqrt
                    $a.Invoke(2.0)					# call Sqrt via the descriptor
                    $a = [math]::(""Sq""+""rt"")	# get method descriptor for Sqrt
                    $a.Invoke(2.0) 					# call Sqrt via the descriptor")]
        [TestCase(@"$a = [char]::ToLower			# get method descriptor for ToLower
                    $a.Invoke(""X"")				# call ToLower via the descriptor")]
        public void Section7_1_3_InvocationExpressions(params string[] input)
        {
            Assert.DoesNotThrow(delegate() {
                TestHost.Execute(input);
            });
        }

        [Test]
        public void InstancePropertyNameIsVariable()
        {
            var result = TestHost.Execute(true, @"
(10,20,30).Length
$property = ""Length""
$a.$property				# property name is a variable
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

        [Test]
        public void StaticPropertyNameIsAVariable()
        {
            var result = TestHost.Execute(true, @"
$property = ""MinValue""
[long]::$property					# property name is a variable
");
            Assert.AreEqual(long.MinValue + Environment.NewLine, result);
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

        [Test]
        public void StaticPropertyOnTypeReferencedByVariable()
        {
            var result = TestHost.Execute(true, @"
$test = [Environment]
$test::Version.ToString()
");

            Assert.AreEqual(Environment.Version.ToString() + Environment.NewLine, result);
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

        [Test]
        public void FullNamePropertyOnType()
        {
            string result = TestHost.Execute("'abc'.GetType().FullName");

            Assert.AreEqual("System.String" + Environment.NewLine, result);
        }

        [Test]
        public void CallGetTypeOnType()
        {
            string result = TestHost.Execute("'abc'.GetType().GetType().FullName");

            Assert.AreEqual(typeof(System.Environment).GetType().FullName + Environment.NewLine, result);
        }

        [Test]
        public void TypeFullNamePropertyOnSystemEnvironment()
        {
            string result = TestHost.Execute("[Environment].FullName");

            Assert.AreEqual("System.Environment" + Environment.NewLine, result);
        }

        [Test]
        public void CallGetTypeMethodOnSystemEnvironment()
        {
            string result = TestHost.Execute("[Environment].GetType().FullName");

            Assert.AreEqual(typeof(System.Environment).GetType().FullName + Environment.NewLine, result);
        }

        [Test]
        public void TypeFullNameOnTypeReferencedByVariable()
        {
            var result = TestHost.Execute(true, @"
$path = [System.IO.Path]
$path.FullName
");

            Assert.AreEqual("System.IO.Path" + Environment.NewLine, result);
        }

        [Test]
        public void CallGetTypeOnTypeReferencedByVariable()
        {
            var result = TestHost.Execute(true, @"
$path = [System.IO.Path]
$path.GetType().FullName
");
            Assert.AreEqual(typeof(System.Environment).GetType().FullName + Environment.NewLine, result);
        }

        public class XmlMemberAccess
        {
            [Test]
            public void CanGetValueFromRootXmlElement()
            {
                var result = TestHost.Execute(true, @"
$path = [xml]""<a>hello a</a>""
$path.a
");
                Assert.AreEqual("hello a" + Environment.NewLine, result);
            }

            [Test]
            public void CanGetValueFromChildXmlElement()
            {
                var result = TestHost.Execute(true, @"
$path = [xml]""<a><b>hello a</b></a>""
$path.a.b
");
                Assert.AreEqual("hello a" + Environment.NewLine, result);
            }

            [Test]
            public void ArrayOfTextNodeElement()
            {
                var result = TestHost.Execute(true, @"
$path = [xml]""<a><b>hello a</b><b>hello a</b></a>""
$path.a.b
");
                Assert.AreEqual("hello a" + Environment.NewLine + "hello a" + Environment.NewLine, result);
            }

            [Test]
            public void MissingXmlProperty()
            {
                var result = TestHost.Execute(true, @"
$path = [xml]""<a>hello a</a>""
$path.z
");
                Assert.AreEqual(String.Empty, result);
            }

        }
    }
}
