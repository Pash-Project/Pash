// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using Pash.ParserIntrinsics;
using System;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
    public class VariablePath
    {
        public VariablePath(
            string userPath
            )
        {
            this.UserPath = userPath;

            if (userPath.Contains(":"))
            {
                var scope = userPath.Split(':')[0];
                this.IsVariable = true;

                switch (scope)
                {
                    case "global":
                        this.IsGlobal = true;
                        break;

                    case "local":
                        this.IsLocal = true;
                        break;

                    case "private":
                        this.IsPrivate = true;
                        break;

                    case "script":
                        this.IsScript = true;
                        break;

                    default:
                        this.IsVariable = false;
                        this.DriveName = scope;
                        this.IsDriveQualified = true;
                        break;
                }
            }
            else
            {
                this.IsUnqualified = true;
                this.IsUnscopedVariable = true;
                this.IsVariable = true;
            }
        }

        public string DriveName { get; private set; }
        public bool IsDriveQualified { get; private set; }
        public bool IsGlobal { get; private set; }
        public bool IsLocal { get; private set; }
        public bool IsPrivate { get; private set; }
        public bool IsScript { get; private set; }
        public bool IsUnqualified { get; private set; }
        public bool IsUnscopedVariable { get; private set; }
        public bool IsVariable { get; private set; }
        public string UserPath { get; private set; }

        public override string ToString() { return this.UserPath; }

        internal string GetUnqualifiedUserPath()
        {
            if (IsDriveQualified)
            {
                return UserPath.Substring(DriveName.Length + 1);
            }
            return UserPath;
        }
    }
}
