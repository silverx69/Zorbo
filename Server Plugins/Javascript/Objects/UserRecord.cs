using Jurassic.Library;
using System;
using System.Net;
using Zorbo.Core.Interfaces.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class UserRecord : ScriptObject
    {
        readonly Record record;

        internal Record Record {
            get { return record; } 
        }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "UserRecord", new UserRecord(script)) {
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

        private UserRecord(JScript script)
            : base(script.Engine) {
            this.PopulateFunctions();
        }

        public UserRecord(JScript script, Record record)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["UserRecord"]).InstancePrototype) {

            this.record = record;
            this.PopulateFunctions();
        }

        [JSProperty(Name = "guid", IsEnumerable = true)]
        public String Guid {
            get { return record.ClientId.Guid.ToString(); }
        }

        [JSProperty(Name = "name", IsEnumerable = true)]
        public String Name {
            get { return record.Name; }
        }

        [JSProperty(Name = "trusted", IsEnumerable = true)]
        public bool Trusted {
            get { return record.Trusted; }
            set { record.Trusted = value; }
        }

        [JSProperty(Name = "muzzled", IsEnumerable = true)]
        public bool Muzzled {
            get { return record.Muzzled; }
            set { record.Muzzled = value; }
        }

        [JSProperty(Name = "dnsName", IsEnumerable = true)]
        public String DnsName {
            get { return record.DnsName; }
        }

        [JSProperty(Name = "externalIp", IsEnumerable = true)]
        public String ExternalIp {
            get { return record.ClientId.ExternalIp.ToString(); }
        }

        public override bool Equals(object obj) {
            return record.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(record);
        }
    }
}
