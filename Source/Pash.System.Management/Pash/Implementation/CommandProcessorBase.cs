using System.Management.Automation.Runspaces;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal abstract class CommandProcessorBase
    {
        // parameters collection (addParameter)

        internal abstract CommandInfo CommandInfo { get; }
        internal ExecutionContext ExecutionContext { get; set; }
        internal CommandParameterCollection Parameters { get; private set; }

        internal CommandProcessorBase()
        {
            Parameters = new CommandParameterCollection();
        }

        //public abstract void Initialize(parameters);
        internal abstract void Initialize();
        internal abstract void Complete();

        internal abstract void ProcessRecord();

        internal abstract void BindArguments(PSObject obj);

        internal abstract ICommandRuntime CommandRuntime { get; set; }

        internal void AddParameter(object value)
        {
            Parameters.Add(null, value);
        }

        internal void AddParameter(string name, object value)
        {
            Parameters.Add(name, value);
        }
    }
}
