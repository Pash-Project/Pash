// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Management.Automation
{
    internal sealed class PSDataCollectionEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
    {
        private T currentElement;
        private int index;
        private PSDataCollection<T> collToEnumerate;

        T IEnumerator<T>.Current
        {
            get
            {
                return this.currentElement;
            }
        }

        public object Current
        {
            get
            {
                return this.currentElement;
            }
        }

        internal PSDataCollectionEnumerator(PSDataCollection<T> collection)
        {
            this.collToEnumerate = collection;
            this.index = 0;
            this.currentElement = default(T);
            this.collToEnumerate.IsEnumerated = true;
        }

        public bool MoveNext()
        {
            bool result;
            try
            {
                while (this.index >= this.collToEnumerate.Count)
                {
                    if (!this.collToEnumerate.IsOpen)
                    {
                        result = false;
                        return result;
                    }
                }
                this.currentElement = this.collToEnumerate[this.index];
                if (this.collToEnumerate.ReleaseOnEnumeration)
                {
                    this.collToEnumerate[this.index] = default(T);
                }
                this.index++;
                result = true;
            }
            finally
            {

            }
            return result;
        }

        public void Reset()
        {
            this.currentElement = default(T);
            this.index = 0;
        }
        void IDisposable.Dispose()
        {
        }
    }
}
