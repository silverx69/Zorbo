using System;
using System.Collections.Generic;
using System.Text;

namespace Zorbo.Core
{
    public static class Base64
    {
        public static string Encode(string plaintext, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return Convert.ToBase64String(encoding.GetBytes(plaintext));
        }

        public static string Decode(string base64text, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetString(Convert.FromBase64String(base64text));
        }

        public static string ToBase64(this string plaintext, Encoding encoding = null)
        {
            return Encode(plaintext, encoding);
        }

        public static string FromBase64(this string base64text, Encoding encoding = null)
        {
            return Decode(base64text, encoding);
        }
    }
}
