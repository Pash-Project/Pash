using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    public sealed class ContentCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void Clear(string path) { throw new NotImplementedException(); }
        public Collection<IContentReader> GetReader(string path) { throw new NotImplementedException(); }
        public Collection<IContentWriter> GetWriter(string path) { throw new NotImplementedException(); }

        // internals
        //internal void Clear(string path, CmdletProviderContext context);
        //internal object ClearContentDynamicParameters(string path, CmdletProviderContext context);
        //internal ContentCmdletProviderIntrinsics(SessionStateInternal sessionState);
        //internal object GetContentReaderDynamicParameters(string path, CmdletProviderContext context);
        //internal object GetContentWriterDynamicParameters(string path, CmdletProviderContext context);
        //internal Collection<IContentReader> GetReader(string path, CmdletProviderContext context);
        //internal Collection<IContentWriter> GetWriter(string path, CmdletProviderContext context);
    }
}
