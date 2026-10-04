namespace Zorbo.Chat.Server.Database
{
    public class Channel
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public bool Default { get; set; }

        public DateTime Created { get; set; }

        public ICollection<Message> Messages { get; set; }

        public Channel() { }

        public Channel(string name, bool @default = false) {
            Name = name;
            Default = @default;
        }
    }
}
