using System.Collections;

namespace System.Management.Automation
{
    public class DefaultCommandRuntime : ICommandRuntime
    {
        private ArrayList outputResults;

        public DefaultCommandRuntime(ArrayList outputResults)
        {
            this.outputResults = outputResults;
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
            if (errorRecord.Exception != null)
            {
                throw errorRecord.Exception;
            }
            throw new InvalidOperationException(errorRecord.ToString());
        }

        public void WriteObject(object sendToPipeline)
        {
            outputResults.Add(sendToPipeline);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!enumerateCollection)
            {
                outputResults.Add(sendToPipeline);
            }
            else
            {
                IEnumerator enumerator = GetEnumerator(sendToPipeline);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        outputResults.Add(enumerator.Current);
                    }
                }
                else
                {
                    outputResults.Add(sendToPipeline);
                }
            }
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