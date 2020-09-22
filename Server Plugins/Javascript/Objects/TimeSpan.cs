using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class TimeSpanInstance :
        ScriptObject, 
        IEquatable<TimeSpanInstance>
    {
        readonly JScript script;
        readonly TimeSpan span;

        [JSProperty(Name = "days", IsEnumerable = true)]
        public int Days { get { return span.Days; } }

        [JSProperty(Name = "hours", IsEnumerable = true)]
        public int Hours { get { return span.Hours; } }

        [JSProperty(Name = "minutes", IsEnumerable = true)]
        public int Minutes { get { return span.Minutes; } }

        [JSProperty(Name = "seconds", IsEnumerable = true)]
        public int Seconds { get { return span.Seconds; } }

        [JSProperty(Name = "milliseconds", IsEnumerable = true)]
        public int Milliseconds { get { return span.Milliseconds; } }

        [JSProperty(Name = "ticks", IsEnumerable = true)]
        public double Ticks { get { return span.Ticks; } }

        [JSProperty(Name = "totalDays", IsEnumerable = true)]
        public double TotalDays { get { return span.TotalDays; } }

        [JSProperty(Name = "totalHours", IsEnumerable = true)]
        public double TotalHours { get { return span.TotalHours; } }

        [JSProperty(Name = "totalMinutes", IsEnumerable = true)]
        public double TotalMinutes { get { return span.TotalMinutes; } }

        [JSProperty(Name = "totalSeconds", IsEnumerable = true)]
        public double TotalSeconds { get { return span.TotalSeconds; } }

        [JSProperty(Name = "totalMilliseconds", IsEnumerable = true)]
        public double TotalMilliseconds { get { return span.TotalMilliseconds; } }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly JScript script = null;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "TimeSpan", new TimeSpanInstance(script)) {

                this.script = script;
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public TimeSpanInstance Call(object d, object h, object m, object s, object ms) {
                return Construct(d, h, m, s, ms);
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public TimeSpanInstance Construct(object d, object h, object m, object s, object ms) {

                if ((s == null || s is Undefined) && (ms == null || ms is Undefined)) {

                    //new TimeSpan((long));
                    if ((h == null || h is Undefined) && (m == null || m is Undefined)) {

                        if (d is int || d is double) {
                            return new TimeSpanInstance(script, new TimeSpan(Convert.ToInt64(d) * TimeSpan.TicksPerMillisecond));
                        }

                    }
#pragma warning disable IDE0038 // Use pattern matching
                    else if (d is int && h is int && m is int) { //new TimeSpan(h, m, s) [d, h, m]
                        return new TimeSpanInstance(script, new TimeSpan((int)d, (int)h, (int)m));
                    }

                }
                else if (d is int && h is int && m is int && s is int && ms is int) {
#pragma warning restore IDE0038 // Use pattern matching
                    return new TimeSpanInstance(script, new TimeSpan((int)d, (int)h, (int)m, (int)s, (int)ms));
                }

                return null;
            }
        }

        #endregion

        private TimeSpanInstance(JScript script) 
            : base(script.Engine) {
            this.script = script;
            this.PopulateFunctions();
        }

        internal TimeSpanInstance(JScript script, TimeSpan span)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["TimeSpan"]).InstancePrototype) {

            this.script = script;
            this.span = span;
            this.PopulateFunctions();
        }

        [JSFunction(Name = "add", IsEnumerable = true, IsWritable = false)]
        public TimeSpanInstance Add(TimeSpanInstance a) {
            if (a == null)
                return this;
            return new TimeSpanInstance(script, a.span);
        }

        [JSFunction(Name = "subtract", IsEnumerable = true, IsWritable = false)]
        public TimeSpanInstance Subtract(TimeSpanInstance a) {
            if (a == null)
                return this;
            return new TimeSpanInstance(script, span.Subtract(a.span));
        }

        [JSFunction(Name = "duration", IsEnumerable = true, IsWritable = false)]
        public TimeSpanInstance Duration() {
            return new TimeSpanInstance(script, span.Duration());
        }

        [JSFunction(Name = "negate", IsEnumerable = true, IsWritable = false)]
        public TimeSpanInstance Negate() {
            return new TimeSpanInstance(script, span.Negate());
        }

        [JSFunction(Name = "compareTo", IsEnumerable = true, IsWritable = false)]
        public int CompareTo(TimeSpanInstance value) {
            if (value == null)
                return -1;
            return span.CompareTo(value.span);
        }

        [JSFunction(Name = "toString", IsConfigurable = true, IsWritable = false)]
        public string ToString(object format) {
            string str = string.Empty;

            if (format is string || format is ConcatenatedString)
                str = format.ToString();

            if (!string.IsNullOrEmpty(str))
                return span.ToString(str);

            return ToString();
        }

        public override string ToString() {
            return span.ToString();
        }

        public bool Equals(TimeSpanInstance other) {
            if (other == null)
                return false;

            return span.Equals(other.span);
        }

        public override bool Equals(object obj) {
            return Equals(obj as TimeSpanInstance);
        }

#pragma warning disable IDE0070 // Use 'System.HashCode'
        public override int GetHashCode() {
#pragma warning restore IDE0070 // Use 'System.HashCode'
            return span.GetHashCode();
        }
    }
}
