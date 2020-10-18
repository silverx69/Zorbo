using System;
using System.Text;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.Chatroom.ib0t;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Formatters
{
    public class AdvancedClientFormatter : 
        AdvancedClientFormatter<PacketSerializer> { }

    public class AdvancedClientFormatter<TSerializer> : 
        ClientFormatter<TSerializer> 
        where TSerializer : PacketSerializer, new()
    {
        public override IPacket Unformat(byte id, string data, bool ib0t = false)
        {
            if (ib0t) {
                string[] values = data.Split(new[] { ':' }, 2);
                return UnformatBasic(values[0], values[1]);
            }
            else if (id == (byte)AresId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL) {
                var advanced = Json.Deserialize<AdvancedJson>(data);
                if (advanced != null) {
                    int idx = advanced.Payload.ExtractId();
                    if (idx > -1) {
                        return new Advanced(Unformat((byte)idx, advanced.Payload, ib0t));
                    }
                }
                return null;
            }
            else {
                return ((AdvancedId)id) switch
                {
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_ADD_TAGS => Json.Deserialize<ClientAddTags>(data),
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_REM_TAGS => Json.Deserialize<ClientRemTags>(data),
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_FONT => Json.Deserialize<ClientFont>(data),
                    AdvancedId.MSG_CHAT_CLIENT_VC_SUPPORTED => Json.Deserialize<ClientVoiceSupport>(data),
                    AdvancedId.MSG_CHAT_CLIENT_VC_FIRST => Json.Deserialize<ClientVoiceFirst>(data),
                    AdvancedId.MSG_CHAT_CLIENT_VC_FIRST_TO => Json.Deserialize<ClientVoiceFirstTo>(data),
                    AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK => Json.Deserialize<ClientVoiceChunk>(data),
                    AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK_TO => Json.Deserialize<ClientVoiceChunkTo>(data),
                    AdvancedId.MSG_CHAT_CLIENT_VC_IGNORE => Json.Deserialize<ClientVoiceIgnore>(data),
                    AdvancedId.MSG_CHAT_CLIENT_SUPPORTS_CUSTOM_EMOTES => Json.Deserialize<ClientEmoteSupport>(data),
                    AdvancedId.MSG_CHAT_SERVER_CUSTOM_EMOTES_ITEM => Json.Deserialize<ClientEmoteItem>(data),
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE => Json.Deserialize<ClientEmoteDelete>(data),
                    AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST => Json.Deserialize<ClientScribbleFirst>(data),
                    AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK => Json.Deserialize<ClientScribbleChunk>(data),
                    _ => base.Unformat(id, data, ib0t),
                };
            }
        }

        public override IPacket Unformat(byte id, byte[] data, int index, int count)
        {
            if (id == (byte)AresId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL) {
                ushort len = BitConverter.ToUInt16(data, index);
                if (Unformat(data[index + 2], data, index + 3, len) is AdvancedPacket packet)
                    return new Advanced(packet);
                else {
                    byte[] body = new byte[count - 3];
                    Array.Copy(data, 3, body, 0, body.Length);
                    return new Advanced(new Unknown(data[3], body));
                }
            }
            else {
                return ((AdvancedId)id) switch
                {
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_ADD_TAGS => Serializer.Deserialize<ClientAddTags>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_REM_TAGS => Serializer.Deserialize<ClientRemTags>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_FONT => Serializer.Deserialize<ClientFont>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_VC_SUPPORTED => Serializer.Deserialize<ClientVoiceSupport>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_VC_FIRST => Serializer.Deserialize<ClientVoiceFirst>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_VC_FIRST_TO => Serializer.Deserialize<ClientVoiceFirstTo>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK => Serializer.Deserialize<ClientVoiceChunk>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK_TO => Serializer.Deserialize<ClientVoiceChunkTo>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_VC_IGNORE => Serializer.Deserialize<ClientVoiceIgnore>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_SUPPORTS_CUSTOM_EMOTES => Serializer.Deserialize<ClientEmoteSupport>(data, index, count),
                    AdvancedId.MSG_CHAT_SERVER_CUSTOM_EMOTES_ITEM => Serializer.Deserialize<ClientEmoteItem>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE => Serializer.Deserialize<ClientEmoteDelete>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST => Serializer.Deserialize<ClientScribbleFirst>(data, index, count),
                    AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK => Serializer.Deserialize<ClientScribbleChunk>(data, index, count),
                    _ => base.Unformat(id, data, index, count),
                };
            }
        }

        //To Client
        protected override string FormatBasic(IPacket packet)
        {
            if (packet is ClientCustom custom) {
                if (custom.CustomId == "zorbo_lag") {
                    string data = new DateTime(BitConverter.ToInt64(custom.Data))
                            .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                            .TotalMilliseconds
                            .ToString();
                    return string.Format("LAG:{0}:{1}", data.Length, data);
                }
            }
            else if (packet is ScribbleHead head) {
                return string.Format(
                    "SCRIBBLE_HEAD:{0},{1},{2}:{3}{4}{5}",
                    head.Name.Length,
                    head.BlockCount.ToString().Length,
                    head.Height.ToString().Length,
                    head.Name,
                    head.BlockCount,
                    head.Height);
            }
            else if (packet is ScribbleBlock block)
                return string.Format("SCRIBBLE_BLOCK:{0}:{1}", block.Block.Length, block.Block);

            return base.FormatBasic(packet);
        }

        //From Client
        protected override IPacket UnformatBasic(string message, string content)
        {
            string[] values;
            switch (message.ToUpper()) {
                case "LAG":
                    values = Parseib0tMessage(content);
                    long value;
                    if (long.TryParse(values[0], out value)) {
                        return new ClientCustom(
                            string.Empty,
                            "zorbo_lag",
                            BitConverter.GetBytes(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(value).Ticks));
                    }
                    break;
                case "SCRIBBLE_HEAD":
                    values = Parseib0tMessage(content);
                    return new ScribbleHead(values[0], int.Parse(values[1]), int.Parse(values[2]));
                case "SCRIBBLE_BLOCK":
                    values = Parseib0tMessage(content);
                    return new ScribbleBlock(values[0]);
            }

            return base.UnformatBasic(message, content);
        }
    }
}
