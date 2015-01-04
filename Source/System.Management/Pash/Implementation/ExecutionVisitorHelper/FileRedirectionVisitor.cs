using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace System.Management.Pash.Implementation
{
    class FileRedirectionVisitor
    {
        ExecutionVisitor _visitor;
        PipelineCommandRuntime _runtime;

        public FileRedirectionVisitor(ExecutionVisitor visitor, PipelineCommandRuntime runtime)
        {
            _visitor = visitor;
            _runtime = runtime;
        }

        public AstVisitAction Visit(FileRedirectionAst redirectionAst)
        {
            string outputPath = _visitor.EvaluateAst(redirectionAst.Location, false).ToString();

            FileMode fileMode = GetFileMode(redirectionAst);
            FileStream file = File.Open(outputPath, fileMode, FileAccess.Write);
            using (var streamWriter = new StreamWriter(file, Encoding.Unicode))
            {
                foreach (object o in _runtime.OutputStream.Read())
                {
                    var psObject = PSObject.AsPSObject(o);
                    streamWriter.WriteLine(psObject.ToString());
                }
            }
            return AstVisitAction.SkipChildren;
        }

        private FileMode GetFileMode(FileRedirectionAst redirectionAst)
        {
            if (redirectionAst.Append)
            {
                return FileMode.Append;
            }
            return FileMode.Create;
        }
    }
}
