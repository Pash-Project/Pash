// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;

namespace System.Management.Automation
{
    public sealed class PowerShell : IDisposable
    {
        private RunspacePool runspacePool;
        private Runspace runspace;
        private PSCommand psCommand;
        private PSDataStreams dataStreams;

        public event EventHandler<PSInvocationStateChangedEventArgs> InvocationStateChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                throw new NotImplementedException();
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                throw new NotImplementedException();
            }
        }
        public PSCommand Commands
        {
            get
            {
                return psCommand;
            }
            set
            {
                psCommand = value;
            }
        }
        public PSDataStreams Streams
        {
            get
            {
                return dataStreams;
            }
        }

        public Guid InstanceId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public PSInvocationStateInfo InvocationStateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public bool IsNested
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Runspace Runspace
        {
            get
            {
                return runspace;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Runspace");

                runspace = value;
            }
        }

        public RunspacePool RunspacePool
        {
            get
            {
                return runspacePool;
            }
            set
            {
                runspacePool = value;
            }
        }

        public static PowerShell Create()
        {
            return new PowerShell();
        }

        private PowerShell()
        {
            psCommand = new PSCommand();
            dataStreams = new PSDataStreams(this);
        }

        public PowerShell CreateNestedPowerShell()
        {
            throw new NotImplementedException();
        }

        public PowerShell AddCommand(string cmdlet)
        {
            throw new NotImplementedException();
        }

        public PowerShell AddCommand(string cmdlet, bool useLocalScope)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a Script into the queue of commands.
        /// </summary>
        /// <param name="script">Script to be run.</param>
        public PowerShell AddScript(string script)
        {
            psCommand.AddScript(script);

            return this;
        }

        public PowerShell AddScript(string script, bool useLocalScope)
        {
            throw new NotImplementedException();
        }

        public PowerShell AddParameter(string parameterName, object value)
        {
            throw new NotImplementedException();
        }

        public PowerShell AddParameter(string parameterName)
        {
            throw new NotImplementedException();
        }

        public PowerShell AddParameters(IList parameters)
        {
            throw new NotImplementedException();
        }

        public PowerShell AddParameters(IDictionary parameters)
        {
            throw new NotImplementedException();
        }

        public PowerShell AddArgument(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Runs all commands in the query.
        /// </summary>
        /// <returns>A Collection of the results. </returns>
        public Collection<PSObject> Invoke()
        {
            //throw new NotImplementedException();
            return new Collection<PSObject>();
        }

        public Collection<PSObject> Invoke(IEnumerable input)
        {
            throw new NotImplementedException();
        }

        public Collection<PSObject> Invoke(IEnumerable input, PSInvocationSettings settings)
        {
            throw new NotImplementedException();
        }

        public Collection<T> Invoke<T>()
        {
            throw new NotImplementedException();
        }

        public Collection<T> Invoke<T>(IEnumerable input)
        {
            throw new NotImplementedException();
        }

        public Collection<T> Invoke<T>(IEnumerable input, PSInvocationSettings settings)
        {
            throw new NotImplementedException();
        }

        public void Invoke<T>(IEnumerable input, IList<T> output)
        {
            throw new NotImplementedException();
        }

        public void Invoke<T>(IEnumerable input, IList<T> output, PSInvocationSettings settings)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginInvoke()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input, PSInvocationSettings settings, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public PSDataCollection<PSObject> EndInvoke(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginStop(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndStop(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
