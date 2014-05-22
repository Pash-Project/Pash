// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections;
using System.Collections.Generic;
using Pash.Implementation;

namespace System.Management.Automation
{
    internal class PipelineCommandRuntime : ICommandRuntime
    {
        internal bool MergeErrorToOutput;
        internal bool MergeUnclaimedPreviousErrors;
        internal ObjectStream OutputStream { get; private set; }
        internal ObjectStream ErrorStream { get; private set; }
        internal ObjectStream InputStream { get; private set; }
        internal PipelineProcessor PipelineProcessor { get; set; }
        internal ExecutionContext ExecutionContext { get; set; }

        // TODO: hook the runtime to the Host for Debug and Error output
        internal PipelineCommandRuntime(PipelineProcessor pipelineProcessor)
        {
            PipelineProcessor = pipelineProcessor;
            MergeErrorToOutput = false;
            MergeUnclaimedPreviousErrors = false;
            OutputStream = new ObjectStream(this);
            ErrorStream = new ObjectStream(this);
            InputStream = new ObjectStream(this);
        }

        internal bool IsStopping
        {
            get
            {
                return (PipelineProcessor != null) && PipelineProcessor.Stopping;
            }
        }

        #region ICommandRuntime Members

        public Host.PSHost Host
        {
            get
            {
                return (ExecutionContext != null) ? ExecutionContext.LocalHost : null;
            }
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
            var psobj = PSObject.AsPSObject(errorRecord);
            // if merged with stdout, we can later on check to which stream the object usually belongs
            psobj.WriteToErrorStream = true;
            ErrorStream.Write(psobj);
            ExecutionContext.AddToErrorVariable(errorRecord);
            if (MergeErrorToOutput)
            {
                OutputStream.Write(psobj);
            }
        }

        public void WriteObject(object sendToPipeline)
        {
            OutputStream.Write(PSObject.AsPSObject(sendToPipeline));
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            var isString = sendToPipeline is string || 
                           (sendToPipeline is PSObject && ((PSObject) sendToPipeline).BaseObject is string);
            if (enumerateCollection && !(isString))
            {
                IEnumerator enumerator = GetEnumerator(sendToPipeline);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
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

        internal IEnumerator GetEnumerator(object obj)
        {
            if (obj is PSObject)
            {
                obj = ((PSObject)obj).BaseObject;
            }
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            IEnumerator enumerator = obj as IEnumerator;
            if (enumerator != null)
            {
                return enumerator;
            }

            return null;
        }
    }
}
