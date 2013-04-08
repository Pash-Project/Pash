// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Convert from one type to another via a string.
    /// </summary>
    public class ConvertThroughString : PSTypeConverter
    {
        public override bool CanConvertFrom(object sourceValue, Type destinationType)
        {
            return (!(sourceValue is string));
        }

        public override bool CanConvertTo(object sourceValue, Type destinationType)
        {
            return false;
        }

        //todo: do something with ignoreCase
        public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            return LanguagePrimitives.ConvertTo(
                (LanguagePrimitives.ConvertTo(sourceValue, typeof(string), formatProvider) as String),
                destinationType, formatProvider);
        }

        public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw new NotSupportedException();
        }
    }
}

