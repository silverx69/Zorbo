namespace Zorbo.Collections
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

        public SafeEnumerator(IEnumerable<T> @enum) {
            sub = [.. @enum];
        }

        public void SetCustomRule(Predicate<T> selector) {
            this.rule = selector;
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

        public void Dispose() {
            rule = null;
            sub.Clear();
            sub = null;
        }
    }
}
