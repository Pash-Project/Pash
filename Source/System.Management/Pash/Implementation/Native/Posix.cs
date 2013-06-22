// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Reflection;

namespace Pash.Implementation.Native
{
    /// <summary>
    /// Uses reflection for dynamic loading of Mono.Posix assembly and invocation of native calls.
    /// Reflection is used for cross-platform compatibility of current assembly. Methods of this
    /// class should be called only if Mono runtime is currently used.
    /// </summary>
    internal static class Posix
    {
        public static readonly object X_OK;

        private static readonly Delegate _access;
        
        static Posix()
        {
            var posix = Assembly.Load("Mono.Posix");
            var syscall = posix.GetType("Mono.Unix.Native.Syscall");
            var access = syscall.GetMethod("access");
            _access = Delegate.CreateDelegate(syscall, access);

            var accessModes = posix.GetType("Mono.Unix.Native.AccessModes");
            X_OK = accessModes.GetField("X_OK").GetRawConstantValue();
        }

        public static int access(string path, object mode)
        {
            return (int) _access.DynamicInvoke(new[] { path, mode });
        }
    }
}
