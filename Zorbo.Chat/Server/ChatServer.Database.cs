using Microsoft.EntityFrameworkCore;
using Zorbo.Chat.Server.Database;

namespace Zorbo.Chat.Server
{
    public partial class ChatServer 
    {
        public class Database : DbContext
        {
            public DbSet<DbProfile> Profiles { get; set; }

            public DbSet<DbChannel> Channels { get; set; }

            public DbSet<DbMessage> Messages { get; set; }

            public Database(DbContextOptions<Database> options)
                : base(options) { }

            /* 
            Cannot be used with the PooledDbContextFactory
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
                optionsBuilder.UseSqlite("Data Source=Zorbo.Chat.db");
            }
            */
        }
    }
}