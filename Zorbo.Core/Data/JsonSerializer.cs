using System.Text;
using System.Text.Json;
using Zorbo.Data.Converters;

namespace Zorbo.Data
{
    using Json = System.Text.Json.JsonSerializer;
    /// <summary>
    /// A simple System.Text.Json serializer api with some useful default options.
    /// </summary>
    public static class JsonSerializer
    {
        public static readonly JsonSerializerOptions Options;

        static JsonSerializer() {
            Options = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
#if DEBUG
                WriteIndented = true,
#endif
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                    new JsonByteArrayConverter(),
                    new JsonDateTimeConverter(),
                    new JsonIPAddressConverter()
                }
            };
        }

        public static string Serialize(object obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            return Json.Serialize(obj, obj.GetType(), Options);
        }

        public static void Serialize(Stream utf8json, object obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            Json.Serialize(utf8json, obj, obj.GetType(), Options);
        }

        public static string Serialize<T>(T obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            return Json.Serialize(obj, Options);
        }

        public static void Serialize<T>(Stream utf8json, T obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            Json.Serialize(utf8json, obj, Options);
        }

        public static Task<string> SerializeAsync(object obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            return SerializeAsync<object>(obj);
        }

        public static Task SerializeAsync(Stream utf8json, object obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            return SerializeAsync<object>(utf8json, obj);
        }

        public static async Task<string> SerializeAsync<T>(T obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));

            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            await Json.SerializeAsync(stream, obj, Options);

            stream.Position = 0;
            return await reader.ReadToEndAsync();
        }

        public static async Task SerializeAsync<T>(Stream utf8json, T obj) {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            await Json.SerializeAsync(utf8json, obj, Options);
        }

        public static object Deserialize(string input, Type type) {
            ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            return Json.Deserialize(input, type, Options);
        }

        public static object Deserialize(Stream utf8json, Type type) {
            ArgumentNullException.ThrowIfNull(utf8json, nameof(utf8json));
            return Json.Deserialize(utf8json, type, Options);
        }

        public static T Deserialize<T>(string input) {
            ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
            return Json.Deserialize<T>(input, Options);
        }

        public static T Deserialize<T>(Stream utf8json) {
            ArgumentNullException.ThrowIfNull(utf8json, nameof(utf8json));
            return Json.Deserialize<T>(utf8json, Options);
        }

        public static async Task<object> DeserializeAsync(string input, Type type) {
            ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);

            await writer.WriteAsync(input);
            await writer.FlushAsync();

            stream.Position = 0;
            return await Json.DeserializeAsync(stream, type, Options);
        }

        public static async Task<object> DeserializeAsync(Stream utf8json, Type type) {
            ArgumentNullException.ThrowIfNull(utf8json, nameof(utf8json));
            return await Json.DeserializeAsync(utf8json, type, Options);
        }

        public static async Task<T> DeserializeAsync<T>(string input) {
            ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);

            await writer.WriteAsync(input);
            await writer.FlushAsync();

            stream.Position = 0;
            return await Json.DeserializeAsync<T>(stream, Options);
        }

        public static async Task<T> DeserializeAsync<T>(Stream utf8json) {
            ArgumentNullException.ThrowIfNull(utf8json, nameof(utf8json));
            return await Json.DeserializeAsync<T>(utf8json, Options);
        }
    }
}
