// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class RuntimeException : SystemException, IContainsErrorRecord
    {
        public RuntimeException() { }

        public RuntimeException(string message)
            : this(message, null)
        {
        }

        public RuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public virtual ErrorRecord ErrorRecord { get; set; }

        protected RuntimeException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }

        internal ErrorCategory Category { private get; set; }

        internal String Id { private get; set; }

        /// <summary>
        ///  Stop the runtime from prompting the error.
        /// </summary>
        internal bool NoPrompt { get; set; }


        //    public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    throw new NotImplementedException();
        //}
        //public override string StackTrace { get { throw new NotImplementedException(); } }

        // internals
        //internal static void LockStackTrace(System.Exception e);
        //internal static System.Exception RetrieveException(System.Management.Automation.ErrorRecord errorRecord);
        //internal static string RetrieveMessage(System.Exception e);
        //internal static string RetrieveMessage(System.Management.Automation.ErrorRecord errorRecord);
        //internal RuntimeException(string message, System.Exception innerException, System.Management.Automation.ErrorRecord errorRecord);
        //internal void SetErrorId(string errorId);
        //internal void SetTargetObject(object targetObject);
        //internal bool SuppressPromptInInterpreter { set; get; }
        //internal bool WasThrownFromThrowStatement { set; get; }
    }
}
