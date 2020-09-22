using Jurassic.Library;
using System;
using System.Linq;
using Zorbo.Core.Interfaces;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Avatar : ScriptObject, IAvatar
    {
        [JSProperty(Name = "smallBytes", IsEnumerable = true)]
        public ArrayInstance SmallBytes { get; }

        [JSProperty(Name = "largeBytes", IsEnumerable = true)]
        public ArrayInstance LargeBytes { get; }

        byte[] IAvatar.SmallBytes {
            get { return SmallBytes.ToArray<byte>(); }
        }

        byte[] IAvatar.LargeBytes {
            get { return LargeBytes.ToArray<byte>(); }
        }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly JScript script = null;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "Avatar", new Avatar(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public Avatar Call(ArrayInstance smallbytes, object largebytes) {

                if (!(largebytes is ArrayInstance))
                    return new Avatar(script, this.InstancePrototype, smallbytes);

                return new Avatar(script, this.InstancePrototype, smallbytes, (ArrayInstance)largebytes);    
            }

            [JSConstructorFunction]
            public Avatar Construct(ArrayInstance smallbytes, object largebytes) {
                return Call(smallbytes, largebytes);
            }
        }

        #endregion

        internal Avatar(JScript script)
            : base(script.Engine) {

            this.PopulateFunctions();
        }

        public Avatar(JScript script, ObjectInstance prototype, ArrayInstance bytes)
            : base(script.Engine, prototype) {

            this.PopulateFunctions();

            this.SmallBytes = bytes;
            this.LargeBytes = bytes;
        }

        public Avatar(JScript script, ObjectInstance prototype, ArrayInstance smallbytes, ArrayInstance largebytes)
            : base(script.Engine, prototype) {

            this.PopulateFunctions();

            this.SmallBytes = smallbytes;
            this.LargeBytes = largebytes;
        }

        public Avatar(JScript script, IAvatar avatar)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["Avatar"]).InstancePrototype) {

            this.PopulateFunctions();

            if (avatar != null) {
                this.SmallBytes = avatar.SmallBytes.ToJSArray(script.Engine);
                this.LargeBytes = avatar.LargeBytes.ToJSArray(script.Engine);
            }
        }

        public bool Equals(IAvatar other) {

            if (other == null ||
                SmallBytes == null ||
                other.SmallBytes == null) 
                return Object.ReferenceEquals(this, other);

            return ((IAvatar)this).SmallBytes.SequenceEqual(other.SmallBytes);
        }

        public override bool Equals(object obj) {
            return Equals(obj as Avatar);
        }


        public static bool operator ==(Avatar a, IAvatar b) {

            if (object.ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Avatar a, IAvatar b) {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SmallBytes, LargeBytes);
        }
    }
}
