using Microsoft.EntityFrameworkCore;
using Zorbo.Chat.Server.Database;

namespace Zorbo.Chat.Server
{
    public partial class ChatServer 
    {
        public class Database : DbContext
        {
            public DbSet<Profile> Profiles { get; set; }

            public DbSet<Channel> Channels { get; set; }

            public DbSet<Message> Messages { get; set; }

            public Database(DbContextOptions<Database> options)
                : base(options) { }

            public override int SaveChanges() {
                OnUpdated();
                return base.SaveChanges();
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
                OnUpdated();
                return base.SaveChangesAsync(cancellationToken);
            }

            private void OnUpdated() {
                foreach (var entry in ChangeTracker.Entries<Profile>())
                    if (entry.State == EntityState.Modified)
                        entry.Entity.Updated = DateTime.UtcNow;

                foreach (var entry in ChangeTracker.Entries<Message>())
                    if (entry.State == EntityState.Modified)
                        entry.Entity.Updated = DateTime.UtcNow;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                var channelEntity = modelBuilder.Entity<Channel>();
                channelEntity
                    .Property(p => p.Created)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                var profileEntity = modelBuilder.Entity<Profile>();
                profileEntity
                    .Property(p => p.Created)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                profileEntity
                    .Property(p => p.Updated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                var messageEntity = modelBuilder.Entity<Message>();
                messageEntity
                    .Property(p => p.Created)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                messageEntity
                    .Property(p => p.Updated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                messageEntity
                    .HasOne(o => o.Sender)
                    .WithMany(c => c.SentMessages)
                    .HasForeignKey(o => o.SenderId)
                    .OnDelete(DeleteBehavior.SetNull);
                messageEntity
                    .HasOne(o => o.Receiver)
                    .WithMany(c => c.ReceivedMessages)
                    .HasForeignKey(o => o.ReceiverId)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }
    }
}