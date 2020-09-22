using Jurassic.Library;
using System;
using System.Collections.Generic;
using Zorbo.Core;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Enum : ScriptObject
    {
        readonly System.Type enumType;

        public override IEnumerable<PropertyNameAndValue> Properties {
            get {
                string[] names = System.Enum.GetNames(enumType);
                Array values = System.Enum.GetValues(enumType);

                for (int i = 0; i < names.Length; i++)
                    yield return new PropertyNameAndValue(
                        names[i], 
                        new PropertyDescriptor(values.GetValue(i), PropertyAttributes.FullAccess));
            }
        }

        public Enum(JScript script, Type enumType)
            : base(script.Engine) {
            this.enumType = enumType;
        }


        private object ToPrimitive(object value) {
            try {
                value = Convert.ChangeType(value, typeof(int));
            }
            catch {
                value = value.ToString();
            }
            return value;
        }

        protected override object GetMissingPropertyValue(object key) {
            string[] names = System.Enum.GetNames(enumType);
            Array values = System.Enum.GetValues(enumType);

            string propertyName = key.ToString();

            int index = names.FindIndex((s) => s == propertyName);
            if (index >= 0) 
                return ToPrimitive(values.GetValue(index));

            return base.GetMissingPropertyValue(propertyName);
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            Array values = System.Enum.GetValues(enumType);

            if (index < values.Length)
                return new PropertyDescriptor(ToPrimitive(values.GetValue(index)), PropertyAttributes.FullAccess);
            
            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}
