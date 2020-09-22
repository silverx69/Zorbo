using System;
using System.Collections.Generic;
using System.Text;

namespace Zorbo.Core.Interfaces
{
    public interface IDatabaseConfig
    {
        string ServerName { get; }

        string Database { get; }

        string Username { get; }

        string Password { get; }
    }
}
