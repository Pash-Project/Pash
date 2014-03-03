// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal class ObjectStream
    {
        // TODO: implement this in a way that we can call a ProcessRecord method if an element is written
        // and the stream is claimed by a specific command (as input). In this way, we implement the "process record
        // as soon as an object is available" specification
        private ArrayList _objectsStream;
        public object Owner { get; private set; }
        public object ClaimedBy { get; private set; }

        internal ObjectStream(object owner)
        {
            Owner = owner;
            _objectsStream = new ArrayList();
            ClaimedBy = null;
        }

        internal ObjectStream(object owner, IEnumerable input)
            : this(owner)
        {
            foreach (object obj in input)
                _objectsStream.Add(obj);
        }

        public void Redirect(ObjectStream redirection)
        {
            _objectsStream = redirection._objectsStream;
            ClaimedBy = redirection.Owner;
        }

        internal void Write(object obj)
        {
            _objectsStream.Add(obj);
        }

        internal Collection<object> Read()
        {
            return Read(null);
        }

        internal Collection<object> Read(int count)
        {
            return Read(count);
        }

        internal Collection<object> Read(int? optCount)
        {
            Collection<object> c = new Collection<object>();

            // nothing to read
            if (_objectsStream.Count < 0)
                return c;

            int count = optCount ?? _objectsStream.Count;
            // TODO: what should be done if requested more than it's available?
            if (count > _objectsStream.Count)
                count = _objectsStream.Count;

            for (int i = 0; i < count; i++)
            {
                object obj = _objectsStream[i];
                c.Add(obj);
            }
            _objectsStream.RemoveRange(0, count);

            return c;
        }

        internal Collection<object> NonBlockingRead()
        {
            return Read();
        }

        internal Collection<object> NonBlockingRead(int maxRequested)
        {
            return Read(maxRequested);
        }

        internal int Count
        {
            get
            {
                return _objectsStream.Count;
            }
        }

        public override string ToString()
        {
            return "Count: " + this.Count;
        }
    }
}
