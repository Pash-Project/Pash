// Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

#region BSD License
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, (either expressed or implied, of the FreeBSD Project.
#endregion

#region GPL License
// This file is part of Pash.
//
// Pash is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Pash is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along
// with Pash.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace Extensions.String
{
    static class _
    {
        #region Format
        //
        // Summary:
        //     Replaces one or more format items in a specified string with the string representation
        //     of a specified object.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The object to format.
        //
        // Returns:
        //     A copy of format in which any format items are replaced by the string representation
        //     of arg0.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format item in format is invalid.-or- The index of a format item is greater
        //     or less than zero.
        public static string FormatString(this string format, object arg0) { return string.Format(format, arg0); }
        //
        // Summary:
        //     Replaces the format item in a specified string with the string representation
        //     of a corresponding object in a specified array.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        //
        // Returns:
        //     A copy of format in which the format items have been replaced by the string
        //     representation of the corresponding objects in args.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     format or args is null.
        //
        //   System.FormatException:
        //     format is invalid.-or- The index of a format item is less than zero, or greater
        //     than or equal to the length of the args array.
        public static string FormatString(this string format, params object[] args) { return string.Format(format, args); }
        //
        // Summary:
        //     Replaces the format items in a specified string with the string representation
        //     of two specified objects.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to format.
        //
        //   arg1:
        //     The second object to format.
        //
        // Returns:
        //     A copy of format in which format items are replaced by the string representations
        //     of arg0 and arg1.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     format is invalid.-or- The index of a format item is less than zero, or greater
        //     than one.
        public static string FormatString(this string format, object arg0, object arg1) { return string.Format(format, arg0, arg1); }
        //
        // Summary:
        //     Replaces the format items in a specified string with the string representation
        //     of three specified objects.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to format.
        //
        //   arg1:
        //     The second object to format.
        //
        //   arg2:
        //     The third object to format.
        //
        // Returns:
        //     A copy of format in which the format items have been replaced by the string
        //     representations of arg0, arg1, and arg2.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     format is invalid.-or- The index of a format item is less than zero, or greater
        //     than two.
        public static string FormatString(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }
        #endregion

        // technically this doesn't extend `string`, but it's about a static method on that class, so I'm putting 
        // it here.
        public static string JoinString<T>(this IEnumerable<T> values, string separator)
        {
            return string.Join(separator, values);
        }
    }
}
