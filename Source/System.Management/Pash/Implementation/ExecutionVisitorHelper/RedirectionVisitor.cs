using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace System.Management.Pash.Implementation
{
    class RedirectionVisitor
    {
        ExecutionVisitor _visitor;
        List<RedirectionAst> _redirections;

        public RedirectionVisitor(ExecutionVisitor visitor, IEnumerable<RedirectionAst> redirections)
        {
            _visitor = visitor;
            _redirections = redirections.ToList();
        }

        public void Visit(PipelineCommandRuntime runtime)
        {
            foreach (RedirectionAst redirectionAst in _redirections)
            {
                var fileRedirectionAst = redirectionAst as FileRedirectionAst;
                if (fileRedirectionAst != null)
                {
                    var redirectionVisitor = new FileRedirectionVisitor(_visitor, runtime);
                    redirectionVisitor.Visit(fileRedirectionAst);
                }
                else
                {
                    throw new NotImplementedException(redirectionAst.ToString());
                }
            }
        }
    }
}
