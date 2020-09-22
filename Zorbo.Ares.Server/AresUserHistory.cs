using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using Zorbo.Ares.Server.Users;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server
{
    public class AresUserHistory : ModelBase, IHistory<Password>
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

        IAdmins<Password> IHistory<Password>.Admin {
            get { return admins; }
        }

        IBanned IHistory<Password>.Bans {
            get { return bans; }
        }

        IRangeBanned IHistory<Password>.RangeBans {
            get { return rBans; }
        }

        IObservableCollection<Record> IHistory<Password>.Records {
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
            var record = Records.Find((s) => s.Equals(client));

            if (record == null) {
                record = new Record();
                Records.Add(record);
            }

            record.ClientId = client.ClientId;
            record.Name = client.Name;
            record.Muzzled = client.Muzzled;
            record.LastSeen = DateTime.Now;

            if (record.Trusted && client.IsCaptcha)
                record.Trusted = false;

            return record;
        }
    }
}
