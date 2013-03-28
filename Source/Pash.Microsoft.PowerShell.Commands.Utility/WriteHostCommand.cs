// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Collections;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Write-Host
    /// 
    /// DESCRIPTION
    ///   Write-Host displays objects as text on the console. You can change the foreground and background colors of the displayed text among other options.
    /// 
    /// RELATED PASH COMMANDS
    ///   Write-Error
    ///   Write-Debug
    ///   
    /// RELATED POSIX COMMANDS
    ///   echo 
    /// </summary>
    [Cmdlet("Write", "Host")]
    public class WriteHostCommand : PSCmdlet
    {
        private bool bgcolorset;
        private ConsoleColor bgcolor;

        private bool fgcolorset;
        private ConsoleColor fgcolor;

        protected override void ProcessRecord()
        {
            String _text = Object as String;

            if (String.IsNullOrEmpty(_text))
                BreakObjectApart(Object);
            
            else Host.UI.Write(ForegroundColor, BackgroundColor, _text);

            if (!NoNewline.ToBool())
                Host.UI.WriteLine(ForegroundColor, BackgroundColor, String.Empty);
        }


        // BreakObjectApart takes in a object hierarchy of unlimited depth and breaks it 
        // apart into individual non-enumerable objects before displaying them on the screen.
        private void BreakObjectApart(Object hierarchy)
        {
            IEnumerable _array = hierarchy as IEnumerable;

            if (_array == null)
            {
                Host.UI.Write(ForegroundColor, BackgroundColor, _array.ToString());
                return;
            }
            
            bool _first = false;

            foreach (IEnumerable _obj in _array)
            {
                if ((Separator != null) && _first)
                    Host.UI.Write(ForegroundColor, BackgroundColor, Separator.ToString());

                else if (_first) Host.UI.Write(ForegroundColor, BackgroundColor, " ");

                String _text = _obj as String;

                if (!String.IsNullOrEmpty(_text))
                    Host.UI.Write(ForegroundColor, BackgroundColor, _text);

                else BreakObjectApart(_obj);

                _first = true;
            }
        }

        /// <summary>
        /// Do not create a newline at the end of the output.
        /// </summary>
        [Parameter]
        public SwitchParameter NoNewline { get; set; }

        /// <summary>
        /// The object or objects to be displayed on the console as text.
        /// </summary>
        [Parameter(
            Position = 0, 
            ValueFromRemainingArguments = true, 
            ValueFromPipeline = true)]
        public object Object { get; set; }

        /// <summary>
        /// If passing in multiple objects, you may use a seperator object to display between them.
        /// </summary>
        [Parameter]
        public object Separator { get; set; }

        /// <summary>
        /// A background color that you wish to use for the output.
        /// </summary>
        [Parameter]
        public ConsoleColor BackgroundColor
        {
            get
            {
                if (bgcolorset)
                    return bgcolor;

                else 
                    return Host.UI.RawUI.BackgroundColor;
            }
            set
            {
                bgcolorset = true;
                bgcolor = value;
            }
        }

        /// <summary>
        /// A foreground color that you wish to use for the output.
        /// </summary>
        [Parameter]
        public ConsoleColor ForegroundColor
        {
            get
            {
                if (fgcolorset)
                    return fgcolor;

                else 
                    return Host.UI.RawUI.ForegroundColor;
            }
            set
            {
                fgcolorset = true;
                fgcolor = value;
            }
        }
    }
}
