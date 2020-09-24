using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zorbo.Core.Converters;

namespace Zorbo.Core.Models
{
    public static class Persistence
    {
        public static T LoadModel<T>(string filename) where T : ModelBase, new()
        {
            if (!File.Exists(filename))
                return new T();

            T ret = null;

            using (var sr = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
                ret = Json.Deserialize<T>(sr.ReadToEnd());

            return ret;
        }

        public static async Task<T> LoadModelAsync<T>(string filename) where T : ModelBase, new()
        {
            if (!File.Exists(filename))
                return new T();

            T ret = null;

            using (var sr = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
                ret = Json.Deserialize<T>(await sr.ReadToEndAsync());

            return ret;
        }

        public static void SaveModel(this object model, string filename)
        {
            string content = Json.Serialize(model, Formatting.Indented);
            using var sw = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write));

            sw.Write(content);
            sw.Flush();
        }

        public static async Task SaveModelAsync(this object model, string filename)
        {
            string content = Json.Serialize(model, Formatting.Indented);
            using var sw = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write));

            await sw.WriteAsync(content);
            await sw.FlushAsync();
        }
    }
}
