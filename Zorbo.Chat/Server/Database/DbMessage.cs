namespace Zorbo.Chat.Server.Database
{
    public class DbMessage
    {
        public ulong Id { get; set; }

        public ulong? ChannelId { get; set; }

        public DbChannel Channel { get; set; }

        public ulong? SenderId { get; set; }

        public DbProfile Sender { get; set; }

        public byte[] Content { get; set; }

        public DateTime Created { get; set; }
    }
}
