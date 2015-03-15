// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Get-Random
    /// 
    /// DESCRIPTION
    ///   Retrieves a random number.
    ///   
    /// RELATED POSIX COMMANDS
    ///   /dev/random 
    /// </summary>
    [Cmdlet("Get", "Random")]
    [OutputType(typeof(int), typeof(long), typeof(double))]
    public sealed class GetRandomCommand : Cmdlet
    {
        int seed;
        bool seed_is_set;

        protected override void ProcessRecord()
        {
            if (seed_is_set)
                WriteObject(new Random(SetSeed).Next());

            else WriteObject(new Random().Next());
        }

        [Parameter]
        public int SetSeed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = value;
                seed_is_set = true;
            }
        }
    }
}