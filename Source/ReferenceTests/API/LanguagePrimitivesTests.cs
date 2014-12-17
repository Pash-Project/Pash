using NUnit.Framework;
using System;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace ReferenceTests.API
{
    public class TestParent {
        private string _msg;
        public TestParent(string msg) { _msg = msg; }
        public override int GetHashCode() { return _msg.ToLower().GetHashCode(); }
        public override bool Equals(object obj)
        {
            var otherParent = obj as TestParent;
            if (otherParent == null)
            {
                throw new InvalidOperationException("Wrong object type to compare to");
            }
            return _msg.Equals(otherParent._msg);
        }
        public bool EqualsIgnoreCase(TestParent other)
        {
            return String.Compare(_msg, other._msg, true) == 0;
        }
    }

    public class TestChild : TestParent {
        private string _msg2;
        public TestChild(string msg, string msg2) : base(msg) { _msg2 = msg2; }
        public override int GetHashCode() { return _msg2.GetHashCode() + base.GetHashCode(); }
        public override bool Equals(object obj)
        {
            var otherChild = obj as TestChild;
            if (otherChild == null)
            {
                throw new InvalidOperationException("Wrong object type to compare to");
            }
            return _msg2.Equals(otherChild._msg2) && base.EqualsIgnoreCase(otherChild);
        }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    public class Custom
    {
        public string Id { get; set; }
    }

    public class CustomTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            return new Custom { Id = stringValue };
        }
    }

    [TestFixture]
    public class LanguagePrimitivesTests
    {

        [TestCase(3, typeof(int), (int) 3)]
        [TestCase(3, typeof(double), (double) 3)]
        [TestCase(3, typeof(string), "3")]
        [TestCase(3, typeof(int[]), new int[] { (int) 3 })]
        [TestCase(3, typeof(double[]), new double[] { (double) 3.0 })]
        [TestCase(3, typeof(string[]), new string[] { "3" })]
        [TestCase("3", typeof(string), "3")]
        [TestCase("3", typeof(int), (int) 3)]
        [TestCase("3", typeof(double), (double) 3.0)]
        [TestCase("3", typeof(string[]), new string[] { "3" })]
        [TestCase("3", typeof(int[]), new int[] { (int) 3 })]
        [TestCase("3", typeof(double[]), new double[] { (double) 3.0 })]
        public void ConvertToWorksWithBasicsAndArrayPacking(object obj, Type type, object expected)
        {
            var result = LanguagePrimitives.ConvertTo(obj, type);
            Assert.AreEqual(type, result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToThrowsExceptionIfStringConversionFails()
        {
            Assert.Throws(
                typeof(PSInvalidCastException),
                delegate() {
                    LanguagePrimitives.ConvertTo("3foo", typeof(int));
                }
            );
        }

        [Test]
        public void ConvertToThrowsExceptionIfNotPossibleAtAll()
        {
            Assert.Throws(
                typeof(PSInvalidCastException),
                delegate() {
                    LanguagePrimitives.ConvertTo(new TestParent("foo"), typeof(int));
                }
            );
        }

        [Test]
        public void ConvertToThrowsExceptionIfNotPossibleToUpcast()
        {
            Assert.Throws(
                typeof(PSInvalidCastException),
                delegate() {
                    LanguagePrimitives.ConvertTo(new TestParent("foo"), typeof(TestChild));
                }
            );
        }

        [Test]
        public void ConvertToThrowsExceptionAndShowsTypeBeingConvertedToInErrorMessage()
        {
            Exception ex = Assert.Throws(
                typeof(PSInvalidCastException),
                delegate()
                {
                    LanguagePrimitives.ConvertTo("foo", typeof(TestChild));
                }
            );

            Assert.That(ex.Message, Contains.Substring(typeof(TestChild).FullName));
        }

        [Test]
        public void ConvertToWorksForUpcast()
        {
            var result = LanguagePrimitives.ConvertTo((TestParent) new TestChild("foo", "bar"), typeof(TestChild));
            var expected = new TestChild("FOO", "bar");
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToWorksForDowncast()
        {
            var result = LanguagePrimitives.ConvertTo(new TestChild("foo", "bar"), typeof(TestParent));
            var expected = new TestParent("foo");
            Assert.AreEqual(expected, result);
        }


        [TestCase(false, false)]
        [TestCase(null, false)]
        [TestCase(true, true)]
        public void ConvertToCanHandleSwitchParameters(object value, bool expectedValue)
        {
            var result = LanguagePrimitives.ConvertTo(value, typeof(SwitchParameter));
            var expected = new SwitchParameter(expectedValue);
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected.IsPresent, ((SwitchParameter)result).IsPresent);
        }

        [TestCase(3)]
        [TestCase(0.0)]
        [TestCase(0.0)]
        [TestCase(0)]
        [TestCase(-1.0)]
        public void ConvertToDoesntConvertFromNumericToSwitchParameter(object value)
        {
            Assert.Throws<PSInvalidCastException>(delegate
            {
                LanguagePrimitives.ConvertTo(value, typeof(SwitchParameter));
            });
        }

        [Test]
        public void ConvertToCanPackAsPSObject()
        {
            var result = LanguagePrimitives.ConvertTo(3, typeof(PSObject));
            var expected = PSObject.AsPSObject(3);
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToCanPackAsPSObjectArray()
        {
            var result = LanguagePrimitives.ConvertTo(3, typeof(PSObject[]));
            var expected = new PSObject[] { PSObject.AsPSObject(3) };
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToConvertsArrayToPSObjectArray()
        {
            var input = new int[] { 3, 4, 5 };
            var expected = new PSObject[] { PSObject.AsPSObject(3), PSObject.AsPSObject(4), PSObject.AsPSObject(5) };
            var result = LanguagePrimitives.ConvertTo(input, typeof(PSObject[]));
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToConvertsArraysRecursivelyNumbers()
        {
            var input = new int[][] { new int[] { 3, 4 }, new int[] { 5, 6 } };
            var expected = new double[][] { new double[] { 3.0, 4.0 }, new double[] { 5.0, 6.0 } };
            var result = LanguagePrimitives.ConvertTo(input, typeof(double[][]));
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToConvertsArraysRecursivelyStrings()
        {
            var input = new int[][] { new int[] { 3, 4 }, new int[] { 5, 6 } };
            var expected = new string[][] { new string[] { "3", "4" }, new string[] { "5", "6" } };
            var result = LanguagePrimitives.ConvertTo(input, typeof(string[][]));
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToConvertsEnumerableToArray()
        {
            var input = new Collection<int> { 3, 4, 5 };
            var expected = new string[] { "3", "4", "5" };
            var result = LanguagePrimitives.ConvertTo(input, typeof(string[]));
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertToConvertsTypeWithTypeConverter()
        {
            string input = "MyId";
            var result = LanguagePrimitives.ConvertTo(input, typeof(Custom));
            Assert.AreEqual(typeof(Custom), result.GetType());
            Assert.AreEqual("MyId", ((Custom)result).Id);
        }

        [Test]
        public void ConvertUsingConstructor()
        {
            string input = "1.0";
            var result = LanguagePrimitives.ConvertTo(input, typeof(Version));
            Assert.That(result, Is.EqualTo(new Version(input)));
        }
    }
}

