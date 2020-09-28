using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zorbo.Chat.Blazor.Server.Data;
using Zorbo.Chat.Blazor.Server.Models;
using Zorbo.Chat.Blazor.Shared;
using Zorbo.Core;

namespace Zorbo.Chat.Blazor.Server.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        readonly ServersDbContext context;

        public ChatController(ServersDbContext dbContext)
        {
            context = dbContext;
        }

        [AllowAnonymous]
        [HttpGet("address")]
        [Produces(typeof(string))]
        public string GetMyAddress()
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [AllowAnonymous]
        [HttpGet("channels")]
        [Produces("application/json")]
        public IEnumerable<ServerRecord> GetChannels()
        {
            DateTime now = DateTime.UtcNow;
            return context.Servers
                          .Where(s => s.WebSockets)
                          .ToList() // DB doesn't like DateTime operation.
                          .Where(s => now.Subtract(s.LastUpdate).TotalMinutes < 40d)
                          .Select(s => {
                              s.Topic = Encoding.UTF8.GetString(Convert.FromBase64String(s.Topic));
                              return s;
                          });
        }
    }
}
