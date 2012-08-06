using System;
using System.Management.Automation.Runspaces;

namespace PashGui
{
    class Model
    {
        readonly public Runspace runspace;

        public Model(Runspace runspace)
        {
            this.runspace = runspace;
        }
    }
}

