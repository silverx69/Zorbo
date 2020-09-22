using System;
using System.Collections.Generic;
using System.Text;

namespace Zorbo.Core.Interfaces
{
    [Flags]
    public enum ClientFlags : byte
    {
        NONE = 0,
        VOICE = 1,
        PRIVATE_VOICE = 2,
        OPUS_VOICE = 4,
        PRIVATE_OPUS_VOICE = 8,
        HTML = 16,
        ALL = HTML | PRIVATE_OPUS_VOICE | OPUS_VOICE | PRIVATE_OPUS_VOICE | VOICE
    }

    [Flags]
    public enum SupportFlags : byte
    {
        NONE = 0,
        PRIVATE = 1,
        SHARING = 2,
        COMPRESSION = 4,
        VOICE = 8,
        OPUS_VOICE = 16,
        ROOM_SCRIBBLES = 32,
        PRIVATE_SCRIBBLES = 64,
        HTML = 128
    }
}
