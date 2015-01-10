using System;
using System.Collections;
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
            string outputPath = GetOutputFileName(redirectionAst);
            if (outputPath == null)
            {
                return AstVisitAction.SkipChildren;
            }

            FileMode fileMode = GetFileMode(redirectionAst);
            FileStream file = File.Open(outputPath, fileMode, FileAccess.Write);
            using (var streamWriter = new StreamWriter(file, Encoding.Unicode))
            {
                foreach (object obj in _runtime.OutputStream.Read())
                {
                    WriteObject(streamWriter, obj);
                }
            }
            return AstVisitAction.SkipChildren;
        }

        private string GetOutputFileName(FileRedirectionAst redirectionAst)
        {
            object outputPath = _visitor.EvaluateAst(redirectionAst.Location, false);
            if (outputPath != null)
            {
                return outputPath.ToString();
            }
            return null;
        }

        private FileMode GetFileMode(FileRedirectionAst redirectionAst)
        {
            if (redirectionAst.Append)
            {
                return FileMode.Append;
            }
            return FileMode.Create;
        }

        private void WriteObject(StreamWriter writer, object obj)
        {
            IEnumerator enumerable = LanguagePrimitives.GetEnumerator(obj);
            if (enumerable != null)
            {
                WriteEnumerable(writer, enumerable);
            }
            else
            {
                WritePSObject(writer, obj);
            }
        }

        private void WritePSObject(StreamWriter writer, object obj)
        {
            var psObject = PSObject.AsPSObject(obj);
            writer.WriteLine(psObject.ToString());
        }

        private void WriteEnumerable(StreamWriter writer, IEnumerator enumerable)
        {
            while (enumerable.MoveNext())
            {
                WritePSObject(writer, enumerable.Current);
            }
        }
    }
}
