using System;
using NUnit.Framework;
using System.Diagnostics;
using System.Text;

namespace PashConsole.Tests.PashConsole
{
    public class PashConsoleTestHelper
    {
        public static string ExecutePash(string arguments, Action<Process, StringBuilder> waitFor = null)
        {
            StringBuilder output = new StringBuilder();
            using (var p = new System.Diagnostics.Process())
            {
                p.StartInfo = new ProcessStartInfo("mono", "Pash.exe " + arguments);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
                p.Start();
                p.BeginOutputReadLine();

                // if waitFor not specified - default to 10 seconds or the process has executed.
                if (waitFor == null)
                {
                    waitFor = (proccess, StringBuilder) =>
                    {
                        int i = 0;
                        while (++i < 10 && !p.HasExited)
                        {
                            // Evil!
                            System.Threading.Thread.Sleep(1000);
                        }
                    };
                }

                waitFor(p, output);
            }
            return output.ToString();
        }
    }
}

