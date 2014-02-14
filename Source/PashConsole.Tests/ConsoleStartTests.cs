using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pash;

namespace PashConsoleTests
{
    [TestFixture]
    public class ConsoleStartTest
    {
        private Regex _whiteSpaceRegex = new Regex(@"\s");

        private String SingleQuoteWithSpace(string input)
        {
            if (_whiteSpaceRegex.IsMatch(input))
            {
                return String.Format("'{0}'", input);
            }
            return input;
        }

        private Process CreateDotNetProcess(string command, params string[] args)
        {
            string realCmd = null;
            List<string> realArgs = new List<string>();
            // find out which runtime environment we have: MS .NET or Mono
            if (Type.GetType("Mono.Runtime") != null)
            {
                realCmd = "mono";
                realArgs.Add(SingleQuoteWithSpace(command));
            }
            else
            {
                realCmd = command;
            }

            foreach (var arg in args)
            {
                realArgs.Add(SingleQuoteWithSpace(arg));
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

        private Process CreatePashProcess(params string[] args)
        {
            var cmd = typeof(Pash.Program).Assembly.Location;
            return CreateDotNetProcess(cmd, args);
        }

        private string FinishProcess(Process process, bool closeInput = true)
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
                throw new Exception("Pash didn't exit");
            }
            return output;
        }

        [Test]
        public void StartAndExitWithoutError()
        {
            var process = CreatePashProcess();
            process.Start();
            FinishProcess(process);
            Assert.AreEqual(0, process.ExitCode);
        }

        [Test]
        public void StartAndExitByCommand()
        {
            var process = CreatePashProcess();
            process.Start();
            process.StandardInput.WriteLine("exit");
            process.StandardInput.WriteLine(@"            Write-Host ""foo""");
            var output = FinishProcess(process);
            Assert.False(output.Contains("foo"));
        }

        [Test]
        public void StartWithCommandInArg()
        {
            var process = CreatePashProcess(@"Write-Host ""foo""");
            process.Start();
            var output = FinishProcess(process);
            // pash should only execute the command, show no banner and not
            // be in interactive mode. so the output should only contain the
            // command's output
            Assert.AreEqual("foo" + Environment.NewLine, output);
        }

        [Test]
        public void StartWithMultipleArgs()
        {
            var process = CreatePashProcess(@"Write-Host ""foo""", @"Write-Host ""bar""");
            process.Start();
            process.StandardInput.WriteLine(@"Write-Host ""interactiveInput"""); // should be ignored by pash process
            var output = FinishProcess(process);
            // pash should only execute the command, show no banner and not
            // be in interactive mode. so the output should only contain the
            // command's output
            Assert.AreEqual("foo" + Environment.NewLine + "bar" + Environment.NewLine, output);
        }

        [Test]
        public void StartWithArgsAndInteractiveInput()
        {
            var process = CreatePashProcess("-noexit", @"Write-Host ""foo""");
            process.Start();
            process.StandardInput.WriteLine(@"Write-Host ""interactiveInput"""); // should be ignored by pash process
            var output = FinishProcess(process);
            // pash should execute the provided args and what's coming from stdin
            // as it will also print a prompt, we won't do an exact comparison
            Assert.True(output.Contains("foo"));
            Assert.True(output.Contains("interactiveInput"));
            Assert.False(output.Contains(FullHost.BannerText));
        }
    }
}

