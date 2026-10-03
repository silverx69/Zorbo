namespace Zorbo.Chat.Server.Database
{
    public class DbProfile : IEquatable<DbProfile>
    {
        public ulong Id { get; set; }

        public string Guid { get; set; }
        
        public string Address { get; set; }

        public string UserName { get; set; }

        public bool Banned { get; set; }

        public bool Muzzled { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        
        public ICollection<DbMessage> Messages { get; set; }


        public bool Equals(DbProfile other) {
            return (other?.GetHashCode() ?? 0) == GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as DbProfile);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Guid, UserName, Address);
        }
    }
}
