using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server.Users
{
    public class Banned : ModelList<ClientId>, IBanned
    {
    }
}
