// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Host;
using NUnit.Framework;

namespace System.Management.Tests.HostTests
{
    [TestFixture]
    public class SizeTests
    {
        [Test]
        public void HeightAndWidthPropertiesShouldBeSetAfterNewInstanceCreated()
        {
            var size = new Size(10, 20);

            Assert.AreEqual(20, size.Height);
            Assert.AreEqual(10, size.Width);
        }

        [Test]
        public void ToStringShouldReturnWidthAndHeightSeparatedByCommad()
        {
            var size = new Size(10, 20);

            string result = size.ToString();

            Assert.AreEqual(result, "10,20");
        }

        [Test]
        public void GetHashCodeIsImplemented()
        {
            var size = new Size(10, 20);

            int hash = size.GetHashCode();

            Assert.AreEqual(30, hash);
        }

        [Test]
        public void EqualityOperatorShouldReturnTrueForSizesWithSameHeightAndWidth()
        {
            var size1 = new Size(10, 20);
            var size2 = new Size(10, 20);

            bool result = size1 == size2;

            Assert.IsTrue(result);
        }

        [TestCase(1, 2, 3, 4)]
        [TestCase(1, 2, 1, 4)]
        [TestCase(1, 2, 3, 2)]
        public void EqualityOperatorShouldReturnFalseForSizesWithDifferentHeightAndWidth(int width1, int height1, int width2, int height2)
        {
            var size1 = new Size(width1, height1);
            var size2 = new Size(width2, height2);

            bool result = size1 == size2;

            Assert.IsFalse(result);
        }

        [Test]
        public void InequalityOperatorShouldReturnFalseForSizesWithSameHeightAndWidth()
        {
            var size1 = new Size(10, 20);
            var size2 = new Size(10, 20);

            bool result = size1 != size2;

            Assert.IsFalse(result);
        }

        [TestCase(1, 2, 3, 4)]
        [TestCase(1, 2, 1, 4)]
        [TestCase(1, 2, 3, 2)]
        public void InequalityOperatorShouldReturnTrueForSizesWithDifferentHeightAndWidth(int width1, int height1, int width2, int height2)
        {
            var size1 = new Size(width1, height1);
            var size2 = new Size(width2, height2);

            bool result = size1 != size2;

            Assert.IsTrue(result);
        }

        [TestCase(1, 2, 3, 4)]
        [TestCase(1, 2, 1, 4)]
        [TestCase(1, 2, 3, 2)]
        public void EqualsShouldReturnFalseForSizesWithDifferentHeightAndWidth(int width1, int height1, int width2, int height2)
        {
            var size1 = new Size(width1, height1);
            var size2 = new Size(width2, height2);

            bool result = size1.Equals(size2);

            Assert.IsFalse(result);
        }

        [Test]
        public void EqualsShouldReturnTrueForSizesWithSameHeightAndWidth()
        {
            var size1 = new Size(10, 20);
            var size2 = new Size(10, 20);

            bool result = size1.Equals(size2);

            Assert.IsTrue(result);
        }
        
        [Test]
        public void EqualsShouldReturnFalseForNonSizeObjectBeingCompared()
        {
            var size = new Size(10, 20);

            bool result = size.Equals(new Object());

            Assert.IsFalse(result);
        }
    }
}
