// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Runspaces
{
    public class InvalidPipelineStateException : SystemException
    {
        public PipelineState CurrentState { get; private set; }
        public PipelineState ExpectedState { get; private set; }

        public InvalidPipelineStateException() : base()
        {
        }

        public InvalidPipelineStateException(string message)
            : base(message)
        {
        }

        public InvalidPipelineStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal InvalidPipelineStateException(string message, PipelineState currentState, PipelineState expectedState)
        {
            CurrentState = currentState;
            ExpectedState = expectedState;
        }

        public override string ToString()
        {
            return string.Format("InvalidPipelineStateException: {0}. Current state is {1}, but expected was {2}.",
                 Message, CurrentState, ExpectedState);
        }
    }
}

