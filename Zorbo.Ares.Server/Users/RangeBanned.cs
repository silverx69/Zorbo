using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Zorbo.Core.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server.Users
{
    public class RangeBanned : ModelList<Regex>, IRangeBanned
    {
    }
}
