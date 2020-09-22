using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Data.Packets
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PacketItemAttribute : Attribute
    {
        bool @null = true;
        bool prefix = false;

        /// <summary>
        /// The zero-based index indicating the packet item(s) ordering upon serialization
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Will read the current property by a statically set length (if a length is required array, string, etc)
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The maximum length of the (String) property be written or read. 
        /// In the case of reading, the entire string is read and then shortened.
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the (String) property
        /// is null terminated and will not be read with a static length
        /// </summary>
        public bool NullTerminated {
            get { return @null; }
            set { @null = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the (String) property
        /// is written with a length prefix and not null terminated
        /// </summary>
        public bool LengthPrefix {
            get { return prefix; }
            set { prefix = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating that this property can be excluded
        /// from deserialization if there is not enough data to parse it. 
        /// When serializing, <see cref="OptionalValue"/> can be used to control omission
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// If Optional flag is true, gets or sets a value indicating that a packet 
        /// item can be excluded if the property is equal to the OptionalValue
        /// </summary>
        public object OptionalValue { get; set; }


        public PacketItemAttribute(int index) {
            Index = index;
        }
    }
}
