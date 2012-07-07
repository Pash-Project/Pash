using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal class ObjectPipelineReader : PipelineReader<object>
    {
        ObjectStream _stream;

        public ObjectPipelineReader(ObjectStream stream)
            : base()
        {
            _stream = stream;
        }

        public ObjectPipelineReader(IEnumerable input)
            : base()
        {
            _stream = new ObjectStream(input);
        }

        public override int Count
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EndOfPipeline
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxCapacity
        {
            get { throw new NotImplementedException(); }
        }

        public override WaitHandle WaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override event EventHandler DataReady;

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override Collection<object> NonBlockingRead()
        {
            Collection<object> c = new Collection<object>();

            throw new NotImplementedException();
        }

        public override Collection<object> NonBlockingRead(int maxRequested)
        {
            throw new NotImplementedException();
        }

        public override object Peek()
        {
            throw new NotImplementedException();
        }

        public override object Read()
        {
            throw new NotImplementedException();
        }

        public override Collection<object> Read(int count)
        {
            throw new NotImplementedException();
        }

        public override Collection<object> ReadToEnd()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Reader: Count=" + Count.ToString();
        }
    }
}
