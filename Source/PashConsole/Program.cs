// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Pash
{
    // TODO: fix all the assembly attributes
    internal class Program
    {
        static int Main(string[] args)
        {
            var interactive = true; // interactive by default
            StringBuilder commands = new StringBuilder();
            int startCommandAt = 0;

            // check first arg for "-noexit"
            if (args.Length > 0)
            {
                // no interactive shell if we have commands given but not this parameter
                interactive = args[0].Equals("-noexit");
                if (interactive)
                {
                    // ignore first arg as it is no command
                    startCommandAt = 1;
                }
            }

            // other args are interpreted as commands to be executed
            for (int i = startCommandAt; i < args.Length; i++)
            {
                commands.Append(args[i]);
                commands.Append("; ");
            }

            FullHost p = new FullHost();
            return p.Run(interactive, commands.ToString());
        }
    }
}
