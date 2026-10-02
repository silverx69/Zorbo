using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Data
{
    public sealed class IOBuffer
    {
        public int Count {
            get;
            internal set;
        }

        public int Offset {
            get;
            internal set;
        }

        public byte[] Buffer {
            get;
            internal set;
        }


        public void SetBuffer(int offset, int count) {
            Count = count;
            Offset = offset;
        }

        public void SetBuffer(byte[] buffer, int offset, int count) {
            Count = count;
            Offset = offset;
            Buffer = buffer;
        }


        public void Release() {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Completed;
    }
}
