using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zorbo.Core.Models;

namespace Zorbo.Core
{
    public interface IDirectories
    {
        string AppData { get; set; }
        string Logs { get; }
        string Plugins { get; }
        string Certificates { get; }

        void EnsureExists();
    }
}
