using Jurassic;
using Jurassic.Library;
using System.Collections.Generic;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Javascript.Objects
{
    public class Banned : Collection
    {
        readonly IHistory history;

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return this.history.Bans.Count; }
        }

        public Banned(Script script, IHistory history)
            : base(script, ((ClrFunction)script.Engine.Global["Collection"]).InstancePrototype) {
            
            this.script = script;
            this.history = history;

            this.PopulateFunctions();
        }

        [JSFunction(Name = "add", IsEnumerable = true, IsWritable = false)]
        public bool Add(object a) {

            if (a is User user) {
                this.history.Bans.Add(user.Client.ClientId);
                return true;
            }
            else if (a is UserId id) {
                this.history.Bans.Add(id);
                return true;
            }
            else if (a is UserRecord record) {
                this.history.Bans.Add(record.Record.ClientId);
                return true;
            }
            return false;
        }

        [JSFunction(Name = "remove", IsEnumerable = true, IsWritable = false)]
        public bool Remove(object a) {

            if (a is User user)
                return this.history.Bans.Remove(user.Client.ClientId);

            else if (a is UserId id) {
                return this.history.Bans.Remove(id);
            }
            else if (a is UserRecord r)
                return this.history.Bans.Remove(r.Record.ClientId);

            else if (a is FunctionInstance func) {

                for (int i = (Count - 1); i >= 0; i--) {

                    var ban = this.history.Bans[i];
                    var record = new UserRecord(script, this.history.Records.Find((s) => s.Equals(ban)));

                    object ret = func.Call(Engine.Global, record);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret)) {
                        this.history.Bans.RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }

        [JSFunction(Name = "removeAll", IsEnumerable = true, IsWritable = false)]
        public void RemoveAll(object a) {
            if (a is FunctionInstance func) {

                for (int i = (Count - 1); i >= 0; i--) {

                    var ban = this.history.Bans[i];
                    var record = new UserRecord(script, this.history.Records.Find((s) => s.Equals(ban)));

                    object ret = func.Call(Engine.Global, record);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        this.history.Bans.RemoveAt(i);
                }
            }
        }

        [JSFunction(Name = "removeAt", IsEnumerable = true, IsWritable = false)]
        public bool RemoveAt(int index) {
            this.history.Bans.RemoveAt(index);
            return true;
        }


        [JSFunction(Name = "clear", IsEnumerable = true, IsWritable = false)]
        public void Clear() {
            this.history.Bans.Clear();
        }
        
        [JSFunction(Name = "find", IsEnumerable = true, IsWritable = false)]
        public override object Find(object a) {
            if (a is FunctionInstance func) {

                for (int i = 0; i < this.history.Bans.Count; i++) {

                    var ban = this.history.Bans[i];
                    var record = new UserRecord(script, this.history.Records.Find((s) => s.Equals(ban)));

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

                for (int i = 0; i < this.history.Bans.Count; i++) {

                    var ban = this.history.Bans[i];
                    var record = new UserRecord(script, this.history.Records.Find((s) => s.Equals(ban)));

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

                for (int i = 0; i < this.history.Bans.Count; i++) {

                    var ban = this.history.Bans[i];
                    var record = new UserRecord(script, this.history.Records.Find((s) => s.Equals(ban)));

                    object ret = func.Call(Engine.Global, record);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return i;
                }
                return -1;
            }
            else if (a is User user) {
                return this.history.Bans.IndexOf(new UserId(script, user.Client.Guid, user.Client.ExternalIp));
            }
            else if (a is UserId id) {
                return this.history.Bans.IndexOf(id);
            }
            else if (a is UserRecord r) {
                return this.history.Bans.IndexOf(new UserId(script, r.Record.ClientId));
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
                    var ban = this.history.Bans[i];
                    var banned = new UserRecord(script, this.history.Records.Find(s => s.Equals(ban)));

                    yield return new PropertyNameAndValue(i.ToString(), new PropertyDescriptor(banned, PropertyAttributes.FullAccess));
                }

                yield return new PropertyNameAndValue(null, new PropertyDescriptor(null, PropertyAttributes.Sealed));
            }
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            if (index < Count) {
                var ban = this.history.Bans[(int)index];
                var banned = new UserRecord(script, this.history.Records.Find(s => s.Equals(ban)));

                return new PropertyDescriptor(banned, PropertyAttributes.FullAccess);
            }

            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}
