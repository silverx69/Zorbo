using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Collections
{
    public sealed class SafeEnumerator<T> : IEnumerator<T>
    {
        int index = -1;

        List<T> sub;
        Predicate<T> rule;

        public T Current {
            get { return sub[index]; }
        }

        object System.Collections.IEnumerator.Current {
            get { return (object)Current; }
        }

        public SafeEnumerator(IEnumerable<T> @enum)
        {
            sub = new List<T>(@enum);
        }

        public void SetCustomRule(Predicate<T> selector) {
            this.rule = selector;
        }

        public void Dispose() {
            sub.Clear();
            sub = null;
        }

        public bool MoveNext() {

            if (rule != null) {

                while (++index < sub.Count)
                    if (rule(sub[index])) return true;

                return false;
            }
            else {
                if (++index >= sub.Count)
                    return false;

                return true;
            }
        }

        public void Reset() {
            index = -1;
        }
    }
}
