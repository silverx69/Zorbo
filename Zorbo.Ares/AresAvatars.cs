using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace Zorbo.Ares
{
    public static class AresAvatars
    {
        public static byte[] Load(string filename)
        {
            using var bmp1 = Image.Load(filename);

            bmp1.Mutate(x => x.Resize(new ResizeOptions() { 
                Mode = ResizeMode.Max,
                Size = new Size(48, 48)
            }));

            using Stream stream = new MemoryStream();

            bmp1.Save(stream, new JpegEncoder());
            stream.Position = 0;

            byte[] small = new byte[stream.Length];
            stream.Read(small, 0, (int)stream.Length);

            return small;
        }
    }
}
