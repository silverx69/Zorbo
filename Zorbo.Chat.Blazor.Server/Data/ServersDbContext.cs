using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zorbo.Chat.Blazor.Server.Models;
using Zorbo.Chat.Blazor.Shared;

namespace Zorbo.Chat.Blazor.Server.Data
{
    public class ServersDbContext : DbContext
    {
        public DbSet<ServerRecord> Servers { get; set; }

        public ServersDbContext() { }

        public ServersDbContext(DbContextOptions<ServersDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerRecord>()
                .Ignore("BareTopic")
                .Ignore("HashlinkUri");
        }
    }
}
