using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Zorbo.Core.Plugins
{
    public class PluginContext : AssemblyLoadContext
    {
        public string FilePath {
            get;
            private set;
        }

        public string PluginPath {
            get;
            private set;
        }

        protected static string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public PluginContext(string pluginPath)
        {
            PluginPath = Path.GetDirectoryName(pluginPath);
            FilePath = pluginPath;
        }

        protected virtual string ResolvePath(string asmname)
        {
            asmname = asmname.ToLower().EndsWith(".dll") ?
                asmname :
                asmname + ".dll";
            string assemblyPath = Path.Combine(PluginPath, asmname);
            if (File.Exists(assemblyPath)) return assemblyPath;

            assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, asmname);
            if (File.Exists(assemblyPath)) return assemblyPath;

            return null;
        }

        protected override Assembly Load(AssemblyName asmname)
        {
            string assemblyPath = ResolvePath(asmname.Name);
            if (assemblyPath == null) return null;
            return LoadFromAssemblyPath(assemblyPath);
        }

        protected override IntPtr LoadUnmanagedDll(string dllname)
        {
            string assemblyPath = ResolvePath(dllname);
            if (assemblyPath == null) return IntPtr.Zero;
            return LoadUnmanagedDllFromPath(assemblyPath);
        }
    }
}