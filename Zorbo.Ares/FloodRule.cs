using System;
using Zorbo.Ares.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Models;

namespace Zorbo.Ares
{
    public class FloodRule : ModelBase, IFloodRule
    {
#pragma warning disable IDE0044 // Add readonly modifier
        double count;
        double timeout;
#pragma warning restore IDE0044 // Add readonly modifier

        public AresId Id { get; }

        public string Name { get; }

        byte IFloodRule.Id {
            get { return (byte)Id; }
        }

        public double Count {
            get { return count; }
            set { OnPropertyChanged(() => count, value); }
        }

        public double Timeout {
            get { return timeout; }
            set { OnPropertyChanged(() => timeout, value); }
        }

        public bool Equals(IFloodRule other) {
            if (other == null)
                return false;

            return this.Id == (AresId)other.Id &&
                   this.Name == other.Name;
        }

        public FloodRule(string name, AresId id, double count, double timeout) {
            this.Id = id;
            this.Name = name;
            this.count = count;
            this.timeout = timeout;
        }

        public override bool Equals(object obj) {
            return Equals(obj as IFloodRule);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Id, Name);
        }
    }
}
