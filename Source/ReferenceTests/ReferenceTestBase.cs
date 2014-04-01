// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace ReferenceTests
{
    public class ReferenceTestBase
    {
        private List<string> _createdScripts;
        private string _assemblyDirectory;
        private Regex _whiteSpaceRegex;
        private bool _isMonoRuntime;

        public ReferenceTestBase()
        {
            _createdScripts = new List<string>();
            _assemblyDirectory = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            _whiteSpaceRegex = new Regex(@"\s");
            _isMonoRuntime = ReferenceTestInfo.IS_PASH && Type.GetType("Mono.Runtime") != null;
        }

        private String QuoteWithSpace(string input)
        {
            if (_whiteSpaceRegex.IsMatch(input))
            {
                return String.Format(@"""{0}""", input);
            }
            return input;
        }

        private Process CreateDotNetProcess(string command, params string[] args)
        {
            string realCmd = null;
            List<string> realArgs = new List<string>();
            if (_isMonoRuntime)
            {
                realCmd = "mono";
                realArgs.Add(QuoteWithSpace(command));
            }
            else
            {
                realCmd = command;
            }

            foreach (var arg in args)
            {
                realArgs.Add(QuoteWithSpace(arg));
            }
            var process = new Process();
            process.StartInfo.FileName = realCmd;
            process.StartInfo.Arguments = String.Join(" ", realArgs);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            return process;
        }

        public Process CreatePowershellOrPashProcess(params string[] args)
        {
            // NOTE: make sure when using functions with "args", that the arguments
            // only use *SINGLE* quotes itself

            return CreateDotNetProcess(ReferenceTestInfo.SHELL_EXECUTABLE, args);
        }

        public string FinishProcess(Process process, bool closeInput = true)
        {
            if (closeInput)
            {
                process.StandardInput.Flush();
                process.StandardInput.Close();
            }
            var output = process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(3000))
            {
                process.Kill();
                throw new Exception(ReferenceTestInfo.SHELL_NAME + " didn't exit");
            }
            return output;
        }

        public string CreateScript(string script)
        {
            string fileName = Path.Combine(_assemblyDirectory, String.Format("TempScript{0}.ps1", _createdScripts.Count));
            File.WriteAllText(fileName, script);
            _createdScripts.Add(fileName);

            return fileName;
        }

        public void RemoveCreatedScripts()
        {
            foreach (var file in _createdScripts)
            {
                File.Delete(file);
            }
            _createdScripts.Clear();
        }

        public static string CmdletName(Type cmdletType)
        {
            var attribute = System.Attribute.GetCustomAttribute(cmdletType, typeof(CmdletAttribute))
                            as CmdletAttribute;
            return string.Format("{0}-{1}", attribute.VerbName, attribute.NounName);
        }

        public static string NewlineJoin(string[] parts)
        {
            return String.Join(Environment.NewLine, parts) + Environment.NewLine;
        }
    }
}

