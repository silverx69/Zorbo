namespace Zorbo.Chat.Messages
{
    public enum MessageId : ushort
    {
        SERVER_ERROR,
        CLIENT_LOGIN,
        SERVER_LOGIN_ACK,
        CLIENT_ADMIN,
        SERVER_ADMIN_ACK,
        SERVER_DETAILS,
        SERVER_USERLIST,
        SERVER_JOINED,
        SERVER_PARTED,
        CLIENT_UPDATE,
        CLIENT_PUBLIC,
        SERVER_PUBLIC,
        CLIENT_PRIVATE,
        SERVER_PRIVATE
    }
}