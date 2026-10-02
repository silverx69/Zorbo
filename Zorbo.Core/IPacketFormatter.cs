using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core
{
    public interface IPacketFormatter
    {
        byte[] Format(IPacket message);
        byte[] FormatJson(IPacket message, bool ib0t = false);
        
        IPacket Unformat(byte id, string data, bool ib0t = false);
        IPacket Unformat(byte id, byte[] data);
        IPacket Unformat(byte id, byte[] data, int index, int count);
    }
}
