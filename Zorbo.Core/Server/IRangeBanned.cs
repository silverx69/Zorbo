using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zorbo.Core.Server
{
    public interface IRangeBanned : IObservableCollection<Regex>
    {
    }
}
