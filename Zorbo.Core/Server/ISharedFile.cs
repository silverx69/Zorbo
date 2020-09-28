using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Server
{
    public interface ISharedFile
    {
        byte Type { get; }
        uint Size { get; }

        string SearchWords { get; }

        byte[] Content { get; }
    }
}
