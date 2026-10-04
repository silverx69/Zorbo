using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Zorbo.Net
{
    public sealed class RequestMetadata
    {
        public string Method { get; set; }
        public string Resource { get; set; }
        public string Protocol { get; set; }
        public Dictionary<string, string> Headers { get; } = [];
    }

    public sealed class ResponseMetadata
    {
        public string Protocol { get; set; }
        public HttpStatusCode Code { get; set; }
        public Dictionary<string, string> Headers { get; } = [];
    }

    public partial class HttpHelper
    {
        const string FIXED_HASH =
            "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        const string RESPONSE_TEMPLATE =
            "HTTP/1.1 {0} {1}\r\n" +
            "Server: {2}\r\n" +
            "Access-Control-Allow-Origin: *\r\n" +
            "Connection: keep-alive\r\n" +
            "Cache-Control: no-store, max-age=0\r\n";


        const string UPGRADE_TEMPLATE =
            "GET {1} HTTP/1.1\r\n" +
            "Host: {0}\r\n" +
            "Connection: Upgrade\r\n" +
            "Upgrade: websocket\r\n" +
            "Sec-WebSocket-Key: {2}\r\n" +
            "Sec-WebSocket-Version: 13\r\n";

        const string ACCEPT_TEMPLATE =
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            "Sec-WebSocket-Accept: {0}\r\n";


        public static string GetAcceptKeyHash(Guid guid) {
            return GetAcceptKeyHash(Convert.ToBase64String(guid.ToByteArray()));
        }

        public static string GetAcceptKeyHash(string key) {
            return Convert.ToBase64String(SHA1.HashData(Encoding.UTF8.GetBytes(key + FIXED_HASH)));
        }

        public static RequestMetadata ParseRequestHeaders(string header) {
            var request = new RequestMetadata();

            string[] lines = header.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
            Match match = RequestHeaderExpression().Match(lines[0]);

            if (match.Success) {
                request.Method = match.Groups[1].Value.ToUpper();
                request.Resource = match.Groups[2].Value;
                request.Protocol = match.Groups[3].Value.ToUpper();

                for (int i = 1; i < lines.Length; i++) {
                    match = HeaderLineExpression().Match(lines[i]);
                    if (match.Success)
                        request.Headers.Add(match.Groups[1].Value.ToUpper(), match.Groups[2].Value);
                }
            }
            return request;
        }

        public static ResponseMetadata ParseResponseHeaders(string header) {
            var response = new ResponseMetadata();

            string[] lines = header.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
            Match match = ResponseHeaderExpression().Match(lines[0]);

            if (match.Success) {
                response.Protocol = match.Groups[1].Value.ToUpper();
                response.Code = Enum.Parse<HttpStatusCode>(match.Groups[2].Value, true);

                for (int i = 1; i < lines.Length; i++) {
                    match = HeaderLineExpression().Match(lines[i]);
                    if (match.Success)
                        response.Headers.Add(match.Groups[1].Value.ToUpper(), match.Groups[2].Value);
                }
            }
            return response;
        }

        public static string ResponseHeader(HttpStatusCode code, string server = "Zorbo", params KeyValuePair<string, string>[] extra_headers) {
            var sb = new StringBuilder(RESPONSE_TEMPLATE);

            foreach (var extra in extra_headers)
                sb.AppendFormat("{0}: {1}\r\n", extra.Key, extra.Value);

            sb.Append("\r\n");
            return sb.ToStringFormat((int)code, code, server);
        }

        public static byte[] ResponseHeaderBytes(HttpStatusCode code, string server = "Zorbo", params KeyValuePair<string, string>[] extra_headers) {
            return Encoding.UTF8.GetBytes(ResponseHeader(code, server, extra_headers));
        }


        public static string UpgradeWebSocketHeader(string host, string resource, string key, params KeyValuePair<string, string>[] extra_headers) {
            var sb = new StringBuilder(UPGRADE_TEMPLATE);

            foreach (var extra in extra_headers)
                sb.AppendFormat("{0}: {1}\r\n", extra.Key, extra.Value);

            sb.Append("\r\n");
            return sb.ToStringFormat(host, resource, key);
        }

        public static byte[] UpgradeWebSocketHeaderBytes(string host, string key, params KeyValuePair<string, string>[] extra_headers) {
            return Encoding.UTF8.GetBytes(UpgradeWebSocketHeader(host, "/", key, extra_headers));
        }

        public static byte[] UpgradeWebSocketHeaderBytes(Uri uri, Guid guid, params KeyValuePair<string, string>[] extra_headers) {
            return UpgradeWebSocketHeaderBytes(uri, Convert.ToBase64String(guid.ToByteArray()), extra_headers);
        }

        public static byte[] UpgradeWebSocketHeaderBytes(Uri uri, string key, params KeyValuePair<string, string>[] extra_headers) {
            return Encoding.UTF8.GetBytes(UpgradeWebSocketHeader(uri.Host, uri.PathAndQuery, key, extra_headers));
        }

        public static string AcceptWebSocketHeader(string key, params KeyValuePair<string, string>[] extra_headers) {
            var sb = new StringBuilder(ACCEPT_TEMPLATE);

            foreach (var extra in extra_headers)
                sb.AppendFormat("{0}: {1}\r\n", extra.Key, extra.Value);

            sb.Append("\r\n");
            return sb.ToStringFormat(GetAcceptKeyHash(key));
        }

        public static byte[] AcceptWebSocketHeaderBytes(Guid guid, params KeyValuePair<string, string>[] extra_headers) {
            return AcceptWebSocketHeaderBytes(Convert.ToBase64String(guid.ToByteArray()), extra_headers);
        }

        public static byte[] AcceptWebSocketHeaderBytes(string key, params KeyValuePair<string, string>[] extra_headers) {
            return Encoding.UTF8.GetBytes(AcceptWebSocketHeader(key, extra_headers));
        }


        [GeneratedRegex("^(\\S+)\\s+/(\\S*)\\s+(.*)", RegexOptions.Singleline)]
        private static partial Regex RequestHeaderExpression();

        [GeneratedRegex("^(\\S*)\\s+(\\d+)\\s+(.*)", RegexOptions.Singleline)]
        private static partial Regex ResponseHeaderExpression();

        [GeneratedRegex("^\\s*(\\S+)\\s*:\\s*(.+)", RegexOptions.Singleline)]
        private static partial Regex HeaderLineExpression();
    }
}