﻿#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public struct SourceLocation {
    public int Position;
    /// <summary>Source line number, 0-based.</summary>
    public int Line;
    /// <summary>Source column number, 0-based.</summary>
    public int Column;
    public SourceLocation(int position, int line, int column) {
      Position = position;
      Line = line;
      Column = column;
    }
    //Line/col are zero-based internally
    public override string ToString() {
      return string.Format(Resources.FmtRowCol, Line + 1, Column + 1);
    }
    //Line and Column displayed to user should be 1-based
    public string ToUiString() {
      return string.Format(Resources.FmtRowCol, Line + 1, Column + 1);
    }
    public static int Compare(SourceLocation x, SourceLocation y) {
      if (x.Position < y.Position) return -1;
      if (x.Position == y.Position) return 0;
      return 1;
    }
    public static SourceLocation Empty {
      get { return _empty; }
    } static SourceLocation _empty = new SourceLocation();  

    public static SourceLocation operator + (SourceLocation x, SourceLocation y) {
      return new SourceLocation(x.Position + y.Position, x.Line + y.Line, x.Column + y.Column); 
    }
    public static SourceLocation operator + (SourceLocation x, int offset) {
      return new SourceLocation(x.Position + offset, x.Line, x.Column + offset); 
    }
  }//SourceLocation

  public struct SourceSpan {
    public readonly SourceLocation Location;
    public readonly int Length;
    public SourceSpan(SourceLocation location, int length) {
      Location = location;
      Length = length;
    }
    public int EndPosition {
      get { return Location.Position + Length; }
    }
    public bool InRange(int position) {
      return (position >= Location.Position && position <= EndPosition);
    }
    public override string ToString(){
      return this.Location.ToUiString() + " + " + this.Length.ToString();
    }

  }


}//namespace
