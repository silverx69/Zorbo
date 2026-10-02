namespace Zorbo.Chat
{
    [Flags]
    public enum ServerSupportFlags : byte
    {
        /// <summary>
        /// Server does not have any flags enabled.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Server allows sending private messages.
        /// </summary>
        PRIVATE = 1,
        /// <summary>
        /// Server supports room shares, (images, files, etc)
        /// </summary>
        SHARING = 2,
        /// <summary>
        /// Server allows sending shares via private messages.
        /// </summary>
        PRIVATE_SHARING = 4,
        /// <summary>
        /// Server supports user avatars. (Disable to save bandwidth?)
        /// </summary>
        AVATARS = 8,
        /// <summary>
        /// Server supports voice chat. (Implementation of voice TBD)
        /// </summary>
        VOICE = 16,
        /// <summary>
        /// Server has all support flags enabled.
        /// </summary>
        ALL = VOICE | AVATARS | PRIVATE_SHARING | SHARING | PRIVATE
    }

    [Flags]
    public enum ClientSupportFlags : byte
    {
        /// <summary>
        /// Client does not have any flags enabled.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Client allows receiving private messages.
        /// </summary>
        PRIVATE = 1,
        /// <summary>
        /// Client supports room shares, (images, files, etc)
        /// </summary>
        SHARING = 2,
        /// <summary>
        /// Client supports user avatars. (Disable to save bandwidth?)
        /// </summary>
        AVATARS = 4,
        /// <summary>
        /// Client supports voice chat. (Implementation of voice TBD)
        /// </summary>
        VOICE = 8,
        /// <summary>
        /// Client has all support flags enabled.
        /// </summary>
        ALL = VOICE | AVATARS | SHARING | PRIVATE
    }
}
