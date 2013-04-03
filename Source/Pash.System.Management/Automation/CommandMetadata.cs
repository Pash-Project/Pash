// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class CommandMetadata
    {
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Type CommandType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DefaultParameterSetName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SupportsShouldProcess
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SupportsTransactions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ConfirmImpact ConfirmImpact
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<string, ParameterMetadata> Parameters
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CommandMetadata(Type commandType)
        {
            throw new NotImplementedException();
        }

        public CommandMetadata(CommandInfo commandInfo)
            : this(commandInfo, false)
        {
        }

        public CommandMetadata(CommandInfo commandInfo, bool shouldGenerateCommonParameters)
        {
            throw new NotImplementedException();
        }

        public CommandMetadata(string path)
        {
            throw new NotImplementedException();
        }

        public CommandMetadata(CommandMetadata other)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<string, CommandMetadata> GetRestrictedCommands(SessionCapabilities sessionCapabilities)
        {
            throw new NotImplementedException();
        }
    }
}
