// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Mono.Terminal;
using System.IO;
using Extensions.Reflection;
using System.Security;

namespace Pash.Implementation
{
    public class LocalHostUserInterface : PSHostUserInterface
    {
        private bool _useUnixLikeInput;
        private LineEditor _getlineEditor;
        private LocalHost _parentHost;
        private TabExpansionProvider _tabExpansionProvider;

        public bool InteractiveIO { get; set; }

        public bool UseUnixLikeInput {
            get
            {
                return _useUnixLikeInput;
            }

            set
            {
                if (value != _useUnixLikeInput)
                {
                    _useUnixLikeInput = value;
                    if (_useUnixLikeInput)
                    {
                        _getlineEditor = new LineEditor("Pash");
                        InitTabExpansion();
                    }
                    else
                    {
                        _getlineEditor = null;
                    }
                }
            }
        }

        public LocalHostUserInterface(LocalHost parent, bool interactiveIO)
        {
            _parentHost = parent;
            UseUnixLikeInput = Environment.OSVersion.Platform != System.PlatformID.Win32NT && Console.WindowWidth > 0;
            InteractiveIO = interactiveIO;

            // Set up the control-C handler.
            try
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleControlC);
                Console.TreatControlCAsInput = false;
            }
            catch (IOException)
            {
                // don't mind. if it doesn't work we're likely in a condition where stdin/stdout was redirected
            }
        }

        protected LocalHostUserInterface()
        {
            UseUnixLikeInput = false;
            _parentHost = null;
            InteractiveIO = false;
        }

        public void InitTabExpansion()
        {
            if (_getlineEditor == null)
            {
                return;
            }
            var localRunspace = _parentHost == null ? null : _parentHost.OpenRunspace as LocalRunspace;
            _tabExpansionProvider = new TabExpansionProvider(localRunspace);
            _getlineEditor.SetTabExpansionFunction(_tabExpansionProvider.GetAllExpansions);
        }

        #region Private stuff
        private void HandleControlC(object sender, ConsoleCancelEventArgs e)
        {
            try
            {
                var runningPipeline = _parentHost.OpenRunspace.GetCurrentlyRunningPipeline();
                if (runningPipeline != null)
                {
                    runningPipeline.Stop();
                }
                e.Cancel = true;
            }
            catch (Exception exception)
            {
                WriteErrorLine(exception.ToString());
            }
        }

        private void ThrowNotInteractiveException()
        {
            throw new HostException("No interactive I/O is available to read data");
        }

        private object PromptValue(string label, Type type, Collection<Attribute> attributes, string helpMessage)
        {
            if (type == typeof(PSCredential))
            {
                return PromptCredential(label, attributes, helpMessage);
            }
            if (type.IsArray)
            {
                List<object> values = new List<object>();
                while (true)
                {
                    var elType = type.GetElementType();
                    var val = PromptValue(String.Format("{0}[{1}]", label, values.Count), elType,
                                          attributes, helpMessage);
                    if (val == null || (elType == typeof(string) && String.IsNullOrEmpty((string) val)))
                    {
                        break;
                    }
                    values.Add(val);
                }
                return values.ToArray();
            }
            Write(label + ":");
            // TODO: What about EOF here? I guess we'd need to stop the pipeline? Verify this before implementing
            if (type == typeof(System.Security.SecureString))
            {
                return ReadLineAsSecureString();
            }
            string value = ReadLine(false);
            if (value != null && value.Equals("!?"))
            {
                var msg = String.IsNullOrEmpty(helpMessage) ? "No help message provided for " + label : helpMessage;
                WriteLine(msg);
                // simply prompt again
                return PromptValue(label, type, attributes, helpMessage);
            }
            object converted;
            if (!(type.IsNumeric() && String.IsNullOrWhiteSpace(value)) &&
                LanguagePrimitives.TryConvertTo(value, type, out converted))
            {
                // TODO: we should validate the value with the defined Attributes here
                return converted;
            }
            return null;
        }

        private PSCredential PromptCredential(string label, Collection<Attribute> attributes, string helpMessage)
        {
            var user = PromptValue(label + " (UserName)", typeof(string), new Collection<Attribute>(),
                                   helpMessage) as string;
            var pw = PromptValue(label + " (Password)", typeof(SecureString),
                                 new Collection<Attribute>(), helpMessage) as SecureString;
            if (user == null || pw == null)
            {
                return null;
            }
            return new PSCredential(user, pw);
        }

        // workaround as long as getline.cs might care about history on unix
        internal virtual string ReadLine(bool addToHistory)
        {
            if (!InteractiveIO)
            {
                ThrowNotInteractiveException();
            }
            return UseUnixLikeInput ? _getlineEditor.Edit("", "", addToHistory) : Console.ReadLine();
        }
        #endregion

        #region User prompt Methods
        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            if (!InteractiveIO)
            {
                ThrowNotInteractiveException();
            }
            WriteLine(caption);
            WriteLine(message);
            var returnValues = new Dictionary<string, PSObject>();
            foreach (var descr in descriptions)
            {
                var value = PromptValue(descr.Label, descr.ParameterType, descr.Attributes, descr.HelpMessage);
                returnValues[descr.Name] = value == null ? descr.DefaultValue : PSObject.AsPSObject(value);
            }
            return returnValues;
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (!InteractiveIO)
            {
                ThrowNotInteractiveException();
            }
            WriteLine();
            WriteLine(caption);
            WriteLine(message);

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
                    Write(String.Format("[{0}] ", chChoice));
                }
                Write(cd.Label.Replace("&", "") + "  ");
            }
            Write(String.Format("[?] Help (default is \"{0}\"): ", chs[defaultChoice]));
            string str = ReadLine().ToUpper();
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
            if (!InteractiveIO)
            {
                ThrowNotInteractiveException();
            }
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            if (!InteractiveIO)
            {
                ThrowNotInteractiveException();
            }
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
            return ReadLine(false);
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (!InteractiveIO)
            {
                ThrowNotInteractiveException();
            }
            var ssReader = new SecureStringReader();
            return ssReader.ReadLine();
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
            // TODO: unfortunately, it doesn't seem to work to print with colors to stderr
            //       so let's stay with stdout
            // var origOut = Console.Out;
            // Console.SetOut(Console.Error);
            WriteLine(ConsoleColor.Red, ConsoleColor.Black, value);
            // Console.SetOut(origOut);
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
            WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, String.Format("VERBOSE: {0}", message));
        }

        public override void WriteWarningLine(string message)
        {
            WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, String.Format("WARNING: {0}", message));
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
