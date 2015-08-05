// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace Extensions.Types
{
    static class TypeExtensions
    {
        public static string FriendlyName(this Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                    return "int";
                case TypeCode.String:
                    return "string";
            }

            return type.Name;
        }
    }
}
