// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell.Commands
{
    public class MatchInfo
    {
        private int _lineNumber;

        public MatchInfoContext Context { get; set; }
        public string Filename { get; internal set; }
        public bool IgnoreCase { get; set; }
        public string Line { get; set; }
        public int LineNumber { get; set; }
        public Match[] Matches { get; set; }
        public string Path { get; set; }
        public string Pattern { get; set; }

        public override string ToString()
        {
            if (Filename == "InputStream")
            {
                return Line;
            }
            return String.Format("{0}:{1}:{2}", Path, LineNumber, Line);
        }

        public MatchInfo()
        {
        }

        internal MatchInfo(string path, string pattern, IEnumerable<Match> matches, string line, int lineNumber, bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
            Line = line;
            LineNumber = lineNumber;
            Filename = System.IO.Path.GetFileName(path);
            Matches = matches.ToArray();
            Path = path;
            Pattern = pattern;
        }

        internal MatchInfo(string path, string pattern, string line, int lineNumber, bool ignoreCase)
        {
            Filename = System.IO.Path.GetFileName(path);
            IgnoreCase = ignoreCase;
            Line = line;
            LineNumber = lineNumber;
            Matches = new Match[0];
            Path = path;
            Pattern = pattern;
        }
    }
}
