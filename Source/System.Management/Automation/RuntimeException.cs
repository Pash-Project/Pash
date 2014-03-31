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

        internal RuntimeException(string message, string errorId, ErrorCategory category, object target)
            : base(message, null)
        {
            Id = errorId;
            Category = category;
            TargetObject = target;
        }

        internal object TargetObject { get; set; }

        private ErrorRecord _errorRecord;
        public virtual ErrorRecord ErrorRecord {
            get
            {
                // TODO: I'm not quite sure if this is the intention of this property
                if (_errorRecord == null)
                {
                    _errorRecord = new ErrorRecord(this, Id, Category, TargetObject);
                }
                return _errorRecord;
            }

            set
            {
                _errorRecord = value;
            }
        }

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
