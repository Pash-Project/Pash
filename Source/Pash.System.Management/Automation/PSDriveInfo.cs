using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
    public class PSDriveInfo : IComparable
    {
        // TODO: drive can be hidden
        public string Name { get; private set; }
        public ProviderInfo Provider { get; private set; }
        public string Root { get; internal set; }
        public string Description { get; set; }
        public PSCredential Credential { get; private set; }
        public string CurrentLocation { get; set; }
        internal bool RemovableDrive { get; set; }

        protected PSDriveInfo(PSDriveInfo driveInfo)
        {
            Name = driveInfo.Name;
            Provider = driveInfo.Provider;
            Root = driveInfo.Root;
            Description = driveInfo.Description;
            Credential = driveInfo.Credential;
            CurrentLocation = driveInfo.CurrentLocation;
        }

        public PSDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential)
        {
            Name = name;
            Provider = provider;
            Root = root;
            Description = description;
            Credential = credential;
            CurrentLocation = string.Empty;
        }

        public static bool operator !=(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            return !(drive1 == drive2);
        }

        public static bool operator <(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object objDrive1 = drive1;
            object objDrive2 = drive2;
            if ((objDrive1 == null) != (objDrive2 == null))
            {
                return false;
            }
            if (objDrive1 != null)
            {
                return drive1.CompareTo(drive2) < 0;
            }
            return true;
        }

        public static bool operator ==(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object objDrive1 = drive1;
            object objDrive2 = drive2;
            if ((objDrive1 == null) != (objDrive2 == null))
            {
                return false;
            }
            if (objDrive1 != null)
            {
                return drive1.Equals(drive2);
            }
            return true;
        }

        public static bool operator >(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object objDrive1 = drive1;
            object objDrive2 = drive2;
            if ((objDrive1 == null) != (objDrive2 == null))
            {
                return false;
            }
            if (objDrive1 != null)
            {
                return drive1.CompareTo(drive2) > 0;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is PSDriveInfo)
                return CompareTo(obj as PSDriveInfo) == 0;

            throw new InvalidOperationException("Can compare only to PSDriveInfo");
        }

        public bool Equals(PSDriveInfo drive)
        {
            return (CompareTo(drive) == 0);
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Provider.FullName, Name);
        }

        #region IComparable Members

        public int CompareTo(PSDriveInfo drive)
        {
            return string.Compare(Name, drive.Name, true, CultureInfo.CurrentUICulture);
        }

        public int CompareTo(object obj)
        {
            if (obj is PSDriveInfo)
                return CompareTo(obj as PSDriveInfo);

            throw new InvalidOperationException("Can compare only to PSDriveInfo");
        }

        #endregion
    }
}
