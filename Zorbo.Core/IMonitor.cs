namespace Zorbo
{
    public interface IMonitor : IObservable
    {
        long SpeedIn { get; }
        long SpeedOut { get; }
        long LastBytesIn { get; }
        long LastBytesOut { get; }
        long TotalBytesIn { get; }
        long TotalBytesOut { get; }

        void Start();
        void Reset();
        void Stop();

        void AddInput(long numbytes);
        void AddOutput(long numbytes);
    }
}
