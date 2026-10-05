namespace Zorbo.Chat.Messages
{
    public enum MessageId : ushort
    {
        UNKNOWN = 0,
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
        SERVER_UPDATE,
        SERVER_ANNOUNCE,
        CLIENT_PUBLIC,
        SERVER_PUBLIC,
        CLIENT_PRIVATE,
        SERVER_PRIVATE,
        SERVER_PRIVATE_ERROR
    }
}