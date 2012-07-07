using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class FileSystemContentDynamicParametersBase
    {
        [Parameter]
        public FileSystemCmdletProviderEncoding Encoding { get; set; }
        public bool UsingByteEncoding { get { return Encoding == FileSystemCmdletProviderEncoding.Byte; } }
        public bool WasStreamTypeSpecified { get { return Encoding != FileSystemCmdletProviderEncoding.String; } }

        public FileSystemContentDynamicParametersBase()
        {
            Encoding = FileSystemCmdletProviderEncoding.String;
        }

        public System.Text.Encoding EncodingType
        {
            get
            {
                switch (Encoding)
                {
                    case FileSystemCmdletProviderEncoding.String:
                        return System.Text.Encoding.Unicode;

                    case FileSystemCmdletProviderEncoding.Unicode:
                        return System.Text.Encoding.Unicode;

                    case FileSystemCmdletProviderEncoding.BigEndianUnicode:
                        return System.Text.Encoding.BigEndianUnicode;

                    case FileSystemCmdletProviderEncoding.UTF8:
                        return System.Text.Encoding.UTF8;

                    case FileSystemCmdletProviderEncoding.UTF7:
                        return System.Text.Encoding.UTF7;

                    case FileSystemCmdletProviderEncoding.Ascii:
                        return System.Text.Encoding.ASCII;
                }
                return System.Text.Encoding.Unicode;
            }
        }

    }
}