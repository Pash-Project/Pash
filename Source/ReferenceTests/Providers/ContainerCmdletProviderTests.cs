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
        public void ContainerProviderSupportsTestPath()
        {

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
