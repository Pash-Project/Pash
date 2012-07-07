using System;
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Resources;
using System.Collections;
using System.Threading;
using ExecutionContext=Pash.Implementation.ExecutionContext;

namespace System.Management.Automation
{
    public class Cmdlet : InternalCommand
    {
        internal string _ParameterSetName { get; set; }

        protected Cmdlet()
        {
        }

        public bool Stopping
        {
            get
            {
                return IsStopping;
            }
        }

        protected virtual void BeginProcessing()
        {
        }

        protected virtual void EndProcessing()
        {
        }

        public virtual string GetResourceString(string baseName, string resourceId)
        {
            ResourceManager resourceManager = new ResourceManager(GetType());
            string str = null;
            try
            {
                str = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
            }
            catch
            {
            }

            return str;
        }

        public IEnumerable Invoke()
        {
            yield return GetResults();
        }

        public IEnumerable<T> Invoke<T>() 
        {
            // TODO: implement Invoke<T>
            throw new NotImplementedException(); 
        }

        protected virtual void ProcessRecord()
        {
        }

        public bool ShouldContinue(string query, string caption)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldContinue(query, caption);
            }
            return true;
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldContinue(query, caption);
            }
            return true;
        }

        public bool ShouldProcess(string target)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(target);
            }
            return true;
        }

        public bool ShouldProcess(string target, string action)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(target, action);
            }
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption);
            }
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
            }
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        protected virtual void StopProcessing()
        {
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            if (CommandRuntime == null)
            {
                if (errorRecord.Exception != null)
                {
                    throw errorRecord.Exception;
                }
                throw new InvalidOperationException(errorRecord.ToString());
            }
            CommandRuntime.ThrowTerminatingError(errorRecord);
        }

        public void WriteCommandDetail(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteCommandDetail");
            }
            CommandRuntime.WriteCommandDetail(text);
        }

        public void WriteDebug(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteDebug");
            }
            CommandRuntime.WriteDebug(text);
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteError");
            }
            CommandRuntime.WriteError(errorRecord);
        }
        
        public void WriteObject(object sendToPipeline)
        { 
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteObject");
            }
            CommandRuntime.WriteObject(sendToPipeline);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteObject");
            }
            CommandRuntime.WriteObject(sendToPipeline, enumerateCollection);
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteProgress");
            }
            CommandRuntime.WriteProgress(progressRecord);
        }

        internal void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteProgress");
            }
            CommandRuntime.WriteProgress(sourceId, progressRecord);
        }

        public void WriteVerbose(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteVerbose");
            }
            CommandRuntime.WriteVerbose(text);
        }
        
        public void WriteWarning(string text) 
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteWarning");
            }
            CommandRuntime.WriteWarning(text);
        }

        // internals
        internal override void DoBeginProcessing()
        {
            BeginProcessing();
        }

        internal override void DoEndProcessing()
        {
            EndProcessing();
        }

        internal override void DoProcessRecord()
        {
            ProcessRecord();
        }

        internal override void DoStopProcessing()
        {
            StopProcessing();
        }

        internal ArrayList GetResults()
        {
            if (this is PSCmdlet)
            {
                throw new InvalidOperationException("Can't invoke PSCmdlets directly");
            }
            ArrayList outputArrayList = new ArrayList();
            CommandRuntime = new DefaultCommandRuntime(outputArrayList);
            BeginProcessing();
            ProcessRecord();
            EndProcessing();
            return outputArrayList;
        }
    }
}
