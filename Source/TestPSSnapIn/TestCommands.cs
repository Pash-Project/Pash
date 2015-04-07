using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;
using System.Runtime.InteropServices;

namespace TestPSSnapIn
{
    public class CustomTestClass
    {
        public string MessageProperty { get; set; }
        public string MessageField { get; set; }

        public CustomTestClass(string messageProperty, string messageField)
        {
            SetMessages(messageProperty, messageField);
        }

        public string Combine()
        {
            return MessageProperty + MessageField;
        }

        public void SetMessages(string messageProperty, string messageField)
        {
            MessageProperty = messageProperty;
            MessageField = messageField;
        }
    }

    public class InternalMessageClass
    {
        private string _msg;
        public InternalMessageClass(string msg)
        {
            _msg = msg;
        }

        public string GetMessage()
        {
            return _msg;
        }

        public static explicit operator InternalMessageClass(FooMessageClass fooObject)
        {
            return new InternalMessageClass("cast_" + fooObject.GetInternalMessage());
        }
    }

    public class FooMessageClass
    {
        public string _internalMsg;
        public InternalMessageClass Foo;

        public FooMessageClass(string msg)
        {
            Foo = new InternalMessageClass(msg);
        }

        public string GetInternalMessage()
        {
            return _internalMsg;
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "Command")]
    public class TestCommand : PSCmdlet
    {
        public static string OutputString = "works";


        protected override void ProcessRecord()
        {
            WriteObject(OutputString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "WithMandatory")]
    public class TestWithMandatoryCommand : PSCmdlet
    {
        public const string HELP_MSG = "Just provide some Message";

        [Parameter(Mandatory = true, HelpMessage = HELP_MSG)]
        public string OutputString { get; set; }

        public static string Transform(string value)
        {
            return ">" + value.ToUpper() + "<";
        }

        protected override void ProcessRecord()
        {
            WriteObject(Transform(OutputString));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoMandatories", DefaultParameterSetName = "Reversed")]
    public class TestNoMandatoriesCommand : PSCmdlet
    {
        [Parameter(Position = 0, ParameterSetName = "Correct")]
        [Parameter(Position = 2, ParameterSetName = "Reversed")]
        public string One { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Correct")]
        [Parameter(Position = 0, ParameterSetName = "Reversed")]
        public string Two { get; set; }

        [Parameter(ValueFromPipeline = true, ParameterSetName = "Correct")]
        public string RandomString;

        [Parameter(ValueFromPipeline = true, ParameterSetName = "Reversed")]
        public int RandomInt;

        protected override void ProcessRecord()
        {
            string setName = RandomString != null ? "Correct" : "Reversed";
            WriteObject(String.Format("{0}: {1} {2}", setName, One, Two));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "MandatoryInOneSet", DefaultParameterSetName = "Default")]
    public class TestMandatoryInOneSetCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Default", Position = 1, Mandatory = true)]
        [Parameter(ParameterSetName = "Alternative", Position = 1, Mandatory = false)]
        [Parameter(ParameterSetName = "Third", Position = 1, Mandatory = true)]
        public string TestParam { get; set; }

        [Parameter(ParameterSetName = "Third", Position = 2, Mandatory = true)]
        public string TestParam2 { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void ProcessRecord()
        {
            WriteObject(RandomString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoDefaultSet")]
    public class TestNoDefaultSetCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Default", Position = 1, Mandatory = false)]
        [Parameter(ParameterSetName = "Alternative", Position = 1, Mandatory = false)]
        public string TestParam { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void ProcessRecord()
        {
            WriteObject(RandomString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "CmdletPhases")]
    public class TestCmdletPhasesCommand : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void BeginProcessing()
        {
            WriteObject("Begin");
        }

        protected override void ProcessRecord()
        {
            WriteObject("Process");
        }

        protected override void EndProcessing()
        {
            WriteObject("End");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ThrowError")]
    public class TestThrowErrorCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            ThrowTerminatingError(new ErrorRecord(new Exception("testerror"), "TestError",
                                                  ErrorCategory.InvalidOperation, null));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "Dummy")]
    public class TestDummyCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoParameters")]
    public class TestNoParametersCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject("works");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "WriteNullObject")]
    public class TestWriteNullCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(null);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "CountProcessRecord")]
    public class TestCountProcessRecordCommand : PSCmdlet
    {
        private int _invocations = 0;

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public PSObject RandomObject;

        protected override void ProcessRecord()
        {
            _invocations++;
        }

        protected override void EndProcessing()
        {
            WriteObject(_invocations);
        }
    }


    [Cmdlet(VerbsDiagnostic.Test, "SwitchAndPositional")]
    public class TestSwitchAndPositionalCommand : PSCmdlet
    {
        [Parameter]
        public SwitchParameter Switch { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void ProcessRecord()
        {
            WriteObject(RandomString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "WriteTwoMessages")]
    public class TestWriteTwoMessagesCommand : PSCmdlet
    {
        [Parameter]
        public string Msg1 { get; set; }

        [Parameter]
        public string Msg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("1: " + Msg1 + ", 2: " + Msg2);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "SwitchParameter")]
    public class TestSwitchParameterCommand : PSCmdlet
    {
        [Parameter]
        public SwitchParameter Switch { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(Switch.IsPresent);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "SameAliases")]
    public class TestSameAliasesCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "One")]
        [Alias("arg")]
        public string Arg1 { get; set; }

        [Parameter(ParameterSetName = "Two")]
        [Alias("ARG")]
        public string Arg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("works");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "DefaultParameterSetDoesntExist", DefaultParameterSetName = "DoesntExist")]
    public class TestDefaultParameterSetDoesntExistCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "One", Mandatory = true)]
        public string Arg1 { get; set; }

        [Parameter(ParameterSetName = "Two", Mandatory = true)]
        public string Arg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("works");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoDefaultSetAndTwoPositionals")]
    public sealed class TestNoDefaultSetAndTwoPositionalsCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "First", Mandatory = true, Position = 0)]
        public string Arg1 { get; set; }

        [Parameter(ParameterSetName = "Second", Mandatory = true, Position = 0)]
        public string Arg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("1:" + Arg1 + ", 2:" + Arg2);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "DefaultSetIsAllParameterSetAndTwoParameterSets",
        DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
    public sealed class TestDefaultSetIsAllParameterSetAndTwoParameterSetsCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string First { get; set; }

        protected override void ProcessRecord()
        {
            if (First == null) {
                WriteObject("First: null");
            } else {
                WriteObject("First:" + First);
            }
        }
        
        [Parameter(Position = 1, ParameterSetName = "Set1")]
        [Parameter(Position = 1, ParameterSetName = "Set2")]
        [ValidateNotNullOrEmpty]
        public string Second { get; set; }

        [Parameter(ParameterSetName = "Set1")]
        public SwitchParameter ListAvailable { get; set; }
    }

    [Cmdlet(VerbsDiagnostic.Test, "TwoAmbiguousParameterSets")]
    public sealed class TestTwoAmbiguousParameterSetsCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string First { get; set; }

        protected override void ProcessRecord()
        {
            if (First == null) {
                WriteObject("First: null");
            } else {
                WriteObject("First:" + First);
            }
        }
        
        [Parameter(Position = 1, ParameterSetName = "Set1")]
        [Parameter(Position = 1, ParameterSetName = "Set2")]
        [ValidateNotNullOrEmpty]
        public string Second { get; set; }

        [Parameter(ParameterSetName = "Set1")]
        public SwitchParameter ListAvailable { get; set; }
    }
 
    [Cmdlet(VerbsDiagnostic.Test, "CreateCustomObject")]
    public class TestCreateCustomObjectCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string CustomMessageProperty { get; set; }

        [Parameter(Position = 1)]
        public string CustomMessageField { get; set; }


        protected override void ProcessRecord()
        {
            WriteObject(new CustomTestClass(CustomMessageProperty ?? "", CustomMessageField ?? ""));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ParametersByPipelinePropertyNames")]
    public class TestParametersByPipelinePropertyNamesCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1)]
        [Alias(new string[] { "Baz" })]
        public string Foo { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 0)]
        public string Bar { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(Foo + " " + Bar);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "CreateFooMessageObject")]
    public class TestCreateFooMessageObjectCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Msg { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(new FooMessageClass(Msg));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "WriteEnumerableToPipeline")]
    public sealed class TestWriteEnumerableToPipelineCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public int Start { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public int End { get; set; }

        protected override void EndProcessing()
        {
            WriteObject(Generate(Start, i => i + 1, End), true);
        }

        private static IEnumerable<int> Generate(int start, Func<int, int> next, int end)
        {
            for (var item = start; !object.Equals(item, end); item = next(item))
            {
                yield return item;
            }

            yield return end;
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ParametersByPipelineWithPropertiesAndConversion")]
    public class TestParametersByPipelineWithPropertiesAndConversionCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias(new string[] { "Baz" })]
        public InternalMessageClass Foo { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(Foo.GetMessage());
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ParameterUsesCustomTypeWithTypeConverter")]
    public sealed class TestParameterUsesCustomTypeWithTypeConverterCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public Custom CustomTypeParameter { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(string.Format("CustomType.Id='{0}'", CustomTypeParameter.Id));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ParameterInTwoSetsButNotDefault", DefaultParameterSetName = "Default")]
    public sealed class TestParameterInTwoSetsButNotDefaultCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Custom1")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Custom2")]
        public string Custom { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Custom2")]
        public string Custom2 { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Default")]
        public string DefParam { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(ParameterSetName);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "IntegerArraySum")]
    public sealed class TestIntegerArraySumCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public int[] IntArray { get; set; }

        public static string Transform(int[] arr)
        {
            int s = 0;
            foreach (var i in arr)
            {
                s += i;
            }
            return "l=" + arr.Length + ",s=" + s;
        }

        protected override void ProcessRecord()
        {
            WriteObject(Transform(IntArray));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ParamIsNotMandatoryByDefault")]
    public class TestParamIsNotMandatoryByDefaultCommand : PSCmdlet
    {
        [Parameter]
        public string Message { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(Message);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "PrintCredentials")]
    public class TestPrintCredentialsCommand : PSCmdlet
    {
        [Credential, Parameter(Mandatory = true, Position=0)]
        public PSCredential Credential { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("User: " + Credential.UserName);
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(Credential.Password);
                WriteObject("Password: " + Marshal.PtrToStringUni(unmanagedString));
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }

        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "OneMandatoryParamByPipelineSelection", DefaultParameterSetName = "Message")]
    public class TestOneMandatoryParamByPipelineSelectionCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Message")]
        public string Message { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Integer", ValueFromPipeline = true)]
        public int Integer { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(ParameterSetName);
        }

        protected override void ProcessRecord()
        {
            WriteObject(ParameterSetName);
        }
    }

    public class TestMandatoryParamByPipelineSelectionCommandBase : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Message", ValueFromPipeline = true)]
        public string Message { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Integer", ValueFromPipeline = true)]
        public int Integer { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(ParameterSetName);
        }

        protected override void ProcessRecord()
        {
            WriteObject(ParameterSetName);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "MandatoryParamByPipelineSelection", DefaultParameterSetName = "Message")]
    public class TestMandatoryParamByPipelineSelectionCommand :
        TestMandatoryParamByPipelineSelectionCommandBase {}

    [Cmdlet(VerbsDiagnostic.Test, "MandatoryParamByPipelineSelectionWithoutDefault")]
    public class TestMandatoryParamByPipelineSelectionWithoutDefaultCommand :
                 TestMandatoryParamByPipelineSelectionCommandBase {}

    [TypeConverter(typeof(CustomTypeConverter))]
    public class Custom
    {
        public string Id { get; set; }
    }

    public class CustomTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            return new Custom { Id = stringValue };
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ContentWriter", DefaultParameterSetName = "Path")]
    public class TestContentWriter : WriteContentCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (string path in Path)
            {
                using (IContentWriter writer = InvokeProvider.Content.GetWriter(path).Single())
                {
                    IList items = writer.Write(Value);
                    // Need to close writer before disposing it otherwise Microsoft's
                    // FileSystemProvider throws an exception.
                    writer.Close();
                }
            }
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ContentReader", DefaultParameterSetName = "Path")]
    public class TestContentReader : ContentCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (string path in Path)
            {
                using (IContentReader reader = InvokeProvider.Content.GetReader(path).Single())
                {
                    while (true)
                    {
                        IList items = reader.Read(1);
                        if (items.Count > 0)
                        {
                            WriteObject(items[0]);
                        }
                        else
                        {
                            reader.Close();
                            break;
                        }
                    }
                }
            }
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ParametersByPositionWhenOneBoundByName")]
    public class TestParametersByPositionWhenOneBoundByName : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0)]
        public string First { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true)]
        public string Second { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true)]
        public string Third { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = true)]
        public string Fourth { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(string.Format("'{0}', '{1}', '{2}', '{3}'", First, Second, Third, Fourth));
        }
    }
}
