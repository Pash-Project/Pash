using System;
using System.IO;
using System.Text;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class FileOutputWriter : OutputWriter
    {
        private StreamWriter _streamWriter;

        public FileOutputWriter(string filename, bool append, bool overwrite, Encoding encoding)
            : this(filename, append, overwrite, encoding, -1)
        {
        }

        public FileOutputWriter(string filename, bool append, bool overwrite, Encoding encoding, int width)
            : base (-1, width)
        {
            var mode = FileMode.Create;
            var access = FileAccess.Write;
            if (append && !overwrite)
            {
                mode = FileMode.Open;
            }
            else if (append && overwrite)
            {
                mode = FileMode.Append;
            }
            else if (!append && !overwrite)
            {
                mode = FileMode.CreateNew;
            }
            var file = File.Open(filename, mode, access);
            _streamWriter = new StreamWriter(file, encoding);
        }

        public override void Close()
        {
            _streamWriter.Close();
        }

        public override void WriteLine(string output)
        {
            _streamWriter.Write(output);
        }
    }
}

