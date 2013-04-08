// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Exception that is called when ActionPreference.Stop or .Inquire is found by the runtime.
    /// </summary>
    [Serializable]
    public class ActionPreferenceStopException : RuntimeException
    {
        private ErrorRecord errorRecord;
        public override ErrorRecord ErrorRecord
        {
            get
            {
                return errorRecord;
            }
        }

        public ActionPreferenceStopException()
            : this("ActionPreferenceStop")
        {
        }

        public ActionPreferenceStopException(string message)
            : base(message)
        {
            base.Category = ErrorCategory.OperationStopped;
            base.Id = "ActionPreferenceStop";
            base.NoPrompt = true;
        }


        public ActionPreferenceStopException(string message, Exception innerException)
            : base(message, innerException)
        {
            base.Category = ErrorCategory.OperationStopped;
            base.Id = "ActionPreferenceStop";
            base.NoPrompt = true;
        }

        //todo: requires implemetation
        /*
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
        */

        // todo: handle info and context
        protected ActionPreferenceStopException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            base.NoPrompt = true;

            errorRecord = null;
        }

    }
}

