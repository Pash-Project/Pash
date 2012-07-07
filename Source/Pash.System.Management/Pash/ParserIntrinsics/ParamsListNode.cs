using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Collections.Specialized;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    internal class ParamsListNode : NonTerminalNode
    {
        public StringCollection Params { get; private set; }

        public static ParamsListNode GetParamsList(Parser theParser)
        {
            ParamsListNode paramsList = null;

            if (theParser != null)
            {
                object objLeft = theParser.GetReductionSyntaxNode(0);
                object objRight = theParser.GetReductionSyntaxNode(1);

                if (objLeft is ParamsListNode)
                {
                    paramsList = (ParamsListNode)objLeft;
                    paramsList.AddParam(objRight);
                }
                else if (objRight is ParamsListNode)
                {
                    paramsList = (ParamsListNode)objRight;
                    paramsList.Insert(0, objLeft);
                }
            }

            if (paramsList == null)
            {
                paramsList = new ParamsListNode(theParser);

                if (theParser != null)
                {
                    paramsList.AddParamFromParser(theParser, 0);
                    paramsList.AddParamFromParser(theParser, 1);
                }
            }

            return paramsList;
        }

        public static ParamsListNode GetParamsListFromRight(Parser theParser)
        {
            object objRight = theParser.GetReductionSyntaxNode(1);

            ParamsListNode paramsList = null;
            if (objRight is ParamsListNode)
            {
                paramsList = (ParamsListNode)objRight;
            }
            else
            {
                paramsList = new ParamsListNode(theParser);
                paramsList.AddParamFromParser(theParser, 1);
            }

            return paramsList;
        }

        private ParamsListNode(Parser theParser)
            : base(theParser)
        {
            Params = new StringCollection();
        }

        private void AddParamFromParser(Parser theParser, int index)
        {
            AddParam(Token(theParser, index));
        }

        public void AddParam(object obj)
        {
            Params.Add(obj.ToString());
        }

        public void Insert(int index, object obj)
        {
            Params.Insert(0, obj.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<Params.Count; i++)
            {
                sb.Append(Params[i]);
                if (i+1 != Params.Count)
                    sb.Append(' ');
            }
            return sb.ToString();
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            throw new NotImplementedException();
        }
    }
}
