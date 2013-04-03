// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/


//todo: Handle RecommendedAction

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Write-Error
    /// 
    /// DESCRIPTION
    ///   Record or display an error by writing it to the error stream.
    /// 
    /// RELATED PASH COMMANDS
    ///   Write-Error
    ///   Write-Debug
    ///   
    /// RELATED POSIX COMMANDS
    ///   echo 
    /// </summary>
    [Cmdlet("Write", "Error", DefaultParameterSetName = "NoException")]

    public class WriteErrorCommand : PSCmdlet
    {

        protected override void ProcessRecord()
        {
            ErrorRecord _error;

            if (ErrorRecord != null)
            {
                _error = ErrorRecord;
            }

            else
            {
                Exception _exception;

                if (Exception != null)
                    _exception = Exception;

                else
                    _exception = new SystemException(Message);

                if (String.IsNullOrEmpty(ErrorId))
                    _error = new ErrorRecord(_exception, _exception.GetType().FullName, Category, TargetObject);

                else
                    _error = new ErrorRecord(_exception, ErrorId, Category, TargetObject);

                //if (!String.IsNullOrEmpty(RecommendedAction))
                //   _error.ErrorDetails.RecommendedAction = RecommendedAction;
            }

            if (!String.IsNullOrEmpty(CategoryTargetType))
                _error.CategoryInfo.TargetType = CategoryTargetType;

            if (!String.IsNullOrEmpty(CategoryReason))
                _error.CategoryInfo.Reason = CategoryReason;

            if (!String.IsNullOrEmpty(CategoryActivity))
                _error.CategoryInfo.Activity = CategoryActivity;

            if (!String.IsNullOrEmpty(CategoryTargetName))
                _error.CategoryInfo.TargetName = CategoryTargetName;

            WriteError(_error);

        }

        /// <summary>
        /// The category of the error.
        /// </summary>
        [Parameter(ParameterSetName = "NoException"), Parameter(ParameterSetName = "WithException")]
        public ErrorCategory Category { get; set; }

        /// <summary>
        /// The activity of the category.
        /// </summary>
        [Parameter, Alias(new string[] { "Activity" })]
        public string CategoryActivity { get; set; }

        /// <summary>
        /// The reason for being in that category.
        /// </summary>
        [Parameter, Alias(new string[] { "Reason" })]
        public string CategoryReason { get; set; }

        /// <summary>
        /// The name of the category target.
        /// </summary>
        [Parameter, Alias(new string[] { "TargetName" })]
        public string CategoryTargetName { get; set; }

        /// <summary>
        /// The type of the category target.
        /// </summary>
        [Parameter,
        Alias(new string[] { "TargetType" })]
        public string CategoryTargetType { get; set; }

        /// <summary>
        /// A unique string to identify this error.
        /// </summary>
        [Parameter(ParameterSetName = "WithException"),
        Parameter(ParameterSetName = "NoException")]
        public string ErrorId { get; set; }

        /// <summary>
        /// A Pash error record object to use.
        /// </summary>
        [Parameter(ParameterSetName = "ErrorRecord", Mandatory = true)]
        public ErrorRecord ErrorRecord { get; set; }

        /// <summary>
        /// An exception to use.
        /// </summary>
        [Parameter(ParameterSetName = "WithException", Mandatory = true)]
        public Exception Exception { get; set; }

        /// <summary>
        /// The error message. Typically displayed in the host.
        /// </summary>
        [Parameter(ParameterSetName = "WithException"),
        AllowNull,
        AllowEmptyString,
        Parameter(Position = 0,
            ParameterSetName = "NoException",
            Mandatory = true,
            ValueFromPipeline = true)]
        public string Message { get; set; }

        /// <summary>
        /// The recommended response to this error.
        /// </summary>
        [Parameter]
        public string RecommendedAction { get; set; }

        /// <summary>
        /// An object that is related to the error.
        /// </summary>
        [Parameter(ParameterSetName = "NoException"), Parameter(ParameterSetName = "WithException")]
        public object TargetObject { get; set; }

    }
}
