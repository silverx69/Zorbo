using Microsoft.EntityFrameworkCore;
using Zorbo.Chat.Server.Database;

namespace Zorbo.Chat.Server
{
    public partial class ChatServer
    {
        protected static class Profiles
        {
            static Profiles() {
                Find = EF.CompileAsyncQuery(
                    (Database ctx, DbProfile profile)
                        => ctx.Profiles.Where(u => u.Equals(profile)).FirstOrDefault());
            }

            public static readonly Func<Database, DbProfile, Task<DbProfile>> Find;
        }
    }
}
