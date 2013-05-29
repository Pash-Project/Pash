// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using CommandLine.Text;

namespace Pash
{
    // TODO: fix all the assembly attributes
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();

            var parser = new CommandLine.Parser(settings => {
                settings.HelpWriter = null;
            });
            if (parser.ParseArguments(args, options))
            {

                var fullHost = new FullHost();

                if (!string.IsNullOrEmpty(options.InputFile))
                {
                    int fileParamStart = -1;
                    for (int i = 0; i < args.Length; i++)
                    {
                        //Console.WriteLine("arg[" + i + "]" + args[i]);
                        if (args[i].Equals("-f", StringComparison.InvariantCultureIgnoreCase) ||
                            args[i].Equals("--file", StringComparison.InvariantCultureIgnoreCase))
                        {
                            fileParamStart = i;
                        }
                    }

                    if (fileParamStart > -1)
                    {
                        // Check to see if we're passing in arguments after the file.
                        if (args.Length > fileParamStart + 2)
                        {
                            // I'm not sure how best to handle file arguments at the command line
                            // Below could be one way (take the input args, execute them in the 
                            // runspace by assigning them to the $args array?
                            //
                            //var argsArray = string.Join(",", args.Skip(fileParamStart + 2));
                            //var inputArgs = "$args=@(" + argsArray + ")";
                            //Console.WriteLine(inputArgs);

                            Console.WriteLine("Parameters to a file not yet supported...");
                            return;
                        }
                    }

                    var script = System.IO.File.ReadAllText(options.InputFile);
                    fullHost.Execute(script);

                    return;
                }

                fullHost.Run();
            }
            else
            {
                Console.WriteLine(options.GetUsage());
            }
        }
    }
}
