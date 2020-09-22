using cb0tProtocol;
using cb0tProtocol.Packets;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.WebSockets;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;

namespace Zorbo.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Attempting to export packets...");

            Export<AresId, AresPacket>("arespackets");
            Export<AdvancedId, AdvancedPacket>("cb0tpackets");

            Console.Write("Packet export completed successfully. Press any key to exit. ");
            Console.ReadKey();
        }

        private static void Export<TId, TPacket>(string name)
            where TId : Enum
            where TPacket : IPacket
        {
            Assembly asm = Assembly.GetAssembly(typeof(TPacket));

            ExportRequiredMembers<TId, TPacket>(name, asm);
            ExportAllMembers<TId, TPacket>(name, asm);
        }

        /// <summary>
        /// Exported only members required for each packet. 
        /// Ultimately what the majority of packets will look like.
        /// </summary>
        private static void ExportRequiredMembers<TId, TPacket>(string name, Assembly asm) 
            where TId : Enum 
            where TPacket : IPacket
        {
            Type apacket = typeof(TPacket);

            using var fs = File.Create($"{name}-required.json");
            using var sw = new StreamWriter(fs, Encoding.UTF8);

            foreach (var type in asm.GetTypes()) {
                if (type.IsClass && !type.IsAbstract && apacket.IsAssignableFrom(type)) {
                    try {
                        var instance = (IPacket)Activator.CreateInstance(type);

                        if (instance is Unknown || instance is PingPongPacket)
                            continue;

                        Console.WriteLine("Type \"{0}\" created, serializing...", instance.GetType());
                        string json = Json.Serialize(instance, Formatting.Indented);

                        sw.Write("\"{0}\": ", Enum.ToObject(typeof(TId), instance.Id));
                        sw.WriteLine(json);
                        sw.WriteLine();
                    }
                    catch (Exception ex) {
                        Console.WriteLine();
                        Console.WriteLine("Unable to create an instance of type \"{0}\"");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine();
                    }
                }
            }

            sw.Flush();
        }
        /// <summary>
        /// Exports all the members that the application knows about and can use.
        /// </summary>
        private static void ExportAllMembers<TId, TPacket>(string name, Assembly asm)
            where TId : Enum
            where TPacket : IPacket
        {
            Type apacket = typeof(TPacket);

            using var fs = File.Create($"{name}-allmembers.json");
            using var sw = new StreamWriter(fs, Encoding.UTF8);

            foreach (var type in asm.GetTypes()) {
                if (type.IsClass && !type.IsAbstract && apacket.IsAssignableFrom(type)) {
                    try {
                        var instance = (IPacket)Activator.CreateInstance(type);

                        if (instance is Unknown || instance is PingPongPacket)
                            continue;

                        Console.WriteLine("Type \"{0}\" created, serializing...", instance.GetType());

                        sw.WriteLine("\"{0}\": {{", Enum.ToObject(typeof(TId), instance.Id));

                        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                        for(int i = 0; i < props.Count; i++) {
                            var property = props[i];
                            var attributes = property.GetCustomAttributes(true);

                            if (attributes.Contains(s => s is JsonIgnoreAttribute))
                                continue;

                            if (property.Name == "Id")
                                sw.Write("  \"id\": {0}", (byte)instance.Id);
                            else {

                                var jsonprop = (JsonPropertyAttribute)attributes.Find(s => s is JsonPropertyAttribute);
                                if (jsonprop == null) continue;

                                string pname = jsonprop.PropertyName;
                                string value;

                                if (property.PropertyType == typeof(bool)) {
                                    bool val = (bool)property.GetValue(instance);
                                    sw.Write("  \"{0}\": {1}", pname, val.ToString().ToLower());
                                }
                                else if (property.PropertyType == typeof(string)) {
                                    string val = (string)property.GetValue(instance);
                                    if (string.IsNullOrEmpty(val))
                                        sw.Write("  \"{0}\": null", pname);
                                    else
                                        sw.Write("  \"{0}\": \"{1}\"", pname, val);
                                }
                                else if (property.PropertyType == typeof(Guid)) {
                                    Guid val = (Guid)property.GetValue(instance);
                                    sw.Write("  \"{0}\": \"{1}\"", pname, val);
                                }
                                else if (property.PropertyType.IsEnum) {
                                    int val = Convert.ToInt32(property.GetValue(instance));
                                    sw.Write("  \"{0}\": {1}", pname, val);
                                }
                                else {
                                    value = property.GetValue(instance)?.ToString() ?? "null";
                                    sw.Write("  \"{0}\": {1}", pname, value);
                                }
                            }
                            if (i < props.Count - 1) sw.Write(',');

                            sw.WriteLine();
                        }

                        sw.WriteLine("}");
                        sw.WriteLine();
                    }
                    catch (Exception ex) {
                        Console.WriteLine();
                        Console.WriteLine("Unable to create an instance of type \"{0}\"", type);
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine();
                    }
                }
            }

            sw.Flush();
        }
    }
}
