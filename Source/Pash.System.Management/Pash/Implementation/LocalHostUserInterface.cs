// Copyright (C) Pash Contributors. All Rights Reserved. See https://github.com/Pash-Project/Pash/

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
using System.Text;
using System.Management.Automation.Host;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Pash.Implementation
{
    class LocalHostUserInterface : PSHostUserInterface
    {
        #region User prompt Methods
        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            Console.WriteLine();
            Console.WriteLine(caption);
            Console.WriteLine(message);

            // stop-service TermService -confirm
            //[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"):
            List<char> chs = new List<char>();
            foreach (var cd in choices)
            {
                string s = cd.Label.ToUpper();
                int z = s.IndexOf('&');

                if (z != -1)
                {
                    char chChoice = s[z + 1];
                    chs.Add(chChoice);
                    Console.Write("[{0}] ", chChoice);
                }
                Console.Write(cd.Label.Replace("&", "") + "  ");
            }
            Console.Write("[?] Help (default is \"{0}\"): ", chs[defaultChoice]);
            string str = Console.ReadLine().ToUpper();
            if (str == "?")
            {
                // TODO: implement help
                /*
                Y - Continue with only the next step of the operation.
                A - Continue with all the steps of the operation.
                N - Skip this operation and proceed with the next operation.
                L - Skip this operation and all subsequent operations.
                S - Pause the current pipeline and return to the command prompt. Type "exit" to resume the pipeline.
                */
            }
            int i = defaultChoice;
            if (Int32.TryParse(str, out i))
                return i;
            return defaultChoice;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region LowLevel UI interface
        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return new LocalHostRawUserInterface();
                // TODO: why is it soo slow with the default Raw UI?
                //return null;
            }
        }
        #endregion

        #region ReadXXX methods
        public override string ReadLine()
        {
            return Console.ReadLine();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region WriteXXX methods
        public override void Write(string value)
        {
            Console.Write(value);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            // Save colors
            ConsoleColor backColor = Console.BackgroundColor;
            ConsoleColor foreColor = Console.ForegroundColor;

            // Set colors
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            // Just ignore the colors...
            Console.Write(value);

            // Restore colors
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;
        }

        public override void WriteDebugLine(string message)
        {
            WriteLine(String.Format("DEBUG: {0}", message));
        }

        public override void WriteErrorLine(string value)
        {
            WriteLine(ConsoleColor.Red, ConsoleColor.Black, String.Format("ERROR: {0}", value));
        }

        public override void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            ; // Do nothing...
        }

        public override void WriteVerboseLine(string message)
        {
            Console.WriteLine(String.Format("VERBOSE: {0}", message));
        }

        public override void WriteWarningLine(string message)
        {
            Console.WriteLine(String.Format("WARNING: {0}", message));
        }

        public override void WriteLine()
        {
            Console.WriteLine();
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            // Save colors
            ConsoleColor foreColor = Console.ForegroundColor;
            ConsoleColor backColor = Console.BackgroundColor;

            // Set colors
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            // Just ignore the colors...
            Console.WriteLine(value);

            // Restore colors
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;
        }
        #endregion
    }
}
