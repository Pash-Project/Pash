using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    public class InvocationInfo
    {
        public string InvocationName { get { throw new NotImplementedException(); } }
        public string Line { get { throw new NotImplementedException(); } }
        public CommandInfo MyCommand { get { throw new NotImplementedException(); } }
        public int OffsetInLine { get { throw new NotImplementedException(); } }
        public int PipelineLength { get { throw new NotImplementedException(); } }
        public int PipelinePosition { get { throw new NotImplementedException(); } }
        public string PositionMessage { get { throw new NotImplementedException(); } }
        public int ScriptLineNumber { get { throw new NotImplementedException(); } }
        public string ScriptName { get { throw new NotImplementedException(); } }

        // internals
        //internal InvocationInfo(System.Management.Automation.CommandInfo commandInfo, System.Management.Automation.Token scriptToken, System.Management.Automation.Token parameterToken);
        //internal InvocationInfo(System.Management.Automation.CommandInfo commandInfo, System.Management.Automation.Token token);
        //internal InvocationInfo(System.Management.Automation.Internal.InternalCommand command);
        //internal System.Management.Automation.Token ScriptToken { set; get; }
        //internal System.Management.Automation.Token ParameterToken { set; get; }
        //internal int _pipelineLength;
        //internal int _pipelinePosition;
    }
}
