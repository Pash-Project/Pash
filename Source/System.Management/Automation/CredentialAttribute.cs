// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    /// <summary>
    /// Parameter that accepts credentials.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CredentialAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            string username = "";
            if (inputData != null)
            {
                PSCredential credential;
                // first check direct conversion (maybe it's already a PSCredential)
                if (LanguagePrimitives.TryConvertTo<PSCredential>(inputData, out credential))
                {
                    return credential;
                }
                if (!LanguagePrimitives.TryConvertTo<string>(inputData, out username))
                {
                    throw new PSArgumentException("Cannot convert the argument to string as username");
                }
            }
            if (engineIntrinsics == null || engineIntrinsics.Host == null || engineIntrinsics.Host.UI == null)
            {
                throw new PSInvalidOperationException("Host UI is not available for querying the credential.");
            }
            return engineIntrinsics.Host.UI.PromptForCredential(CredentialMessages.DefaultQueryCaption,
                CredentialMessages.DefaultQueryMessage, username, "");
        }
    }
}

