using Jurassic.Library;
using Zorbo.Core.Interfaces.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class RoomStats : Monitor
    {
        readonly IServerStats stats;

        [JSProperty(Name = "startTime", IsEnumerable = true)]
        public DateInstance StartTime {
            get {
                return stats != null ?
                    Engine.Date.Construct(
                        stats.StartTime.Year, 
                        stats.StartTime.Month - 1, 
                        stats.StartTime.Day,
                        stats.StartTime.Hour,
                        stats.StartTime.Minute,
                        stats.StartTime.Second,
                        stats.StartTime.Millisecond) :
                    Engine.Date.Construct();
            }
        }

        [JSProperty(Name = "peakUsers", IsEnumerable = true)]
        public int PeakUsers {
            get { return stats?.PeakUsers ?? 0; }
        }

        [JSProperty(Name = "joined", IsEnumerable = true)]
        public int Joined {
            get { return stats?.Joined ?? 0; }
        }

        [JSProperty(Name = "parted", IsEnumerable = true)]
        public int Parted {
            get { return stats?.Parted ?? 0; }
        }

        [JSProperty(Name = "rejected", IsEnumerable = true)]
        public int Rejected {
            get { return stats?.Rejected ?? 0; }
        }

        [JSProperty(Name = "banned", IsEnumerable = true)]
        public int Banned {
            get { return stats?.Banned ?? 0; }
        }

        [JSProperty(Name = "captchaBanned", IsEnumerable = true)]
        public int CaptchaBanned {
            get { return stats?.CaptchaBanned ?? 0; }
        }

        [JSProperty(Name = "invalidLogins", IsEnumerable = true)]
        public int InvalidLogins {
            get { return stats?.InvalidLogins ?? 0; }
        }

        [JSProperty(Name = "floodsTriggered", IsEnumerable = true)]
        public int FloodsTriggered {
            get { return stats?.FloodsTriggered ?? 0; }
        }

        [JSProperty(Name = "packetsSent", IsEnumerable = true)]
        public int PacketsSent {
            get { return stats?.PacketsSent ?? 0; }
        }

        [JSProperty(Name = "packetsReceived", IsEnumerable = true)]
        public int PacketsReceived {
            get { return stats?.PacketsReceived ?? 0; }
        }

        [JSProperty(Name = "speedIn", IsEnumerable = true)]
        public override double SpeedIn {
            get { return stats?.SpeedIn ?? 0; }
        }

        [JSProperty(Name = "speedOut", IsEnumerable = true)]
        public override double SpeedOut {
            get { return stats?.SpeedOut ?? 0; }
        }

        [JSProperty(Name = "totalBytesIn", IsEnumerable = true)]
        public override double TotalBytesIn {
            get { return stats?.TotalBytesIn ?? 0; }
        }

        [JSProperty(Name = "totalBytesOut", IsEnumerable = true)]
        public override double TotalBytesOut {
            get { return stats?.TotalBytesOut ?? 0; }
        }

        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "RoomStats", new RoomStats(script)) {
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public User Call() {
                return null;
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public User Construct() {
                return null;
            }
        }

        #endregion

        protected RoomStats(JScript script)
            : base(script, ((ClrFunction)script.Engine.Global["Monitor"]).InstancePrototype) {
            this.PopulateFunctions();
        }

        public RoomStats(JScript script, IServerStats stats)
            : base(script, stats) {

            this.stats = stats;
            this.PopulateFunctions();
        }
    }
}
