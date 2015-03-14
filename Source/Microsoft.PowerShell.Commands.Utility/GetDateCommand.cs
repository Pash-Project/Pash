// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Get-Date
    /// 
    /// DESCRIPTION
    ///   Retrieves the current date and time and contains other date and time related functionality.
    ///   
    /// RELATED POSIX COMMANDS
    ///   date 
    /// </summary>
    [Cmdlet("Get", "Date", DefaultParameterSetName = "net")]
    [OutputType(typeof(string), typeof(DateTime))]
    public sealed class GetDateCommand : Cmdlet
    {
        DateTime date;
        bool dateIsSet;

        protected override void ProcessRecord()
        {
            DateTime _date;

            if (dateIsSet)
                _date = Date;

            else _date = DateTime.Now;

            if (Year != 0)
                _date = _date.AddYears(Year - _date.Year);

            if (Month != 0)
                _date = _date.AddMonths(Month - _date.Month);

            if (Day != 0)
                _date = _date.AddDays(Day - _date.Day);

            if (Hour != 0)
                _date = _date.AddHours(Hour - _date.Hour);

            if (Minute != 0)
                _date = _date.AddMinutes(Year - _date.Minute);

            if (Second != 0)
                _date = _date.AddSeconds(Year - _date.Second);

            WriteObject(_date);
        }

        /// <summary>
        /// A date (DateTime) object to pass through the cmdlet.  
        /// </summary>
        [Alias(new string[] { "LastWriteTime" }),
        Parameter(
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;

                // Workaround because Date is never NULL
                dateIsSet = true;
            }
        }

        /// <summary>
        /// The year of the date that will be returned.  
        /// </summary>
        [Parameter, ValidateRange(1, 0x270f)]
        public int Year { get; set; }

        /// <summary>
        /// The month of the date that will be returned.  
        /// </summary>
        [Parameter, ValidateRange(1, 12)]
        public int Month { get; set; }

        /// <summary>
        /// The day of the date that will be returned.  
        /// </summary>
        [Parameter, ValidateRange(1, 0x1f)]
        public int Day { get; set; }

        /// <summary>
        /// The hour of the date that will be returned.  
        /// </summary>
        [Parameter, ValidateRange(0, 0x17)]
        public int Hour { get; set; }

        /// <summary>
        /// The minute of the date that will be returned.  
        /// </summary>
        [Parameter, ValidateRange(0, 0x3b)]
        public int Minute { get; set; }

        /// <summary>
        /// The second of the date that will be returned.  
        /// </summary>
        [Parameter, ValidateRange(0, 0x3b)]
        public int Second { get; set; }

    }
}
