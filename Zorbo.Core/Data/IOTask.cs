using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Data
{
    public interface IOTask
    {
        object UserToken { get; set; }
        void   Execute(IOBuffer buffer);
    }

    public interface IOTask<TTask> : IOTask
        where TTask : class, IOTask<TTask>
    {
        event EventHandler<IOTaskCompleteEventArgs<TTask>> Completed;
    }

    public class IOTaskCompleteEventArgs<TTask> : EventArgs
    {
        public TTask Task {
            get;
            private set;
        }

        public IOBuffer Buffer {
            get;
            private set;
        }

        public IOTaskCompleteEventArgs(TTask task, IOBuffer buffer) {
            Task = task;
            Buffer = buffer;
        }
    }
}
