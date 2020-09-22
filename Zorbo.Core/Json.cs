using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Zorbo.Core.Converters;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Core
{
    public static class Json
    {
        public static readonly JsonConverter[] Converters;
        private static readonly Regex IdRegex = new Regex("(['\"]*)id\\1:\\s*(['\"]*)(\\d+)\\2", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        static Json() {
            Converters = new JsonConverter[] {
                JsonByteArrayConverter.Default,
                JsonIPAddressConverter.Default
            };
        }

        public static string Serialize(object obj) 
        {
            if (obj is UnknownJson json)
                return Encoding.UTF8.GetString(json.Payload);

            return JsonConvert.SerializeObject(obj, Converters);
        }

        public static string Serialize(object obj, Formatting formatting)
        {
            if (obj is UnknownJson json)
                return Encoding.UTF8.GetString(json.Payload);

            return JsonConvert.SerializeObject(obj, formatting, Converters);
        }

        public static T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, Converters);
        }

        public static T Deserialize<T>(byte[] buffer, int index, int count)
        {
            return Deserialize<T>(Encoding.UTF8.GetString(buffer, index, count));
        }

        public static int ExtractId(this string json)
        {
            Match match = IdRegex.Match(json);
            if (match.Success && byte.TryParse(match.Groups[3].Value, out byte id))
                return id;
            return -1;
        }
    }
}
