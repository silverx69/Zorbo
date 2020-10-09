using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Javascript.Objects
{
    public class Records : Collection
    {
        readonly IHistory history;

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return this.history.Records.Count; }
        }

        public Records(Script script, IHistory history)
            : base(script, ((ClrFunction)script.Engine.Global["Collection"]).InstancePrototype) {

            this.history = history;
            this.PopulateFunctions();
        }

        [JSFunction(Name = "clear", IsEnumerable = true, IsWritable = false)]
        public void Clear() {
            this.history.Bans.Clear();
        }

        [JSFunction(Name = "find", IsEnumerable = true, IsWritable = false)]
        public override object Find(object a) {

            if (a is FunctionInstance func) {

                for (int i = 0; i < this.history.Records.Count; i++) {

                    var record = new UserRecord(script, this.history.Records[i]);
                    object ret = func.Call(Engine.Global, record);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return record;
                }
            }

            return null;
        }

        [JSFunction(Name = "findAll", IsEnumerable = true, IsWritable = false)]
        public override Collection FindAll(object a) {

            if (a is FunctionInstance func) {

                var list = new List(script);

                for (int i = 0; i < this.history.Records.Count; i++) {

                    var record = new UserRecord(script, this.history.Records[i]);
                    object ret = func.Call(Engine.Global, record);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        list.Add(record);
                }

                return list;
            }

            return null;
        }

        [JSFunction(Name = "indexOf", IsEnumerable = true, IsWritable = false)]
        public override int IndexOf(object a) {

            if (a is FunctionInstance func) {
                
                for (int i = 0; i < this.history.Records.Count; i++) {

                    var record = new UserRecord(script, this.history.Records[i]);
                    object ret = func.Call(Engine.Global, record);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return i;
                }
                return -1;
            }
            else if (a is User user) {
                return this.history.Records.FindIndex((s) => s.ClientId.Equals(user.Client.ClientId));
            }
            else if (a is UserId id) {
                return this.history.Records.FindIndex((s) => s.ClientId.Equals(id));
            }
            else if (a is UserRecord r) {
                return this.history.Records.IndexOf(r.Record);
            }

            return -1;
        }

        [JSFunction(Name = "contains", IsEnumerable = true, IsWritable = false)]
        public override bool Contains(object a) {
            return (IndexOf(a) > -1);
        }

        public override IEnumerable<PropertyNameAndValue> Properties {
            get {
                for (int i = 0; i < this.Count; i++) {
                    var record = new UserRecord(script, this.history.Records[i]);
                    yield return new PropertyNameAndValue(i.ToString(), new PropertyDescriptor(record, PropertyAttributes.FullAccess));
                }
            }
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            if (index < Count) {
                var record = new UserRecord(script, this.history.Records[(int)index]);
                return new PropertyDescriptor(record, PropertyAttributes.FullAccess);
            }

            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}
