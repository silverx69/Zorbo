using Jurassic.Library;
using Zorbo.Core.Interfaces;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Channels : ScriptObject
    {
        readonly IChannelList channels;

        [JSProperty(Name = "addIpHits", IsEnumerable = true)]
        public uint AddIpHits {
            get { return channels?.AddIpHits ?? 0; }
        }

        [JSProperty(Name = "ackIpHits", IsEnumerable = true)]
        public uint AckIpHits {
            get { return channels?.AckIpHits ?? 0; }
        }

        [JSProperty(Name = "ackInfoHits", IsEnumerable = true)]
        public uint AckInfoHits {
            get { return channels?.AckInfoHits ?? 0; }
        }

        [JSProperty(Name = "sendInfoHits", IsEnumerable = true)]
        public uint SendInfoHits {
            get { return channels?.SendInfoHits ?? 0; }
        }

        [JSProperty(Name = "checkFirewallHits", IsEnumerable = true)]
        public uint CheckFirewallHits {
            get { return channels?.CheckFirewallHits ?? 0; }
        }

        [JSProperty(Name = "showRoom", IsEnumerable = true)]
        public bool Listing {
            get { return channels?.Listing ?? false; }
            set { if (channels != null) channels.Listing = value; }
        }

        [JSProperty(Name = "firewallOpen", IsEnumerable = true)]
        public bool FirewallOpen {
            get { return channels?.FirewallOpen ?? false; }
        }

        [JSProperty(Name = "finishedTestingFirewall", IsEnumerable = true)]
        public bool FinishedTestingFirewall {
            get { return channels?.FinishedTestingFirewall ?? false; }
        }

        [JSProperty(Name = "speedIn", IsEnumerable = true)]
        public virtual double SpeedIn {
            get { return channels.Monitor?.SpeedIn ?? 0; }
        }

        [JSProperty(Name = "speedOut", IsEnumerable = true)]
        public virtual double SpeedOut {
            get { return channels.Monitor?.SpeedOut ?? 0; }
        }

        [JSProperty(Name = "totalBytesIn", IsEnumerable = true)]
        public virtual double TotalBytesIn {
            get { return channels.Monitor?.TotalBytesIn ?? 0; }
        }

        [JSProperty(Name = "totalBytesOut", IsEnumerable = true)]
        public virtual double TotalBytesOut {
            get { return channels.Monitor?.TotalBytesOut ?? 0; }
        }

        public Channels(JScript script, IChannelList channels)
            : base(script.Engine) {

            this.channels = channels;
            this.PopulateFunctions();
        }
    }
}
