using System.Management.Automation.Language;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    internal class StringExpressionHelper
    {
        public static  string ResolveEscapeCharacters(string orig, StringConstantType quoteType)
        {
            // look at this: http://technet.microsoft.com/en-us/library/hh847755.aspx
            // it's the about_Escape_Characters page which shows that escape characters are different in PS
            var value = orig;
            if (quoteType.Equals(StringConstantType.DoubleQuoted))
            {
                var sb = new StringBuilder(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    // TODO: It *should* be safe here to use the index i + 1 because we cannot have a string
                    // literal ending in ` or " when it's not part of an escape sequence.
                    // Should we check anyway? If so, how?
                    if (value[i] == '"' && value[i + 1] == '"')
                    {
                        sb.Append('"');
                        // Skip the next character
                        i++;
                    }
                    else if (value[i] == '`')
                    {
                        switch (value[i + 1])
                        {
                            case '0': sb.Append('\0'); break;
                            case 't': sb.Append('\t'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'v': sb.Append('\v'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 'a': sb.Append('\a'); break;
                            case '\'':
                            case '$':
                            case '"':
                            case '`':
                            default:
                                sb.Append(value[i + 1]);
                                break;
                        }
                        // Skip the next character
                        i++;
                    }
                    else
                    {
                        sb.Append(value[i]);
                    }
                }
                value = sb.ToString();
            }
            else if (quoteType.Equals(StringConstantType.SingleQuoted))
            {
                value = value.Replace("''", "'");
            }
            return value;
        }
    }
}

