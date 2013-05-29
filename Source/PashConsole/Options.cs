// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using CommandLine;
using CommandLine.Text;

namespace Pash
{
    public class Options
    {
        [Option('f', "file", 
                Required = false, 
                HelpText = "Runs the specified script in the local scope (\"dot-sourced\"), so that the functions an dvariables that the script creates are available in the current session. Enter the script file path and any parameters. File must be the last parameter in the command, because all characters typed after the file parameter name are interpreted as the script file path followed by the script parameters." )]
        public string InputFile { get; set; }

        [HelpOption]
        public string GetUsage() {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}

