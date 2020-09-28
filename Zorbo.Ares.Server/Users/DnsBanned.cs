using System.Text.RegularExpressions;
using Zorbo.Core.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server.Users
{
    public class DnsBanned : ModelList<Regex>, IDnsBanned
    {
    }
}
