using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Zorbo.Core.Interfaces;
using Zorbo.Core.Models;

namespace Zorbo.Ares
{
    public class AresAvatar : ModelBase, IAvatar
    {
        int hashCode = 0;

#pragma warning disable IDE0044 // Add readonly modifier
        byte[] small = null;
        byte[] large = null;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("small", Required = Required.Always)]
        public byte[] SmallBytes {
            get { return small; }
            set { OnPropertyChanged(() => small, value); }
        }

        [JsonProperty("large", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public byte[] LargeBytes {
            get { return large; }
            set { OnPropertyChanged(() => large, value); }
        }

        public AresAvatar() { }

        public AresAvatar(byte[] bytes) {
            this.small = bytes;
            this.large = bytes;
        }

        public AresAvatar(byte[] small, byte[] large) {
            this.small = small;
            this.large = large;
        }
        
        public bool Equals(IAvatar other) {

            if (other == null ||
                small == null ||
                other.SmallBytes == null) 
                return Object.ReferenceEquals(this, other);

            return small.SequenceEqual(other.SmallBytes);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AresAvatar);
        }


        public override int GetHashCode() {
            if (hashCode == 0)
                hashCode = small == null ? base.GetHashCode() : small.GetHashCode();
            return hashCode;
        }


        public static bool operator ==(AresAvatar a, IAvatar b) {

            if (Object.ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(AresAvatar a, IAvatar b) {
            return !(a == b);
        }


        public static AresAvatar Load(string filename) {
            Bitmap bmp1 = null;
            Bitmap bmp2 = null;
            Bitmap bmp3 = null;
            Stream stream = null;

            try {
                bmp1 = new Bitmap(filename);

                bmp2 = ScaleImage(bmp1, 48, 48);
                bmp3 = ScaleImage(bmp1, 96, 96);

                stream = ConvertToJpegStream(bmp2);

                byte[] small = new byte[stream.Length];
                stream.Read(small, 0, (int)stream.Length);

                stream.SetLength(0);
                ConvertToJpegStream(stream, bmp3);

                byte[] large = new byte[stream.Length];
                stream.Read(large, 0, (int)stream.Length);

                return new AresAvatar(small, large);
            }
            finally {
                if (bmp1 != null)
                    bmp1.Dispose();

                if (bmp2 != null)
                    bmp2.Dispose();

                if (bmp3 != null)
                    bmp3.Dispose();

                if (stream != null) {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        internal static Bitmap ScaleImage(Bitmap bitmap, int maxwidth, int maxheight) {
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

        internal static Bitmap ResizeImage(Bitmap bitmap, int maxwidth, int maxheight) {

            int biggest = Math.Max(maxheight, maxwidth);
            
            if (bitmap.Width == bitmap.Height)
                return new Bitmap(bitmap, new Size(biggest, biggest));

            double p;

            if (bitmap.Width > bitmap.Height)
                p = (double)biggest / (double)bitmap.Height;
            else
                p = (double)biggest / (double)bitmap.Width;

            return new Bitmap(bitmap, new Size((int)(bitmap.Width * p), (int)(bitmap.Height * p)));
        }

        private static Bitmap TrimExceeding(Bitmap bitmap, int maxwidth, int maxheight) {

            if (bitmap.Width > maxwidth) {
                int p = (bitmap.Width - maxwidth);
                return bitmap.Clone(new Rectangle(p / 2, 0, bitmap.Width - p, bitmap.Height), bitmap.PixelFormat);
            }
            else {
                int p = (bitmap.Height - maxheight);
                return bitmap.Clone(new Rectangle(0, p / 2, bitmap.Width, bitmap.Height - p), bitmap.PixelFormat);
            }
        }

        public static Stream ConvertToJpegStream(Bitmap img) {
            
            MemoryStream stream = new MemoryStream();

            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            return stream;
        }

        public static void ConvertToJpegStream(Stream stream, Bitmap img) {
            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;
        }
    }
}
