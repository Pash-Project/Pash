using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

namespace Pash.Implementation
{
    internal class ObjectStream
    {
        // TODO: implement ObjectStream
        private ArrayList _objectsStream;

        internal ObjectStream()
        {
            _objectsStream = new ArrayList();
        }

        internal ObjectStream(IEnumerable input)
            : this()
        {
            foreach (object obj in input)
                _objectsStream.Add(obj);
        }

        internal void Write(object obj)
        {
            _objectsStream.Add(obj);
        }

        internal Collection<object> Read()
        {
            // TODO: (code duplication) does it makes sense to call the Read(count) with "everyting" as a value?

            Collection<object> c = new Collection<object>();

            // nothing to read
            if (_objectsStream.Count < 0)
                return c;

            foreach (object obj in _objectsStream)
            {
                c.Add(obj);
            }

            _objectsStream.Clear();

            return c;
        }

        internal Collection<object> Read(int count)
        {
            Collection<object> c = new Collection<object>();

            // nothing to read
            if (_objectsStream.Count < 0)
                return c;

            // TODO: what should be done if requested more than it's available?
            if (count > _objectsStream.Count)
                count = _objectsStream.Count;

            for(int i=0; i<count; i++)
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
    }
}
