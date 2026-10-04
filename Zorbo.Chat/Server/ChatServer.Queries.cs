using Microsoft.EntityFrameworkCore;
using Zorbo.Chat.Server.Database;

namespace Zorbo.Chat.Server
{
    public partial class ChatServer
    {
        protected static class Channels
        {
            public static Task<Channel> Default(Database ctx) {
                return DefaultQuery(ctx);
            }

            public static Task<Channel> Get(Database ctx, ulong id) {
                return GetQuery(ctx, id);
            }

            public static Task<bool> Exists(Database ctx, string name) {
                return ExistQuery(ctx, name);
            }

            public static Task<bool> Exists(Database ctx, Channel channel) {
                return ExistQuery(ctx, channel.Name);
            }

            public static Task<Channel> Find(Database ctx, Channel channel) {
                return FindQuery(ctx, channel.Name);
            }

            public static async Task<Channel> Add(Database ctx, Channel channel) {
                ctx.Channels.Add(channel);
                await ctx.SaveChangesAsync();
                return channel;
            }

            public static async Task<Channel> Update(Database ctx, Channel channel) {
                ctx.Channels.Add(channel);
                await ctx.SaveChangesAsync();
                return channel;
            }

            static Channels() {
                DefaultQuery = EF.CompileAsyncQuery(
                    (Database ctx)
                        => ctx.Channels.FirstOrDefault(u => u.Default) ?? ctx.Channels.FirstOrDefault());

                GetQuery = EF.CompileAsyncQuery(
                    (Database ctx, ulong id)
                        => ctx.Channels.FirstOrDefault(u => u.Id == id));

                ExistQuery = EF.CompileAsyncQuery(
                    (Database ctx, string name)
                        => ctx.Channels.Any(u => u.Name == name));

                FindQuery = EF.CompileAsyncQuery(
                    (Database ctx, string name)
                        => ctx.Channels.FirstOrDefault(u => u.Name == name));
            }

            static readonly Func<Database, Task<Channel>> DefaultQuery;

            static readonly Func<Database, ulong, Task<Channel>> GetQuery;

            static readonly Func<Database, string, Task<bool>> ExistQuery;

            static readonly Func<Database, string, Task<Channel>> FindQuery;
        }

        protected static class Profiles
        {
            public static Task<Profile> Get(Database ctx, ulong id) {
                return GetQuery(ctx, id);
            }

            public static Task<bool> Exists(Database ctx, Profile profile) {
                return ExistQuery(ctx, profile.Guid, profile.Address, profile.Username);
            }

            public static Task<Profile> Find(Database ctx, Profile profile) {
                return FindQuery(ctx, profile.Guid, profile.Address, profile.Username);
            }

            public static async Task<Profile> Add(Database ctx, Profile profile) {
                ctx.Profiles.Add(profile);
                await ctx.SaveChangesAsync();
                return profile;
            }

            public static async Task<Profile> Update(Database ctx, Profile profile) {
                ctx.Profiles.Update(profile);
                await ctx.SaveChangesAsync();
                return profile;
            }

            static Profiles() {
                GetQuery = EF.CompileAsyncQuery(
                    (Database ctx, ulong id)
                        => ctx.Profiles.FirstOrDefault(u => u.Id == id));

                ExistQuery = EF.CompileAsyncQuery(
                    (Database ctx, string a, string b, string c)
                        => ctx.Profiles.Any(u => u.Guid == a || u.Address == b || u.Username == c));

                FindQuery = EF.CompileAsyncQuery(
                    (Database ctx, string a, string b, string c)
                        //=> ctx.Profiles.FirstOrDefault(u => u.Guid == a || u.Username == c));
                        => ctx.Profiles.FirstOrDefault(u => u.Guid == a || u.Address == b || u.Username == c));
            }

            static readonly Func<Database, ulong, Task<Profile>> GetQuery;

            static readonly Func<Database, string, string, string, Task<bool>> ExistQuery;

            static readonly Func<Database, string, string, string, Task<Profile>> FindQuery;
        }

        protected static class Messages
        {
            static Messages() {
                GetQuery = EF.CompileAsyncQuery(
                    (Database ctx, ulong id)
                        => ctx.Messages.FirstOrDefault(u => u.Id == id));
            }

            public static async Task<Message> Add(Database ctx, Message message) {
                ctx.Messages.Add(message);
                await ctx.SaveChangesAsync();
                return message;
            }

            public static async Task<Message> Update(Database ctx, Message message) {
                ctx.Messages.Update(message);
                await ctx.SaveChangesAsync();
                return message;
            }

            static readonly Func<Database, ulong, Task<Message>> GetQuery;
        }
    }
}