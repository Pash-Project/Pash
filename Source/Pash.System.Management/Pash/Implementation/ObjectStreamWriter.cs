using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Management.Automation.Runspaces;

namespace Pash.Implementation
{
    class ObjectStreamWriter : PipelineWriter
    {
        ObjectStream _stream;

        public ObjectStreamWriter(ObjectStream stream)
        {
            _stream = stream;
        }

        public override int Count
        {
            get { return _stream.Count; }
        }

        public override bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxCapacity
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Threading.WaitHandle WaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Write(object obj)
        {
            _stream.Write(obj);

            return 1;
        }

        public override int Write(object obj, bool enumerateCollection)
        {
            int numWritten = 0;
            if (!enumerateCollection)
            {
                Write(obj);
                numWritten = 1;
            }
            else
            {
                IEnumerator enumerator = GetEnumerator(obj);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        Write(enumerator.Current);
                        numWritten++;
                    }
                }
                else
                {
                    Write(obj);
                    numWritten = 1;
                }
            }

            return numWritten;
        }

        private IEnumerator GetEnumerator(object obj)
        {
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            IEnumerator enumerator = obj as IEnumerator;
            if (obj != null)
            {
                return enumerator;
            }

            return null;
        }

        public override string ToString()
        {
            return "Writer: Count=" + Count.ToString();
        }
    }
}
