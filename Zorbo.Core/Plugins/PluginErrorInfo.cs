using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Interfaces;

namespace Zorbo.Core.Plugins
{
    public sealed class PluginErrorInfo : IErrorInfo
    {
        public string Name { get; internal set; }

        public string Method { get; internal set; }

        public Exception Exception { get; internal set; }

        public PluginErrorInfo(string name, string method, Exception ex)
        {
            Name = name;
            Method = method;
            Exception = ex;
        }
    }
}
