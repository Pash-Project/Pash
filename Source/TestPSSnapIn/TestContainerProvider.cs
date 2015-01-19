using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;

namespace TestPSSnapIn
{
    [CmdletProvider(TestContainerProvider.ProviderName, ProviderCapabilities.None)]
    public class TestContainerProvider : ContainerCmdletProvider
    {
        public const string ProviderName = "TestContainerProvider";

        protected override bool IsValidPath(string path)
        {
            throw new NotImplementedException();
        }

        protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            base.CopyItem(path, copyPath, recurse);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            base.GetChildItems(path, recurse);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            base.GetChildNames(path, returnContainers);
        }

        protected override bool HasChildItems(string path)
        {
            return base.HasChildItems(path);
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            base.NewItem(path, itemTypeName, newItemValue);
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            base.RemoveItem(path, recurse);
        }

        protected override void RenameItem(string path, string newName)
        {
            base.RenameItem(path, newName);
        }

        protected override void GetItem(string path)
        {
            base.GetItem(path);
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return base.InitializeDefaultDrives();
        }

        protected override bool ItemExists(string path)
        {
            return base.ItemExists(path);
        }
    }
}
