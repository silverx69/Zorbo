using Newtonsoft.Json;
using System;
using Zorbo.Ares.Server.Users;
using Zorbo.Core;
using Zorbo.Core.Models;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Server
{
    public class AresUserHistory : ModelBase, IHistory
    {
#pragma warning disable IDE0044 // Add readonly modifier
        Admin admins = null;

        Banned bans = null;
        RangeBanned rBans = null;
        ModelList<Record> records = null;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonIgnore]
        public Admin Admin { 
            get { return admins; }
            set { OnPropertyChanged(() => admins, value); }
        }

        [JsonProperty("records")]
        public ModelList<Record> Records {
            get { return records; }
            set { OnPropertyChanged(() => records, value); }
        }

        [JsonProperty("bans")]
        public Banned Bans { 
            get { return bans; }
            set { OnPropertyChanged(() => bans, value); }
        }

        [JsonProperty("rangeBans")]
        public RangeBanned RangeBans { 
            get { return rBans; }
            set { OnPropertyChanged(() => rBans, value); }
        }

        [JsonIgnore]
        public DateTime LastSaved { get; set; }

        #region " IHistory "

        IAdmins IHistory.Admin {
            get { return admins; }
        }

        IBanned IHistory.Bans {
            get { return bans; }
        }

        IRangeBanned IHistory.RangeBans {
            get { return rBans; }
        }

        IObservableCollection<Record> IHistory.Records {
            get { return Records; }
        }

        #endregion

        public AresUserHistory() {
            this.admins = new Admin();
            this.bans = new Banned();
            this.rBans = new RangeBanned();
            this.records = new ModelList<Record>();
            this.LastSaved = DateTime.Now;
        }

        public Record Add(IClient client) {
            bool isnew = false;
            var record = Records.Find((s) => s.Equals(client));

            if (record == null) {
                isnew = true;
                record = new Record();
            }

            record.ClientId = client.ClientId;
            record.Name = client.Name;
            record.Muzzled = client.Muzzled;
            record.LastSeen = DateTime.Now;

            if (record.Trusted && client.IsCaptcha)
                record.Trusted = false;

            if (isnew) Records.Add(record);

            return record;
        }
    }
}
