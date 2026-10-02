using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core
{
    public interface IPacket
    {
        byte Id { get; }
    }
}
