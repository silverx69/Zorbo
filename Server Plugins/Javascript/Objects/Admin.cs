using Jurassic.Library;
using System.Collections.Generic;
using Zorbo.Core.Interfaces.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Admin : ReadOnlyList
    {
        readonly IAdmins<Zorbo.Core.Interfaces.Password> admin;

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return admin.Count; }
        }


        public Admin(JScript script, IAdmins<Zorbo.Core.Interfaces.Password> admin)
            : base(script) {

            this.admin = admin;
            this.PopulateFunctions();
        }

        public override IEnumerable<PropertyNameAndValue> Properties {
            get {
                int i = -1;
                foreach (var admin in this.admin) {

                    var user = script.Room.Users.Items.Find((s) => ((User)s).Id == admin.Id);
                    yield return new PropertyNameAndValue((++i).ToString(), new PropertyDescriptor(user, PropertyAttributes.FullAccess));
                }
            }
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            if (index < this.admin.Count) {
                var admin = this.admin[(int)index];
                var user = script.Room.Users.Items.Find((s) => ((User)s).Id == admin.Id);

                return new PropertyDescriptor(user, PropertyAttributes.FullAccess);
            }
            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}
