namespace Zorbo.Chat.Server.Database
{
    public class DbChannel : IEquatable<DbChannel>
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public bool Default { get; set; }

        public DateTime Created { get; set; }

        public ICollection<DbMessage> Messages { get; set; }

        public bool Equals(DbChannel other) {
            if (other is null) 
                return false;
            return Id == other.Id;
        }

        public override bool Equals(object other) {
            return Equals(other as DbChannel);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }
    }
}
