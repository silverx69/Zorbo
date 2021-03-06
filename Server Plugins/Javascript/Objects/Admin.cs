﻿using Jurassic.Library;
using System.Collections.Generic;
using Zorbo.Core.Server;

namespace Javascript.Objects
{
    public class Admins : ReadOnlyList
    {
        readonly IAdmins admin;

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return admin.Count; }
        }


        public Admins(Script script, IAdmins admin)
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
