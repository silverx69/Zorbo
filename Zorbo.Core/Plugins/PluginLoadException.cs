using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Plugins
{
    public class PluginLoadException : Exception
    {
        public PluginLoadException(string message)
            : base(message) { }

        public PluginLoadException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
