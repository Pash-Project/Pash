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
    internal class PSObjectPipelineReader : PipelineReader<PSObject>
    {
        ObjectStream _stream;

        public PSObjectPipelineReader(ObjectStream stream)
            : base()
        {
            _stream = stream;
        }

        public PSObjectPipelineReader(IEnumerable input)
            : base()
        {
            _stream = new ObjectStream(input);
        }

        public override int Count
        {
            get { return _stream.Count; }
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

        public override Collection<PSObject> NonBlockingRead()
        {
            return GetPSCollection(_stream.NonBlockingRead());
        }

        public override Collection<PSObject> NonBlockingRead(int maxRequested)
        {
            return GetPSCollection(_stream.NonBlockingRead(maxRequested));
        }

        public override PSObject Peek()
        {
            throw new NotImplementedException();
        }

        public override PSObject Read()
        {
            var items = _stream.Read(1);
            if (items.Count == 0)
                return null;
            return PSObject.AsPSObject(items[0]);
        }

        public override Collection<PSObject> Read(int count)
        {
            return GetPSCollection(_stream.Read(count));
        }

        public override Collection<PSObject> ReadToEnd()
        {
            return GetPSCollection(_stream.Read());
        }

        // internals
        private Collection<PSObject> GetPSCollection(Collection<object> collection)
        {
            if (collection == null)
            {
                return null;
            }
            Collection<PSObject> colRet = new Collection<PSObject>();
            foreach (object obj in collection)
            {
                colRet.Add(PSObject.AsPSObject(obj));
            }
            return colRet;
        }

        public override string ToString()
        {
            return "Reader: Count=" + Count.ToString();
        }
    }
}
