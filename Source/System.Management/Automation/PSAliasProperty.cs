// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Needs full implementation (consistuant on runtime advances)

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Represents an alias to a property.
    /// </summary>
    public class PSAliasProperty : PSPropertyInfo
    {
        public PSAliasProperty(string name, string referencedMemberName)
        {
            base.Name = name;
            ReferencedMemberName = referencedMemberName;
        }

        public PSAliasProperty(string name, string referencedMemberName, Type conversionType)
        {
            base.Name = name;
            ReferencedMemberName = referencedMemberName;
            ConversionType = conversionType;
        }

        //todo: implement
        public override PSMemberInfo Copy()
        {
            return null;
        }

        public override string ToString()
        {
            if (ConversionType != null)
                return (base.Name + " (" + ConversionType + ") = " + ReferencedMemberName);

            return (base.Name + " = " + ReferencedMemberName);
        }

        public Type ConversionType { get; private set; }

        private bool isgettable;
        public override bool IsGettable
        {
            get
            {
                return isgettable;
            }
        }


        private bool issettable;
        public override bool IsSettable
        {
            get
            {
                return issettable;
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.AliasProperty;
            }
        }

        public string ReferencedMemberName { get; private set; }

        public override string TypeNameOfValue
        {
            get
            {
                return ConversionType.FullName ?? "Null";
            }
        }

        public override object Value { get; set; }
    }
}

