using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Pash
{
    // TODO: fix all the assembly attributes
    class Program
    {
        static void Main(string[] args)
        {

            /*
            PashHost host = new PashHost();

            using (Runspace space = RunspaceFactory.CreateRunspace(host))
            {
                space.Open();

                // Create the pipeline.
                Pipeline pipe = space.CreatePipeline();

                // Add the script we want to run. This script does two things. 
                // First, it runs the Get-Process cmdlet with the cmdlet output 
                // sorted by handle count. Second, the GetDate cmdlet is piped 
                // to the Out-String cmdlet so that we can see the
                // date displayed in German.

                pipe.Commands.AddScript(@"
                    get-process | sort handlecount
                    # This should display the date in German...
                    get-date | out-string
                    ");

                // Add the default outputter to the end of the pipe and indicate
                // that it should handle both output and errors from the previous
                // commands. This will result in the output being written using the PSHost
                // and PSHostUserInterface classes instead of returning objects to the hosting
                // application.
                pipe.Commands.Add("out-default");
                pipe.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                Collection<PSObject> results = pipe.Invoke();

                foreach (PSObject result in results)
                {
                    Console.WriteLine(result);
                }

                System.Console.WriteLine("Hit any key to exit...");
                System.Console.ReadKey();
            }
            */

            FullHost p = new FullHost();
            p.Run();
        }
    }
}
