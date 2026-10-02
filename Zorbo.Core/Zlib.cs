using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Zorbo.Core
{
    public static class Zlib
    {
        public static byte[] Compress(byte[] input)
        {
            using var mem = new MemoryStream();
            using var zlib = new DeflaterOutputStream(mem);

            zlib.Write(input, 0, input.Length);
            zlib.Finish();

            return mem.ToArray();
        }

        public static void Compress(Stream stream, byte[] input)
        {
            using var zlib = new DeflaterOutputStream(stream);

            zlib.IsStreamOwner = false;

            zlib.Write(input, 0, input.Length);
            zlib.Finish();
        }

        public static byte[] Decompress(byte[] input)
        {
            byte[] ret;

            using (var stream = new MemoryStream(input))
            using (var inflater = new InflaterInputStream(stream)) {

                stream.Position = 0;

                using var output = new MemoryStream();

                int count = 0;
                byte[] buffer = new byte[2048];

                while ((count = inflater.Read(buffer, 0, buffer.Length)) > 0)
                    output.Write(buffer, 0, count);

                output.Flush();
                output.Position = 0;

                ret = new byte[output.Length];
                output.Read(ret, 0, ret.Length);
            }

            return ret;
        }

        public static void Decompress(Stream output, byte[] input)
        {
            using var stream = new MemoryStream(input);
            using var inflater = new InflaterInputStream(stream);

            stream.Position = 0;

            int count = 0;
            long start = output.Position;

            byte[] buffer = new byte[2048];

            while ((count = inflater.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, count);

            output.Flush();
            output.Position = start;
        }
    }
}