using System;
using System.Threading;
using Zorbo.Core.Data;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Ares.Server
{
    public class AresServerStats : IOMonitor, IServerStats
    {
        int peakusers;
        int joined;
        int parted;
        int rejected;
        int banned;
        int captchabanned;
        int invalidlogins;
        int floodtiggered;
        int packetssent;
        int packetsrecv;
        DateTime starttime;

        public int PeakUsers {
            get { return peakusers; }
            internal set {
                if (peakusers != value) {
                    Interlocked.Exchange(ref peakusers, value);
                    OnPropertyChanged();
                }
            }
        }

        public int Joined {
            get { return joined; }
            internal set {
                if (joined != value) {
                    Interlocked.Exchange(ref joined, value);
                    OnPropertyChanged();
                }
            }
        }

        public int Parted {
            get { return parted; }
            internal set {
                if (parted != value) {
                    Interlocked.Exchange(ref parted, value);
                    OnPropertyChanged();
                }
            }
        }

        public int Rejected {
            get { return rejected; }
            internal set {
                if (rejected != value) {
                    Interlocked.Exchange(ref rejected, value);
                    OnPropertyChanged();
                }
            }
        }

        public int Banned {
            get { return banned; }
            internal set {
                if (banned != value) {
                    Interlocked.Exchange(ref banned, value);
                    OnPropertyChanged();
                }
            }
        }

        public int CaptchaBanned {
            get { return captchabanned; }
            internal set {
                if (captchabanned != value) {
                    Interlocked.Exchange(ref captchabanned, value);
                    OnPropertyChanged();
                }
            }
        }

        public int InvalidLogins {
            get { return invalidlogins; }
            internal set {
                if (invalidlogins != value) {
                    Interlocked.Exchange(ref invalidlogins, value);
                    OnPropertyChanged();
                }
            }
        }

        public int FloodsTriggered {
            get { return floodtiggered; }
            internal set {
                if (floodtiggered != value) {
                    Interlocked.Exchange(ref floodtiggered, value);
                    OnPropertyChanged();
                }
            }
        }

        public int PacketsSent {
            get { return packetssent; }
            internal set {
                if (packetssent != value) {
                    Interlocked.Exchange(ref packetssent, value);
                    OnPropertyChanged();
                }
            }
        }

        public int PacketsReceived {
            get { return packetsrecv; }
            internal set {
                if (packetsrecv != value) {
                    Interlocked.Exchange(ref packetsrecv, value);
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartTime {
            get { return starttime; }
            internal set {
                if (!starttime.Equals(value)) {
                    starttime = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void Reset() {
            base.Reset();
            peakusers = 0;
            joined = 0;
            parted = 0;
            rejected = 0;
            banned = 0;
            captchabanned = 0;
            invalidlogins = 0;
            floodtiggered = 0;
            starttime = DateTime.MinValue;
        }
    }
}
