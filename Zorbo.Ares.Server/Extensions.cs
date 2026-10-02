using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Server;

namespace Zorbo.Core
{
    public static partial class Extensions
    {
        public static bool CanSee(this IClient client, IClient other)
        {
            return !other.Cloaked || client.Admin >= other.Admin;
        }
    }
}
