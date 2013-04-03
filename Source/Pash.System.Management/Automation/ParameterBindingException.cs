// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Requires implemetation

using System;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when the given value can't be binded to a cmdlet's parameter.
    /// eg: get-date -Minute "waffle"
    /// </summary>
    [Serializable]
    public class ParameterBindingException : RuntimeException
    {
        public InvocationInfo CommandInvocation { get; private set; }

        public long Line { get; private set; }

        private string message;
        public override string Message
        {
            get
            {
                if (message == null)
                    return base.Message;
                return message;
            }
        }

        public long Offset { get; private set; }

        public string ParameterName { get; private set; }

        public Type ParameterType { get; private set; }

        public Type TypeSpecified { get; private set; }

        public ParameterBindingException()
        {
        }

        public ParameterBindingException(string message)
            : base(message)
        {
        }

        public ParameterBindingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        //todo: implement
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        protected ParameterBindingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

