// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public sealed class ParameterMetadata
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

        public Type ParameterType
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

        public Dictionary<string, ParameterSetMetadata> ParameterSets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDynamic
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

        public Collection<string> Aliases
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool SwitchParameter
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ParameterMetadata(string name)
            : this(name, null)
        {
        }

        public ParameterMetadata(string name, Type parameterType)
        {
            throw new NotImplementedException();
        }

        public ParameterMetadata(ParameterMetadata other)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<string, ParameterMetadata> GetParameterMetadata(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
