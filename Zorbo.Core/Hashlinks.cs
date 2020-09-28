using System;
using System.IO;

using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Zorbo.Core
{
    public static class Hashlinks
    {
        public static T FromHashlinkString<T>(string base64hash) where T : IHashlink
        {
            base64hash = base64hash.TrimStart('\\');

            if (base64hash.StartsWith("arlnk://"))
                base64hash = base64hash.Substring(8);

            if (base64hash.Substring(0, 8).ToLower().StartsWith("chatroom"))
                return FromPlainText<T>(base64hash);

            return FromHashlinkArray<T>(Convert.FromBase64String(base64hash));
        }

        public static T FromHashlinkString<T>(string base64hash, T hashlink) where T : IHashlink
        {
            base64hash = base64hash.TrimStart('\\');

            if (base64hash.StartsWith("arlnk://"))
                base64hash = base64hash.Substring(8);

            if (base64hash.Substring(0, 8).ToLower().StartsWith("chatroom"))
                return FromPlainText<T>(base64hash, hashlink);

            return FromHashlinkArray<T>(Convert.FromBase64String(base64hash), hashlink);
        }

        public static T FromPlainText<T>(string plainText) where T : IHashlink
        {
            T ret = Activator.CreateInstance<T>();
            ret.FromPlainText(plainText);
            return ret;
        }

        public static T FromPlainText<T>(string plainText, T hashlink) where T : IHashlink
        {
            hashlink.FromPlainText(plainText);
            return hashlink;
        }

        public static T FromHashlinkArray<T>(byte[] hashbytes) where T : IHashlink
        {
            T ret = default;

            using (var stream = new MemoryStream(D67(hashbytes, 28435)))
            using (var inflater = new InflaterInputStream(stream)) {

                stream.Position = 0;

                using var output = new MemoryStream();

                int count = 0;
                byte[] buffer = new byte[2048];

                while ((count = inflater.Read(buffer, 0, buffer.Length)) > 0)
                    output.Write(buffer, 0, count);

                output.Flush();
                output.Position = 0;

                byte[] tmp = new byte[output.Length];
                output.Read(tmp, 0, tmp.Length);

                ret = Activator.CreateInstance<T>();
                ret.FromByteArray(tmp);
            }

            return ret;
        }

        public static T FromHashlinkArray<T>(byte[] hashbytes, T hashlink) where T : IHashlink
        {
            if (hashlink == null)
                throw new ArgumentNullException("hashlink");

            using (var stream = new MemoryStream(D67(hashbytes, 28435)))
            using (var inflater = new InflaterInputStream(stream)) {

                stream.Position = 0;

                using var output = new MemoryStream();

                int count = 0;
                byte[] buffer = new byte[2048];

                while ((count = inflater.Read(buffer, 0, buffer.Length)) > 0)
                    output.Write(buffer, 0, count);

                output.Flush();
                output.Position = 0;

                byte[] tmp = new byte[output.Length];
                output.Read(tmp, 0, tmp.Length);

                hashlink.FromByteArray(tmp);
            }

            return hashlink;
        }


        public static byte[] ToHashlinkArray<T>(T hashlink) where T : IHashlink
        {
            return ToHashlinkArray<T>(hashlink);
        }

        public static byte[] ToHashlinkArray<T>(T hashlink, int level) where T : IHashlink
        {
            using var stream = new MemoryStream();
            using var deflater = new DeflaterOutputStream(stream, new Deflater(level));

            byte[] buffer = hashlink.ToByteArray();

            deflater.Write(buffer, 0, buffer.Length);
            deflater.Finish();

            return E67(stream.ToArray(), 28435);
        }


        public static string ToHashlinkString<T>(T hashlink) where T : IHashlink
        {
            return ToHashlinkString<T>(hashlink, 9);
        }

        public static string ToHashlinkString<T>(T hashlink, int level) where T : IHashlink
        {
            using var stream = new MemoryStream();
            using var deflater = new DeflaterOutputStream(stream, new Deflater(level));

            byte[] buffer = hashlink.ToByteArray();

            deflater.Write(buffer, 0, buffer.Length);
            deflater.Finish();

            return Convert.ToBase64String(E67(stream.ToArray(), 28435));
        }

        public static string ToURLSafeBase64(this string base64)
        {
            return base64.Replace('+', '-').Replace('/', '_');
        }

        public static string FromURLSafeBase64(this string urlSafeBase64)
        {
            return urlSafeBase64.Replace('-', '+').Replace('_', '/');
        }

        public static byte[] D67(byte[] data, int b)
        {
            byte[] buffer = (byte[])data.Clone();

            for (int i = 0; i < data.Length; i++) {
                buffer[i] = (byte)((data[i] ^ (b >> 8)) & 255);
                b = ((b + data[i]) * 23219 + 36126) & 65535;
            }

            return buffer;
        }

        public static byte[] E67(byte[] data, int b)
        {
            byte[] buffer = (byte[])data.Clone();

            for (int i = 0; i < data.Length; i++) {
                buffer[i] = (byte)((data[i] ^ (b >> 8)) & 255);
                b = ((buffer[i] + b) * 23219 + 36126) & 65535;
            }

            return buffer;
        }
    }
}
