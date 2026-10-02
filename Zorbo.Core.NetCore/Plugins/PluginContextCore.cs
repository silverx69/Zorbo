using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Zorbo.Core.Plugins
{
    class PluginContextCore : PluginContext
    {
        readonly AssemblyDependencyResolver _resolver;

        public PluginContextCore(string pluginPath)
            : base(pluginPath) {
            _resolver = new AssemblyDependencyResolver(FilePath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath == null) return null;
            return LoadFromAssemblyPath(assemblyPath);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath == null) return IntPtr.Zero;
            return LoadUnmanagedDllFromPath(libraryPath);
        }
    }
}
