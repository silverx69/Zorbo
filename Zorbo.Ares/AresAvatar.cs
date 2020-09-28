using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Zorbo.Ares
{
    public static class AresAvatars
    {
        public static byte[] Load(string filename)
        {
            Bitmap bmp1 = null;
            Bitmap bmp2 = null;
            Stream stream = null;
            try {
                bmp1 = new Bitmap(filename);
                bmp2 = ScaleImage(bmp1, 48, 48);

                stream = ConvertToJpegStream(bmp2);

                byte[] small = new byte[stream.Length];
                stream.Read(small, 0, (int)stream.Length);

                return small;
            }
            finally {
                if (bmp1 != null)
                    bmp1.Dispose();

                if (bmp2 != null)
                    bmp2.Dispose();

                if (stream != null) {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public static Bitmap ScaleImage(Bitmap bitmap, int maxwidth, int maxheight)
        {
            Bitmap bmp1 = null;
            try {
                bmp1 = ResizeImage(bitmap, maxwidth, maxheight);

                if (bmp1.Width != bmp1.Height)
                    return TrimExceeding(bmp1, maxwidth, maxheight);

                return (Bitmap)bmp1.Clone();
            }
            finally {
                if (bmp1 != null) bmp1.Dispose();
            }
        }

        public static Bitmap ResizeImage(Bitmap bitmap, int maxwidth, int maxheight)
        {
            int biggest = Math.Max(maxheight, maxwidth);

            if (bitmap.Width == bitmap.Height)
                return bitmap.Clone(new Rectangle(0, 0, biggest, biggest), bitmap.PixelFormat);

            double p;

            if (bitmap.Width > bitmap.Height)
                p = (double)biggest / (double)bitmap.Height;
            else
                p = (double)biggest / (double)bitmap.Width;

            return bitmap.Clone(new Rectangle(0, 0, (int)(bitmap.Width * p), (int)(bitmap.Height * p)), bitmap.PixelFormat);
        }

        public static Bitmap TrimExceeding(Bitmap bitmap, int maxwidth, int maxheight)
        {
            if (bitmap.Width > maxwidth) {
                int p = (bitmap.Width - maxwidth);
                return bitmap.Clone(new Rectangle(p / 2, 0, bitmap.Width - p, bitmap.Height), bitmap.PixelFormat);
            }
            else {
                int p = (bitmap.Height - maxheight);
                return bitmap.Clone(new Rectangle(0, p / 2, bitmap.Width, bitmap.Height - p), bitmap.PixelFormat);
            }
        }

        public static Stream ConvertToJpegStream(Bitmap img)
        {
            MemoryStream stream = new MemoryStream();

            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            return stream;
        }

        public static void ConvertToJpegStream(Stream stream, Bitmap img)
        {
            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;
        }
    }
}
