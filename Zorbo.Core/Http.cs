using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Zorbo.Core
{
    public static class Http
    {
        const string fixed_hash = 
            "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        const string upgrade_template = 
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            "Sec-WebSocket-Accept: {0}\r\n";

        public static string WebSocketAcceptHeader(string key, params KeyValuePair<string, string>[] extra_headers)
        {
            string header = upgrade_template;

            foreach (var extra in extra_headers)
                header += string.Format("{0}: {1}\r\n", extra.Key, extra.Value);

            header += "\r\n";

            return string.Format(header, Convert.ToBase64String(Utils.SHA1.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(key, fixed_hash)))));
        }

        public static byte[] WebSocketAcceptHeaderBytes(string key, params KeyValuePair<string, string>[] extra_headers)
        {
            return Encoding.UTF8.GetBytes(WebSocketAcceptHeader(key, extra_headers));
        }
    }
}
