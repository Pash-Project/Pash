// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class ErrorDetails : ISerializable
    {
        /*
        public ErrorDetails(string message);
        protected ErrorDetails(SerializationInfo info, StreamingContext context);
        public ErrorDetails(Assembly assembly, string baseName, string resourceId, params object[] args);
        public ErrorDetails(Cmdlet cmdlet, string baseName, string resourceId, params object[] args);
        public ErrorDetails(IResourceSupplier resourceSupplier, string baseName, string resourceId, params object[] args);

        public string Message { get; }
        public string RecommendedAction { get; set; }

        public override string ToString();
        */
        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
