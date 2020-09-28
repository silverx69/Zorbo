using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Zorbo.Core.Models;

namespace Zorbo.Core
{
    public static partial class Extensions
    {
        public static readonly Regex FullRegex;
        public static readonly Regex ReadRegex;
        public static readonly Regex StripRegex;
        public static readonly Regex TopicRegex;

        static Extensions() {
            string emote_pat = BuildEmoteRegex();
            string strip_pat = "\u0003|\u0005|\u0006|\a|\t";
            string link_pat = "arlnk://|www\\.|https?://|ftps?://|wss?://";
            ReadRegex = new Regex("\u0002(3|5|6|7|9)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            StripRegex = new Regex(strip_pat, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            TopicRegex = new Regex(strip_pat + "|" + emote_pat, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            FullRegex = new Regex(strip_pat + "|" + link_pat + "|" + emote_pat, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        public static uint ToUInt32(this IPAddress address)
        {
            return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
        }

        public static IPAddress ToIPAddress(this uint address)
        {
            return new IPAddress(BitConverter.GetBytes(address));
        }

        public static bool IsValidInternalIp(this IPAddress address)
        {
            return address != null && !Equals(address, IPAddress.Any);
        }

        public static bool IsLocalMachine(this IPAddress address)
        {
            if (IPAddress.IsLoopback(address) ||
                address.IsIPv6LinkLocal ||
                address.IsIPv6SiteLocal) return true;
            return Utils.GetLocalAddresses().Contains(address);
        }

        public static bool IsLocalAreaNetwork(this IPAddress address)
        {
            if (IPAddress.IsLoopback(address) ||
                address.IsIPv6LinkLocal ||
                address.IsIPv6SiteLocal) return true;

            byte[] b = address.GetAddressBytes();

            if (b[0] == 10 ||
               (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
               (b[0] == 192 && b[1] == 168)) return true;

            return false;
        }

        public static string ToNativeString(this SecureString secure)
        {
            IntPtr ptr = IntPtr.Zero;

            try {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secure);
                return Marshal.PtrToStringUni(ptr);
            }
            finally {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        public static SecureString ToSecureString(this string unsecure)
        {
            SecureString ret = new SecureString();
            foreach (char c in unsecure) ret.AppendChar(c);

            ret.MakeReadOnly();
            return ret;
        }

        public static ColorCode ToColorCode(this string htmlcolor)
        {
            return (htmlcolor.ToUpper()) switch
            {
                "#FFFFFF" => ColorCode.White,
                "#000000" => ColorCode.Black,
                "#000080" => ColorCode.Navy,
                "#008000" => ColorCode.Green,
                "#FF0000" => ColorCode.Red,
                "#800000" => ColorCode.Maroon,
                "#800080" => ColorCode.Purple,
                "#FFA500" => ColorCode.Orange,
                "#FFFF00" => ColorCode.Yellow,
                "#00FF00" => ColorCode.Lime,
                "#008080" => ColorCode.Teal,
                "#00FFFF" => ColorCode.Aqua,
                "#0000FF" => ColorCode.Blue,
                "#FF00FF" => ColorCode.Fuchsia,
                "#808080" => ColorCode.Gray,
                "#C0C0C0" => ColorCode.Silver,
                _ => ColorCode.None,
            };
        }

        public static string ToHtmlColor(this int color)
        {
            return ((ColorCode)color).ToHtmlColor();
        }

        public static string ToHtmlColor(this ColorCode color)
        {
            return color switch
            {
                ColorCode.White => "#FFFFFF",
                ColorCode.Black => "#000000",
                ColorCode.Navy => "#000080",
                ColorCode.Green => "#008000",
                ColorCode.Red => "#FF0000",
                ColorCode.Maroon => "#800000",
                ColorCode.Purple => "#800080",
                ColorCode.Orange => "#FFA500",
                ColorCode.Yellow => "#FFFF00",
                ColorCode.Lime => "#00FF00",
                ColorCode.Teal => "#008080",
                ColorCode.Aqua => "#00FFFF",
                ColorCode.Blue => "#0000FF",
                ColorCode.Fuchsia => "#FF00FF",
                ColorCode.Gray => "#808080",
                ColorCode.Silver => "#C0C0C0",
                _ => string.Empty,
            };
        }

        public static string StripColor(this string text)
        {
            int lastMatch = 0;
            string text2 = string.Empty;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            Match match = StripRegex.Match(text);
            while (match.Success) {
                text2 += text[lastMatch..match.Index];
                lastMatch = match.Index + match.Length;
                switch (match.Value) {
                    case "\u0003":
                        if (text.Length >= lastMatch + 7 && text[lastMatch] == '#') {
                            lastMatch += 7;
                            break;
                        }

                        if (text.Length >= lastMatch + 2 && int.TryParse(text.Substring(lastMatch, 2), out _))
                            lastMatch += 2;
                        break;
                    case "\u0005":
                        if (text.Length >= lastMatch + 7 && text[lastMatch] == '#') {
                            lastMatch += 7;
                            break;
                        }
                        if (text.Length >= lastMatch + 2 && int.TryParse(text.Substring(lastMatch, 2), out _)) {
                            lastMatch += 2;
                        }
                        break;
                }
                match = StripRegex.Match(text, lastMatch);
            }
            text2 += text[lastMatch..];
            return text2;
        }

        public static string ToAresColor(this string text)
        {
            int lastMatch = 0;
            string text2 = string.Empty;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            Match match = ReadRegex.Match(text);
            while (match.Success) {
                text2 += text[lastMatch..match.Index];
                lastMatch = match.Index + match.Length;
                text2 += Convert.ToChar(int.Parse(match.Groups[1].Value));
                match = ReadRegex.Match(text, lastMatch);
            }
            text2 += text[lastMatch..];
            return text2;
        }

        public static string ToReadableColor(this string text)
        {
            int lastMatch = 0;
            string text2 = string.Empty;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            Match match = StripRegex.Match(text);
            while (match.Success) {
                text2 += text[lastMatch..match.Index];
                lastMatch = match.Index + match.Length;
                text2 += string.Format("\u0002{0}", (int)match.Value[0]);
                match = StripRegex.Match(text, lastMatch);
            }
            text2 += text[lastMatch..];
            return text2;
        }

        static string BuildEmoteRegex()
        {
            var sb = new StringBuilder();
            foreach (Emoticon emote in Emotes.Images)
                sb.AppendFormat("{0}|", Regex.Escape(emote.Key));
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static string AnonUsername(this IPAddress address)
        {
            byte[] addr = address.GetAddressBytes();

            Array.Reverse(addr);

            byte[] hash = Utils.SHA1.ComputeHash(addr);

            return "anon_" + BitConverter.ToString(hash.Take(4).ToArray()).Replace("-", string.Empty);
        }

        public static string FormatUsername(this String @string)
        {
            string result = Regex.Replace(
                @string,
                "￼|­|\"|'|`|/|\\\\|www'.",
                "").Replace('_', ' ');

            return result;
        }


        public static string Repeat(this string @string, int count)
        {
            count = count <= 0 ? 1 : count;//if 0 return input
            var sb = new StringBuilder();

            for (int i = 0; i < count; i++)
                sb.Append(@string);

            return sb.ToString();
        }

        public static bool ContainsAny(this string @string, IEnumerable<String> collection)
        {
            foreach (string str in collection)
                if (@string.Contains(str)) return true;

            return false;
        }

        public static string Join(this IEnumerable<string> collection, string delim = ", ") 
        {
            var sb = new StringBuilder();
            foreach (string str in collection) {
                sb.Append(str);
                sb.Append(delim);
            }
            sb.Remove(sb.Length - delim.Length, delim.Length);
            return sb.ToString();
        }

        public static IObservableCollection<TResult> Cast<TResult>(this IObservableCollection collection)
        {
            var cast = ((IEnumerable)collection).Cast<TResult>();
            if (cast is ModelList<TResult> list)
                return list;
            return new ModelList<TResult>(cast);
        }

        public static IObservableCollection<T> ToObservable<T>(this IEnumerable<T> collection)
        {
            if (collection is ModelList<T> list)
                return list;
            return new ModelList<T>(collection);
        }

        public static bool Contains<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            foreach (T item in collection)
                if (predicate(item)) return true;

            return false;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }


        public static T Find<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            foreach (var item in collection)
                if (predicate(item)) return item;

            return default;
        }

        public static int FindIndex<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            int index = -1;
            foreach (var item in collection) {
                ++index;
                if (predicate(item)) return index;
            }
            return -1;
        }

        public static IEnumerable<T> FindAll<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            var ret = new List<T>();

            foreach (var item in collection)
                if (predicate(item)) ret.Add(item);

            return ret;
        }


        public static T Find<T>(this ObservableCollection<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
                if (predicate(list[i])) return list[i];

            return default;
        }

        public static void Sort<T>(this ObservableCollection<T> list, Comparison<T> comparison)
        {
            for (int i = list.Count - 1; i >= 0; i--) {

                for (int j = 1; j <= i; j++) {

                    T o1 = list[j - 1];
                    T o2 = list[j];

                    if (comparison(o1, o2) > 0)
                        list.Move(j - 1, j);
                }
            }
        }

        public static void RemoveAll<T>(this ObservableCollection<T> list, Predicate<T> predicate)
        {
            for (int i = (list.Count - 1); i >= 0; i--)
                if (predicate(list[i])) list.RemoveAt(i);

        }


        /*****************************************************
                    DICTIONARY EXTENSIONS
         *****************************************************/

        /// <summary>
        /// An extension method for Dictionaries to perform ForEach lambda expressions
        /// </summary>
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>> action)
        {

            foreach (var keypair in dictionary)
                action(keypair);
        }

        /// <summary>
        /// An extension method for Dictionaries to perform Find lambda expressions
        /// </summary>
        public static KeyValuePair<TKey, TValue> Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            foreach (var pair in dictionary)
                if (predicate(pair)) return pair;

            return default;
        }

        /// <summary>
        /// An extension method for Dictionaries to perform Find lambda expressions
        /// </summary>
        public static KeyValuePair<TKey, TValue> Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate, int startIndex)
        {
            int index = -1;
            foreach (var pair in dictionary) {
                index++;
                if (index >= startIndex && predicate(pair)) return pair;
            }
            return default;
        }

        /// <summary>
        /// An extension method for Dictionaries to perform FindAll lambda expressions
        /// </summary>
        public static IEnumerable<KeyValuePair<TKey, TValue>> FindAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            var ret = new List<KeyValuePair<TKey, TValue>>();

            foreach (var pair in dictionary)
                if (predicate(pair)) ret.Add(pair);

            return ret;
        }
    }
}
