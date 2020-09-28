using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Server
{
    public enum CaptchaEvent : byte
    {
        Enter,
        Failed,
        Banned,
        Exit,
    }
}
