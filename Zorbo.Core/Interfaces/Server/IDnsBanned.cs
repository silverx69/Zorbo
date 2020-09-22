using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IDnsBanned : IObservableCollection<Regex>
    {
    }
}
