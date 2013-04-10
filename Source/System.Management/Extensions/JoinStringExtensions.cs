using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// this is useful in so many places that I'm putting it in the root namespace
static class JoinStringExtensions
{
    //
    // Summary:
    //     Concatenates the members of a constructed System.Collections.Generic.IEnumerable<T>
    //     collection of type System.String, using the specified separator between each
    //     member.
    //
    // Parameters:
    //   separator:
    //     The string to use as a separator.
    //
    //   values:
    //     A collection that contains the strings to concatenate.
    //
    // Returns:
    //     A string that consists of the members of values delimited by the separator
    //     string.
    //
    // Exceptions:
    //   System.ArgumentNullException:
    //     values is null.
    public static string JoinString(this IEnumerable<string> @this, string separator) { return string.Join(separator, @this); }

    //
    // Summary:
    //     Concatenates the members of a string collection, using the specified separator
    //     between each member.
    //
    // Parameters:
    //   separator:
    //     The string to use as a separator.
    //
    //   values:
    //     A collection that contains the objects to concatenate.
    //
    // Type parameters:
    //   T:
    //     The type of the members of values.
    //
    // Returns:
    //     A string that consists of the members of values delimited by the separator
    //     string.
    //
    // Exceptions:
    //   System.ArgumentNullException:
    //     values is null.
    public static string JoinString<T>(this IEnumerable<T> @this, string separator) { return string.Join(separator, @this); }

    //
    // Summary:
    //     Concatenates the elements of an object array, using the specified separator
    //     between each element.
    //
    // Parameters:
    //   separator:
    //     The string to use as a separator.
    //
    //   values:
    //     An array that contains the elements to concatenate.
    //
    // Returns:
    //     A string that consists of the elements of values delimited by the separator
    //     string.
    //
    // Exceptions:
    //   System.ArgumentNullException:
    //     values is null.
    public static string JoinString(this object[] @this, string separator) { return string.Join(separator, @this); }

    //
    // Summary:
    //     Concatenates all the elements of a string array, using the specified separator
    //     between each element.
    //
    // Parameters:
    //   separator:
    //     The string to use as a separator.
    //
    //   value:
    //     An array that contains the elements to concatenate.
    //
    // Returns:
    //     A string that consists of the elements in value delimited by the separator
    //     string.
    //
    // Exceptions:
    //   System.ArgumentNullException:
    //     value is null.
    public static string JoinString(this string[] @this, string separator) { return string.Join(separator, @this); }
}
