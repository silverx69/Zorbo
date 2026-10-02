using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Data
{
    public class IOBufferManager
    {
        readonly object lockObj = null;

        public class Slot 
        {
            public byte[] Buffer { get; private set; }

            public int Offset { get; private set; }

            internal Slot(byte[] buffer, int offset) {
                Buffer = buffer;
                Offset = offset;
            }
        }


        public int BufferSize {
            get;
            private set;
        }

        public int BufferChunkSize {
            get;
            private set;
        }

        public int MaximumBufferSize {
            get;
            private set;
        }


        internal List<Byte[]> Buffers {
            get;
            private set;
        }

        internal SortedDictionary<Int32, Stack<Int32>> Indices {
            get;
            private set;
        }


        /// <param name="bufferSize">The size of each large buffer created to house many "<paramref name="bufferChunkSize"/>" buffers</param>
        /// <param name="bufferChunkSize">The size of each individual buffer slot, this number should divide evenly into <paramref name="bufferSize"/></param>
        /// <exception cref="System.ArgumentException"/>
        public IOBufferManager(int bufferSize, int bufferChunkSize) {

            if (bufferSize % bufferChunkSize != 0)
                throw new ArgumentException("bufferChunkSize", "For optimal usage, BufferSize devide evenly by BufferChunkSize");

            BufferSize = bufferSize;
            BufferChunkSize = bufferChunkSize;

            lockObj = new Object();
            Buffers = new List<Byte[]>();
            Indices = new SortedDictionary<int, Stack<int>>();

            CreateBuffer();
        }

        /// <param name="bufferSize">The size of each large buffer created to house many "<paramref name="bufferChunkSize"/>" buffers</param>
        /// <param name="bufferChunkSize">The size of each individual buffer slot, this number should divide evenly into <paramref name="bufferSize"/></param>
        /// <param name="maxBufferSize">The maximum size that this buffer manager is allowed to allocate, this number should be a multiple of <paramref name="bufferSize"/></param>
        /// <exception cref="System.ArgumentException"/>
        public IOBufferManager(int bufferSize, int bufferChunkSize, int maxBufferSize) {
            if (bufferSize % bufferChunkSize != 0)
                throw new ArgumentException("bufferChunkSize", "BufferSize should devide evenly by BufferChunkSize");

            if (maxBufferSize % bufferSize != 0)
                throw new ArgumentException("maxBufferSize", "MaximumBufferSize should be a multiple of BufferSize");

            MaximumBufferSize = maxBufferSize;
            BufferSize = bufferSize;
            BufferChunkSize = bufferChunkSize;

            lockObj = new Object();
            Buffers = new List<Byte[]>();
            Indices = new SortedDictionary<int, Stack<int>>();

            CreateBuffer();
        }

        /// <summary>
        /// Returns a buffer slot that is not being used, will attempt to allocate a new buffer or null if it has reached MaximumBufferSize.
        /// A null result from this method should indicate some type of blocking mechanism until a buffer is free or increase MaximumBufferSize when appropriate.
        /// </summary>
        /// <returns></returns>
        public Slot GetBuffer() {
            lock (lockObj) {
                foreach (var pair in Indices) {
                    if (pair.Value.Count > 0)
                        return new Slot(Buffers[pair.Key], pair.Value.Pop());
                }
            }

            if (CreateBuffer())
                return new Slot(Buffers[^1], Indices[Buffers.Count - 1].Pop());

            return null;
        }

        /// <summary>
        /// Releases a buffer slot back to it's pool. This step is very important.
        /// </summary>
        /// <param name="slot">The slot to return.</param>
        public void FreeBuffer(Slot slot) {
            FreeBuffer(slot.Buffer, slot.Offset);
        }

        /// <summary>
        /// Releases a buffer slot back to it's pool. This step is very important.
        /// </summary>
        /// <param name="slot">The slot to return.</param>
        /// <param name="offset">The offset within the buffer where this slot starts.</param>
        public void FreeBuffer(byte[] buffer, int offset) {
            lock (lockObj) {
                int index = Buffers.IndexOf(buffer);

                if (index > -1)
                    Indices[index].Push(offset);

                CleanupCandidates();
            }
        }


        private bool CreateBuffer() {
            lock (lockObj) {

                if (MaximumBufferSize > 0 && Buffers.Count + 1 * BufferSize > MaximumBufferSize)
                    return false;

                Buffers.Add(new byte[BufferSize]);

                int index = Buffers.Count - 1;
                Stack<int> pool = new Stack<int>();

                for (int i = 0; i < BufferSize; i += BufferChunkSize)
                    pool.Push(i);

                Indices.Add(index, pool);
                return true;
            }
        }

        private void DestroyBuffer(int index) {
            lock (lockObj) {
                int lastindex = Buffers.Count - 1;

                if (index > 0) {
                    if (index < lastindex) {
                        Buffers[index] = Buffers[lastindex];
                        Indices[index] = Indices[lastindex];

                        Indices.Remove(lastindex);
                        Buffers.RemoveAt(lastindex);
                    }
                    else {
                        Indices.Remove(index);
                        Buffers.RemoveAt(index);
                    }
                }
            }
        }

        private void CleanupCandidates() 
        {
            //always leave one buffer in the pool
            var candidates = Indices.FindAll(pair => pair.Key > 0 && pair.Value.Count == (BufferSize / BufferChunkSize));
            if (candidates.Count() > 0) candidates.ForEach(s => DestroyBuffer(s.Key));
        }
    }
}
