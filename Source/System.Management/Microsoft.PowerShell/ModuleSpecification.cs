// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Microsoft.PowerShell.Commands
{
    public class ModuleSpecification
    {

        public ModuleSpecification(string moduleName)
        {
            this.Name = moduleName;
        }

        public ModuleSpecification(Hashtable moduleSpecification)
        {
            throw new NotImplementedException();
        }

        public string Name { get; private set; }
    }
}
