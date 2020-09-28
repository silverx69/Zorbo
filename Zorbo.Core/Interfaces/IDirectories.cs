using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zorbo.Core.Models;

namespace Zorbo.Core.Interfaces
{
    public interface IDirectories
    {
        string AppData { get; }
        string Logs { get; }
        string Plugins { get; }
        string Certificates { get; }
    }
}
