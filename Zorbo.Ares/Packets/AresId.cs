﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Ares.Packets
{
    /// <summary>
    /// An enumeration containing the basic set of Ares packet ID numbers. 
    /// This is not exhaustive. Plugins have the ability to handle packets with unknown IDs. 
    /// See demo 'cb0tProtocol' plugin for an example of extending the protocol.
    /// </summary>
    public enum AresId : byte
    {
        MSG_CHAT_SERVER_ERROR = 0,
        MSG_CHAT_CLIENT_RELOGIN = 1,
        MSG_CHAT_CLIENT_LOGIN = 2,
        MSG_CHAT_SERVER_LOGIN_ACK = 3,
        MSG_CHAT_CLIENT_UPDATE_STATUS = 4,
        MSG_CHAT_SERVER_UPDATE_USER_STATUS = 5,
        MSG_CHAT_SERVER_REDIRECT = 6,
        MSG_CHAT_CLIENT_AUTOLOGIN = 7,
        MSG_SERVER_ECHO = 8,
        MSG_CHAT_SERVER_AVATAR = 9,
        MSG_CHAT_CLIENT_AVATAR = 9,
        MSG_CHAT_SERVER_PUBLIC = 10,
        MSG_CHAT_CLIENT_PUBLIC = 10,
        MSG_CHAT_SERVER_EMOTE = 11,
        MSG_CHAT_CLIENT_EMOTE = 11,
        MSG_CHAT_SERVER_PERSONAL_MESSAGE = 13,
        MSG_CHAT_CLIENT_PERSONAL_MESSAGE = 13,
        MSG_CHAT_CLIENT_FASTPING = 14,
        MSG_CHAT_SERVER_FASTPING = 14,
        MSG_CHAT_SERVER_JOIN = 20,
        MSG_CHAT_SERVER_PART = 22,
        MSG_CHAT_CLIENT_PVT = 25,
        MSG_CHAT_SERVER_PVT = 25,
        MSG_CHAT_SERVER_ISIGNORINGYOU = 26,
        MSG_CHAT_SERVER_OFFLINEUSER = 27,
        MSG_CHAT_SERVER_CHANNEL_USER_LIST = 30,
        MSG_CHAT_SERVER_TOPIC = 31,
        MSG_CHAT_SERVER_TOPIC_FIRST = 32,
        MSG_CHAT_SERVER_CHANNEL_USER_LIST_END = 35,
        MSG_CHAT_SERVER_CHANNEL_USERLIST_CLEAR = 36,
        MSG_CHAT_SERVER_HTML = 43,
        MSG_CHAT_SERVER_NOSUCH = 44,
        MSG_CHAT_CLIENT_IGNORELIST = 45,
        MSG_CHAT_CLIENT_ADDSHARE = 50,
        MSG_CHAT_CLIENT_REMSHARE = 51,
        MSG_CHAT_CLIENT_BROWSE = 52,
        MSG_CHAT_SERVER_ENDOFBROWSE = 53,
        MSG_CHAT_SERVER_BROWSEERROR = 54,
        MSG_CHAT_SERVER_BROWSEITEM = 55,
        MSG_CHAT_SERVER_STARTOFBROWSE = 56,
        MSG_CHAT_CLIENT_SEARCH = 60,
        MSG_CHAT_SERVER_SEARCHHIT = 61,
        MSG_CHAT_SERVER_ENDOFSEARCH = 62,
        MSG_CHAT_CLIENT_DUMMY = 64,
        MSG_CHAT_SERVER_HERE_SUPERNODES = 70,
        MSG_CHAT_CLIENT_SEND_SUPERNODES = 70,
        MSG_CHAT_CLIENT_DIRCHATPUSH = 72,
        MSG_CHAT_SERVER_URL = 73,
        MSG_CHAT_CLIENT_COMMAND = 74,
        MSG_CHAT_SERVER_OPCHANGE = 75,
        MSG_CHAT_CLIENTCOMPRESSED = 80,
        MSG_CHAT_CLIENT_AUTHLOGIN = 82,
        MSG_CHAT_CLIENT_AUTHREGISTER = 83,
        MSG_CHAT_SERVER_MYFEATURES = 92,
        MSG_SERVER_LINK_REQ = 100,
        MSG_SERVER_LINK_ACK = 101,
        MSG_SERVER_LINK_ERROR = 102,
        MSG_SERVER_BROADCAST = 103,
        MSG_SERVER_RELAYTOUSER = 104,
        MSG_SERVER_CLOAK = 105,
        MSG_SERVER_NEWLINK = 106,
        MSG_SERVER_PONG = 107,
        MSG_HUB_TOSERVER_LOGINREQ = 110,
        MSG_HUB_TOSERVER_LOGINACK = 111,
        MSG_SERVER_TOHUB_LOGINREQ = 112,
        MSG_SERVER_TOHUB_LOGINACK = 113,
        MSG_SERVER_ADMIN_COMMAND = 120,
        MSG_SERVER_ADMIN_DISABLED = 121,
        MSG_SERVER_ADMIN_MESSAGE = 122,
        MSG_SERVER_SERVER_COMMAND = 123,
        MSG_SERVER_TEXT_RECEIVED = 124,
        MSG_SERVER_EMOTE_RECEIVED = 125,
        MSG_SERVER_FORCE_PM = 126,
        MSG_SERVER_CUSTOM_SET_TAGS = 127,
        MSG_SERVER_CUSTOM_NAME_TEXT = 128,
        MSG_SERVER_MUZZLE_UPDATE = 129,
        MSG_CHAT_SERVER_CUSTOM_DATA = 200,
        MSG_CHAT_CLIENT_CUSTOM_DATA = 200,
        MSG_CHAT_CLIENT_CUSTOM_DATA_ALL = 201,
    }
}
