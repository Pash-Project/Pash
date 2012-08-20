using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public struct LanguageFlags {
    public bool None;

    //Compilation options
    //Be careful - use this flag ONLY if you use NewLine terminal in grammar explicitly!
    // - it happens only in line-based languages like Basic.
    public bool NewLineBeforeEOF;
    //Emit LineStart token
    public bool EmitLineStartToken;
    public bool DisableScannerParserLink; //in grammars that define TokenFilters (like Python) this flag should be set
    public bool CreateAst; //create AST nodes 

    //Runtime
    public bool SupportsCommandLine;
    public bool TailRecursive; //Tail-recursive language - Scheme is one example
    public bool SupportsBigInt;
    public bool SupportsComplex;
    public bool SupportsRational;

    public static LanguageFlags Default { get { return new LanguageFlags(); } }
  }

  //Operator associativity types
  public enum Associativity {
    Left,
    Right,
    Neutral  //honestly don't know what that means, but it is mentioned in literature 
  }

  //Used by Make-list-rule methods
  [Flags]
  public enum TermListOptions {
    None = 0,
    AllowEmpty = 0x01,
    AllowTrailingDelimiter = 0x02,

    // In some cases this hint would help to resolve the conflicts that come up when you have two lists separated by a nullable term.
    // This hint would resolve the conflict, telling the parser to include as many as possible elements in the first list, and the rest, 
    // if any, would go to the second list. By default, this flag is included in Star and Plus lists. 
    AddPreferShiftHint = 0x04,
    //Combinations - use these 
    PlusList = AddPreferShiftHint,
    StarList = AllowEmpty | AddPreferShiftHint,
  }

}
