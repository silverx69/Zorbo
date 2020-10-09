using Jurassic.Library;
using System;
using System.Collections.Generic;
using Zorbo.Core;

namespace Javascript.Objects
{
    public class Enum : ScriptObject
    {
        readonly Type m_enumType;

        public Enum(Script script, Type enumType)
            : base(script.Engine) {
            m_enumType = enumType;
            var m_names = System.Enum.GetNames(m_enumType);
            var m_values = System.Enum.GetValues(m_enumType);
            for (int i = 0; i < m_names.Length; i++)
                DefineProperty(
                    m_names[i], 
                    new PropertyDescriptor(
                        ToPrimitive(m_values.GetValue(i)), 
                        PropertyAttributes.Enumerable | PropertyAttributes.Sealed), 
                    true);
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

        protected override string InternalClassName()
        {
            return m_enumType.Name;
        }
    }
}
