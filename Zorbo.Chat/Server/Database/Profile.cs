namespace Zorbo.Chat.Server.Database
{
    public class Profile
    {
        public ulong Id { get; set; }

        public string Guid { get; set; }

        public string Address { get; set; }

        public string Username { get; set; }

        public bool Banned { get; set; }

        public bool Muzzled { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public ICollection<Message> SentMessages { get; set; }

        public ICollection<Message> ReceivedMessages { get; set; }
    }
}