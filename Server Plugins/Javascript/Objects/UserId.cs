using Jurassic.Library;
using System;
using System.Net;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class UserId : ScriptObject
    {
        ClientId client_id;

        [JSProperty(Name = "guid", IsEnumerable = true)]
        public string Guid {
            get { return client_id.Guid.ToString(); }
            set {
                if (System.Guid.TryParse(value, out Guid g))
                    client_id = new ClientId(g, client_id.ExternalIp);
            }
        }

        [JSProperty(Name = "externalIp", IsEnumerable = true)]
        public string ExternalIp {
            get { return client_id.ExternalIp.ToString(); }
            set {
                if (IPAddress.TryParse(value, out IPAddress a))
                    client_id = new ClientId(client_id.Guid, a);
            }
        }
        

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly JScript script;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "UserId", new UserId(script)) {

                this.script = script;
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public UserId Call(string a, string b) {


                if (System.Guid.TryParse(a, out Guid guid) && IPAddress.TryParse(b, out IPAddress address))
                    return new UserId(script, guid, address);

                return null;
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public UserId Construct(string a, string b) {
                return Call(a, b);
            }
        }

        #endregion


        private UserId(JScript script)
            : base(script.Engine) { }

        public UserId(JScript script, ClientId id)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["UserId"]).InstancePrototype)
        {
            client_id = id;
            this.PopulateFunctions();
        }

        public UserId(JScript script, Guid guid, IPAddress address)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["UserId"]).InstancePrototype) 
        {
            client_id = new ClientId(guid, address);
            this.PopulateFunctions();
        }

        public bool Equals(IClient other) {

            return (other != null && (
                other.Guid.Equals(Guid) ||
                other.ExternalIp.Equals(ExternalIp)));
        }

        public bool Equals(ClientId other) {

            return (other.Guid.Equals(Guid) || other.ExternalIp.Equals(ExternalIp));
        }

        public bool Equals(Record other) {

            return (other.ClientId.Guid.Equals(Guid) || other.ClientId.ExternalIp.Equals(ExternalIp));
        }

        public static implicit operator ClientId(UserId a)
        {
            return a.client_id;
        }
    }
}
