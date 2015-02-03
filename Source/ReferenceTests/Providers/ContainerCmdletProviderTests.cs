using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{
    [TestFixture]
    class ContainerCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        [Test]
        public void ContainerProviderSupportsGetItem()
        {
            var cmd = "(Get-Item " + TestContainerProvider.DefaultItemPath + ").Value";
            ExecuteAndCompareTypedResult(cmd, TestContainerProvider.DefaultItemValue);
        }

        [Test]
        public void ContainerProviderSupportsGetItemWithNode()
        {
            var cmd = "Get-Item " + TestContainerProvider.DefaultNodePath;
            ExecuteAndCompareType(cmd, typeof(TestTreeNode));
        }

        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "any", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "leaf", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "container", false)]
        [TestCase(TestContainerProvider.DefaultItemPath, "leaf", true)]
        [TestCase(TestContainerProvider.DefaultItemPath, "container", false)]
        [TestCase(TestContainerProvider.DefaultItemPath, "any", true)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "leaf", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "container", true)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "any", true)]
        [TestCase(TestContainerProvider.DefaultNodePath, "any", true)]
        [TestCase(TestContainerProvider.DefaultNodePath, "leaf", true)] // is technically a container, but ContainerProvider only supports drives as containers
        [TestCase(TestContainerProvider.DefaultNodePath, "container", false)] // is technically a container, but ContainerProvider only supports drives as containers
        public void ContainerProviderSupportsTestPath(string path, string type, bool expected)
        {
            var cmd = "Test-Path " + path + " -PathType " + type;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void ContainerProviderSupportsNewItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsNewItemWithContent()
        {

        }

        [Test]
        public void ContainerProviderSupportsNewItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsRemoveItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsRemoveItemWithRecursion()
        {

        }

        [Test]
        public void ContainerProviderSupportsRemoveItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsRenameItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsRenameItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsCopyItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsCopyItemWithRecursion()
        {

        }

        [Test]
        public void ContainerProviderSupportsCopyItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetChildItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetChildItemWithRecursion()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetChildItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetLocation()
        {

        }

        [Test]
        public void ContainerProviderSupportsSetLocation()
        {

        }

        [Test]
        public void ContainerProviderSupportsSetLocationWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsPushLocation()
        {

        }

        [Test]
        public void ContainerProviderSupportsPopLocation()
        {

        }

    }
}
