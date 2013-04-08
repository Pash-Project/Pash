// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Abstract class for implemeting specific type converters for various Pash types.
    /// </summary>
    public abstract class PSTypeConverter
    {
        protected PSTypeConverter()
        {
        }

        public abstract bool CanConvertFrom(object sourceValue, Type destinationType);
        public abstract object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase);

        public abstract bool CanConvertTo(object sourceValue, Type destinationType);
        public abstract object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase);
    }
}

