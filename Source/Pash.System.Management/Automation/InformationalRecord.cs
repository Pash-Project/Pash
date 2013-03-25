// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public abstract class InformationalRecord
    {
        private string message;

        public string Message
        {
            get
            {
                return this.message;
            }
        }
        public InvocationInfo InvocationInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public ReadOnlyCollection<int> PipelineIterationInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal InformationalRecord(string message)
        {
            this.message = message;
        }

        internal InformationalRecord(PSObject serializedObject)
        {
            throw new NotImplementedException();
        }
    }
}
