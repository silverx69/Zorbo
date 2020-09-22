using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Plugins.Client;

namespace Zorbo.Core.Interfaces.Client
{
    public interface IChatClientPluginHost : IPluginHost<IChatClient, ChatClientPlugin>
    {
    }
}
