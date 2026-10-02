using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class RoomScribble 
    {
        bool received;

        readonly List<byte[]> buffers;

        public uint Size { get; set; }

        public ushort Chunks { get; set; }

        public ushort ChunkCount { get; private set; }

        public int Index { get; set; }

        public string Username { get; set; }

        public long Remaining {
            get { return ChunkCount - Index; }
        }

        public long Received {
            get { return buffers.Sum((s) => s.Length); }
        }

        public bool IsComplete {
            get {
                return received && 
                    (this.ChunkCount >= Chunks) &&
                    (this.Received == Size);
            }
        }

        public RoomScribble() {
            this.buffers = new List<byte[]>();
        }

        public void Write(byte[] chunk) {
            if (chunk == null)
                throw new ArgumentNullException("chunk");

            if (IsComplete)
                throw new Exception("Something wierd happened.");

            this.received = true;
            this.ChunkCount++;
            this.buffers.Add(chunk);
        }

        public void Write(byte[] chunk, int index, int count) {
            if (chunk == null)
                throw new ArgumentNullException("chunk");

            if (IsComplete)
                throw new Exception("Something wierd happened.");

            this.received = true;
            this.ChunkCount++;
            this.buffers.Add(chunk.Skip(index).Take(count).ToArray());
        }


        public byte[] Read() {
            if (!IsComplete)
                return null;

            return buffers[Index++];
        }

        public byte[] RawImage() {
            var buffer = new List<byte>();
            foreach (var chunk in buffers) 
                buffer.AddRange(chunk);
            return buffer.ToArray();
        }

        public int GetHeight() {
            return GetHeight(Zlib.Decompress(RawImage()));
        }

        public static int GetHeight(byte[] decompressedBytes) {

            using var stream = new MemoryStream(decompressedBytes);
            using var bmp1 = (Image<Rgba32>)Image.Load(stream);

            return bmp1.Height;
        }

        class LoadRequest
        {
            public object State { get; set; }
            
            public WebRequest Request { get; set; }

            public Action<object> Callback { get; set; }

            public LoadRequest() { }
            public LoadRequest(WebRequest request, Action<object> cb, object state) {
                State = state;
                Request = request;
                Callback = cb;
            }
        }


        public bool Download(Uri uri, Action<object> callback, object state = null) {
            try {
                var request = WebRequest.Create(uri);

                request.Method = "GET";
                request.BeginGetResponse(LoadCallback, new LoadRequest(request, callback, state));

                return true;
            }
            catch { return false; }
        }

        private void LoadCallback(IAsyncResult ar) {
            var lr = (LoadRequest)ar.AsyncState;

            Image<Rgba32> bmp2 = null;

            try {
                using var response = lr.Request.EndGetResponse(ar);

                using (var stream = response.GetResponseStream()) {
                    using var bmp1 = (Image<Rgba32>)Image.Load(stream);
                    CopyResponse(bmp1);
                }

                lr.Callback(lr.State);
            }
            catch (Exception ex) {
                lr.Callback(ex);
            }
            finally {
                if (bmp2 != null) bmp2.Dispose();
            }
        }

        private void CopyResponse(Image<Rgba32> bitmap) 
        {
            bitmap.Mutate((s) => s.Resize(new ResizeOptions() { 
                Mode = ResizeMode.Max,
                Size = new Size(384, 384)
            }));

            using var stream2 = new MemoryStream();

            bitmap.Save(stream2, new JpegEncoder());
            byte[] tmp = stream2.ToArray();

            stream2.SetLength(0);
            Zlib.Compress(stream2, tmp);

            tmp = new byte[4000];

            Reset();
            stream2.Position = 0;

            while (true) {
                int count = stream2.Read(tmp, 0, tmp.Length);
                if (count == 0) break;

                Write(tmp, 0, count);
            }

            Size = (uint)Received;
            Chunks = ChunkCount;
        }

        public void Reset() {
            Size = 0;
            Chunks = 0;
            Index = 0;
            this.ChunkCount = 0;
            this.received = false;
            this.buffers.Clear();
        }

        public static RoomScribble GetScribble(IClient client) {

            if (client.Extended.TryGetValue("Scribble", out object tmp))
                return (RoomScribble)tmp;

            var scribble = new RoomScribble();
            client.Extended["Scribble"] = scribble;

            return scribble;
        }
    }
}
