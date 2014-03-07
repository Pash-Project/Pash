// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        // unused - NYI
        public override event EventHandler DataReady = delegate { };

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override Collection<object> NonBlockingRead()
        {
            return _stream.Read();
        }

        public override Collection<object> NonBlockingRead(int maxRequested)
        {
            return _stream.Read(maxRequested);
        }

        public override object Peek()
        {
            throw new NotImplementedException();
        }

        public override object Read()
        {
            return _stream.Read();
        }

        public override Collection<object> Read(int count)
        {
            return _stream.Read(count);
        }

        public override Collection<object> ReadToEnd()
        {
            return _stream.Read();
        }

        public override string ToString()
        {
            return "Reader: Count=" + Count.ToString();
        }
    }
}
