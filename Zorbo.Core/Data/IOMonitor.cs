namespace Zorbo.Data
{
    public class IOMonitor : Observable, IMonitor
    {
        long speedIn = 0;
        long speedOut = 0;

        long lastIn = 0;
        long lastOut = 0;

        long totalIn = 0;
        long totalOut = 0;

        long currentIn = 0;
        long currentOut = 0;

        volatile bool running;
        readonly Lock updateLock = new();

        public bool Running { get { return running; } }

        public long SpeedIn { get { return speedIn; } }

        public long SpeedOut { get { return speedOut; } }

        public long LastBytesIn { get { return lastIn; } }

        public long LastBytesOut { get { return lastOut; } }

        public long TotalBytesIn { get { return totalIn; } }

        public long TotalBytesOut { get { return totalOut; } }

        static readonly Timer timer;
        static readonly List<IOMonitor> monitors;

        static IOMonitor() {
            monitors = [];
            timer = new Timer(Tick, null, 1000, 1000);
        }

        static void Tick(object state) {
            for (int i = 0; i < monitors.Count; i++)
                monitors[i].UpdateSpeed();
        }

        public IOMonitor(bool start = false) {
            if (start) Start();
        }

        public void Start() {
            monitors.Add(this);
            running = true;
        }

        public virtual void Reset() {
            if (running)
                Stop();

            speedIn = 0;
            speedOut = 0;
            lastIn = 0;
            lastOut = 0;
            totalIn = 0;
            totalOut = 0;
            currentIn = 0;
            currentOut = 0;
        }

        public void Stop() {
            monitors.Remove(this);
            running = false;
        }

        public void AddInput(long numbytes) {
            if (running) {
                lock(updateLock) {
                    totalIn += numbytes;
                    currentIn += numbytes;
                    lastIn = numbytes;
                }
                OnPropertyChanged(nameof(LastBytesIn));
                OnPropertyChanged(nameof(TotalBytesIn));
            }
        }

        public void AddOutput(long numbytes) {
            if (running) {
                lock(updateLock) {
                    totalOut += numbytes;
                    currentOut += numbytes;
                    lastOut = numbytes;
                }
                OnPropertyChanged(nameof(LastBytesOut));
                OnPropertyChanged(nameof(TotalBytesOut));
            }
        }

        // TODO: implement some sort of averaging
        protected void UpdateSpeed() {
            if (running) {
                lock(updateLock) {
                    speedIn = currentIn;
                    speedOut = currentOut;
                }
                OnPropertyChanged(nameof(SpeedIn));
                OnPropertyChanged(nameof(SpeedOut));
                currentIn = 0;
                currentOut = 0;
            }
        }
    }
}
