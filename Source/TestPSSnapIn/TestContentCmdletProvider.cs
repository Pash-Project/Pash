using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;

namespace TestPSSnapIn
{
    [CmdletProvider(TestContentCmdletProvider.ProviderName, ProviderCapabilities.None)]
    public class TestContentCmdletProvider : DriveCmdletProvider, IContentCmdletProvider
    {
        public const string ProviderName = "TestContentCmdletProvider";
        public static List<string> Messages = new List<string>();

        public void ClearContent(string path)
        {
            Messages.Add(string.Format("ClearContent {0}", path));
        }

        public object ClearContentDynamicParameters(string path)
        {
            //Messages.Add(string.Format("ClearContentDynamicParameters {0}", path));
            return null;
        }

        public IContentReader GetContentReader(string path)
        {
            Messages.Add(string.Format("GetContentReader {0}", path));
            return new ContentReader(this);
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            //Messages.Add(string.Format("GetContentReaderDynamicParameters {0}", path));
            return null;
        }

        public IContentWriter GetContentWriter(string path)
        {
            Messages.Add(string.Format("GetContentWriter {0}", path));
            return new ContentWriter(this);
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            //Messages.Add(string.Format("GetContentWriterDynamicParameters {0}", path));
            return null;
        }

        class ContentWriter : IContentWriter
        {
            TestContentCmdletProvider _provider;

            public ContentWriter(TestContentCmdletProvider provider)
            {
                _provider = provider;
            }

            public void Close()
            {
                _provider.AddMessage("ContentWriter.Close()");
            }

            public void Seek(long offset, SeekOrigin origin)
            {
                _provider.AddMessage(string.Format("ContentWriter.Seek({0}, {1}", offset, origin));
            }

            public IList Write(IList content)
            {
                var items = new List<object>();
                foreach (object item in content)
                {
                    items.Add(item);
                }
                string text = String.Join(",", items.ToArray());
                _provider.AddMessage(String.Format("ContentWriter.Write({0})", text));

                return content;
            }

            public void Dispose()
            {
                _provider.AddMessage("ContentWriter.Dispose()");
            }
        }

        class ContentReader : IContentReader
        {
            TestContentCmdletProvider _provider;

            public ContentReader(TestContentCmdletProvider provider)
            {
                _provider = provider;
            }

            public void Close()
            {
                _provider.AddMessage("ContentReader.Close()");
            }

            public IList Read(long readCount)
            {
                _provider.AddMessage(String.Format("ContentReader.Read({0})", readCount));
                return new string[0];
            }

            public void Seek(long offset, SeekOrigin origin)
            {
                _provider.AddMessage(String.Format("ContentReader.Seek({0}, {1})", offset, origin));
            }

            public void Dispose()
            {
                _provider.AddMessage("ContentReader.Dispose()");
            }
        }

        internal void AddMessage(string message)
        {
            Messages.Add(message);
        }
    }
}
