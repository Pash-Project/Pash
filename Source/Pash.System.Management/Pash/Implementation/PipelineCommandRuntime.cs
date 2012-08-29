using System.Collections;
using System.Collections.Generic;
using Pash.Implementation;

namespace System.Management.Automation
{
    // TODO: can we replace the runtime with the ExecutionContext?
    internal class PipelineCommandRuntime : ICommandRuntime
    {
        private ExecutionContext _context;

        internal ObjectStream outputResults { get; private set; }
        internal ObjectStream errorResults { get; private set; }
        internal PipelineProcessor pipelineProcessor { get; private set; }

        // TODO: hook the runtime to the Host for Debug and Error output
        internal PipelineCommandRuntime(PipelineProcessor pipelineProcessor)
        {
            this.pipelineProcessor = pipelineProcessor;
            this.outputResults = new ObjectStream();
            this.errorResults = new ObjectStream();
        }

        internal bool IsStopping
        {
            get
            {
                return ((pipelineProcessor != null) && pipelineProcessor.Stopping);
            }
        }

        #region ICommandRuntime Members

        public Host.PSHost Host
        {
            get { throw new NotImplementedException(); }
        }

        public bool ShouldContinue(string query, string caption)
        {
            return true;
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            return true;
        }

        public bool ShouldProcess(string target)
        {
            return true;
        }

        public bool ShouldProcess(string target, string action)
        {
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            if (errorRecord.Exception != null)
            {
                throw errorRecord.Exception;
            }
            throw new InvalidOperationException(errorRecord.ToString());
        }

        public void WriteCommandDetail(string text)
        {
        }

        public void WriteDebug(string text)
        {
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            errorResults.Write(PSObject.AsPSObject(errorRecord));
        }

        public void WriteObject(object sendToPipeline)
        {
            outputResults.Write(PSObject.AsPSObject(sendToPipeline));
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (enumerateCollection && !(sendToPipeline is string))
            {
                IEnumerator enumerator = GetEnumerator(sendToPipeline);
                if (enumerator != null)
                {
                    while ((enumerator.MoveNext()))
                    {
                        WriteObject(enumerator.Current);
                    }
                    return;
                }
            }

            WriteObject(sendToPipeline);
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
        }

        public void WriteVerbose(string text)
        {
        }

        public void WriteWarning(string text)
        {
        }

        #endregion

        private IEnumerator GetEnumerator(object obj)
        {
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            IEnumerator enumerator = obj as IEnumerator;
            if (obj != null)
            {
                return enumerator;
            }

            return null;
        }
    }
}