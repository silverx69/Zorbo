using System.ComponentModel.DataAnnotations.Schema;

namespace Zorbo.Chat.Server.Database
{
    public enum DbMessageType 
    {
        Announce,
        Public,
        Emote,
        Private
    }

    public class Message
    {
        public ulong Id { get; set; }

        public ulong? ChannelId { get; set; }

        [ForeignKey(nameof(ChannelId))]
        public Channel Channel { get; set; }

        public DbMessageType Type { get; set; }

        
        public ulong? SenderId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public Profile Sender { get; set; }

        public ulong? ReceiverId { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public Profile Receiver { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}
