using System.Text;

namespace Zorbo
{
    public static partial class StringBuilderExtensions
    {
        public static bool StartsWith(this StringBuilder sb, string value) {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));
            if (sb.Length < value.Length)
                return false;
            for (int i = 0; i < value.Length; i++)
                if (sb[i] != value[i]) return false;
            return true;
        }

        public static bool EndsWith(this StringBuilder sb, string value) {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));
            if (sb.Length < value.Length)
                return false;
            int start = sb.Length - value.Length;
            for (int i = 0; i < value.Length; i++)
                if (value[i] != sb[start + i]) return false;
            return true;
        }

        public static string ToStringFormat(this StringBuilder sb, params object[] args) {
            return string.Format(sb.ToString(), args);
        }
    }
}
