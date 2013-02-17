// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public sealed class PSPrimitiveDictionary : Hashtable
    {
        private static readonly Type[] handshakeFriendlyTypes = new Type[]
		{
			typeof(bool),
			typeof(byte),
			typeof(char),
			typeof(DateTime),
			typeof(decimal),
			typeof(double),
			typeof(Guid),
			typeof(int),
			typeof(long),
			typeof(sbyte),
			typeof(float),
			typeof(string),
			typeof(TimeSpan),
			typeof(ushort),
			typeof(uint),
			typeof(ulong),
			typeof(Uri),
			typeof(byte[]),
			typeof(Version),
			typeof(ProgressRecord),
			typeof(XmlDocument),
			typeof(PSPrimitiveDictionary)
		};
        public override object this[object key]
        {
            get
            {
                return base[key];
            }
            set
            {
                string key2 = this.VerifyKey(key);
                this.VerifyValue(value);
                base[key2] = value;
            }
        }
        public object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                this.VerifyValue(value);
                base[key] = value;
            }
        }
        public PSPrimitiveDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        public PSPrimitiveDictionary(Hashtable other)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            throw new NotImplementedException();
        }
        private PSPrimitiveDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private string VerifyKey(object key)
        {
            throw new NotImplementedException();
        }
        private void VerifyValue(object value)
        {
            throw new NotImplementedException();
        }
        public override void Add(object key, object value)
        {
            string key2 = this.VerifyKey(key);
            this.VerifyValue(value);
            base.Add(key2, value);
        }
        public override object Clone()
        {
            return new PSPrimitiveDictionary(this);
        }
        public void Add(string key, bool value)
        {
            this.Add(key, value);
        }
        public void Add(string key, bool[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, byte value)
        {
            this.Add(key, value);
        }
        public void Add(string key, byte[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, char value)
        {
            this.Add(key, value);
        }
        public void Add(string key, char[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, DateTime value)
        {
            this.Add(key, value);
        }
        public void Add(string key, DateTime[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, decimal value)
        {
            this.Add(key, value);
        }
        public void Add(string key, decimal[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, double value)
        {
            this.Add(key, value);
        }
        public void Add(string key, double[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, Guid value)
        {
            this.Add(key, value);
        }
        public void Add(string key, Guid[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, int value)
        {
            this.Add(key, value);
        }
        public void Add(string key, int[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, long value)
        {
            this.Add(key, value);
        }
        public void Add(string key, long[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, sbyte value)
        {
            this.Add(key, value);
        }
        public void Add(string key, sbyte[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, float value)
        {
            this.Add(key, value);
        }
        public void Add(string key, float[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, string value)
        {
            this.Add(key, value);
        }
        public void Add(string key, string[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, TimeSpan value)
        {
            this.Add(key, value);
        }
        public void Add(string key, TimeSpan[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, ushort value)
        {
            this.Add(key, value);
        }
        public void Add(string key, ushort[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, uint value)
        {
            this.Add(key, value);
        }
        public void Add(string key, uint[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, ulong value)
        {
            this.Add(key, value);
        }
        public void Add(string key, ulong[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, Uri value)
        {
            this.Add(key, value);
        }
        public void Add(string key, Uri[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, Version value)
        {
            this.Add(key, value);
        }
        public void Add(string key, Version[] value)
        {
            this.Add(key, value);
        }
        public void Add(string key, PSPrimitiveDictionary value)
        {
            this.Add(key, value);
        }
        public void Add(string key, PSPrimitiveDictionary[] value)
        {
            this.Add(key, value);
        }
        internal static PSPrimitiveDictionary CloneAndAddPSVersionTable(PSPrimitiveDictionary originalHash)
        {
            throw new NotImplementedException();
        }
    }
}
