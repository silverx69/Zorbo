using System;
using System.Collections.Generic;
using System.Text;

namespace Zorbo.Core.Interfaces
{
    public interface IFloodCounter
    {
        int Count { get; set; }
        DateTime Last { get; set; }
    }
}
