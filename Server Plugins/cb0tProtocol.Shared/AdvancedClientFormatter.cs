using cb0tProtocol.Packets;
using cb0tProtocol.Packets.ib0t;
using System;
using System.Text;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol
{
    public class AdvancedClientFormatter : ClientFormatter
    {
        public override IPacket Unformat(byte id, string data, bool ib0t = false)
        {
            if (ib0t) {
                string[] values = data.Split(new[] { ':' }, 2);
                return UnformatBasic(values[0], values[1]);
            }

            switch ((AdvancedId)id) {
                case AdvancedId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL: {
                    var advanced = Json.Deserialize<AdvancedJson>(data);
                    if (advanced != null) {
                        int idx = advanced.Payload.ExtractId();
                        if (idx > -1) {
                            return new Advanced(Unformat((byte)idx, advanced.Payload, ib0t));
                        }
                    }
                    return null;
                }
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_ADD_TAGS:
                    return Json.Deserialize<ClientAddTags>(data);
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_REM_TAGS:
                    return Json.Deserialize<ClientRemTags>(data);
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_FONT:
                    return Json.Deserialize<ClientFont>(data);
                case AdvancedId.MSG_CHAT_CLIENT_VC_SUPPORTED:
                    return Json.Deserialize<ClientVoiceSupport>(data);
                case AdvancedId.MSG_CHAT_CLIENT_VC_FIRST:
                    return Json.Deserialize<ClientVoiceFirst>(data);
                case AdvancedId.MSG_CHAT_CLIENT_VC_FIRST_TO:
                    return Json.Deserialize<ClientVoiceFirstTo>(data);
                case AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK:
                    return Json.Deserialize<ClientVoiceChunk>(data);
                case AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK_TO:
                    return Json.Deserialize<ClientVoiceChunkTo>(data);
                case AdvancedId.MSG_CHAT_CLIENT_VC_IGNORE:
                    return Json.Deserialize<ClientVoiceIgnore>(data);
                case AdvancedId.MSG_CHAT_CLIENT_SUPPORTS_CUSTOM_EMOTES:
                    return Json.Deserialize<ClientEmoteSupport>(data);
                case AdvancedId.MSG_CHAT_SERVER_CUSTOM_EMOTES_ITEM:
                    return Json.Deserialize<ClientEmoteItem>(data);
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE:
                    return Json.Deserialize<ClientEmoteDelete>(data);
                case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST:
                    return Json.Deserialize<ClientScribbleFirst>(data);
                case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK:
                    return Json.Deserialize<ClientScribbleChunk>(data);
                default: return base.Unformat(id, data, ib0t);
            }
        }

        public override IPacket Unformat(byte id, byte[] data, int index, int count)
        {
            switch ((AdvancedId)id) {
                case AdvancedId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL: {
                    ushort len = BitConverter.ToUInt16(data, index);
                    if (Unformat(data[index + 2], data,  index + 3, len) is AdvancedPacket packet)
                        return new Advanced(packet);
                    else {
                        byte[] body = new byte[count - 3];
                        Array.Copy(data, 3, body, 0, body.Length);
                        return new Advanced(new Unknown(data[3], body));
                    }
                }
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_ADD_TAGS:
                    return Serializer.Deserialize<ClientAddTags>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_REM_TAGS:
                    return Serializer.Deserialize<ClientRemTags>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_FONT:
                    return Serializer.Deserialize<ClientFont>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_VC_SUPPORTED:
                    return Serializer.Deserialize<ClientVoiceSupport>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_VC_FIRST:
                    return Serializer.Deserialize<ClientVoiceFirst>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_VC_FIRST_TO:
                    return Serializer.Deserialize<ClientVoiceFirstTo>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK:
                    return Serializer.Deserialize<ClientVoiceChunk>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK_TO:
                    return Serializer.Deserialize<ClientVoiceChunkTo>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_VC_IGNORE:
                    return Serializer.Deserialize<ClientVoiceIgnore>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_SUPPORTS_CUSTOM_EMOTES:
                    return Serializer.Deserialize<ClientEmoteSupport>(data, index, count);
                case AdvancedId.MSG_CHAT_SERVER_CUSTOM_EMOTES_ITEM:
                    return Serializer.Deserialize<ClientEmoteItem>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE:
                    return Serializer.Deserialize<ClientEmoteDelete>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST:
                    return Serializer.Deserialize<ClientScribbleFirst>(data, index, count);
                case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK:
                    return Serializer.Deserialize<ClientScribbleChunk>(data, index, count);
            }

            return base.Unformat(id, data, index, count);
        }

        protected override string FormatBasic(IPacket packet)
        {
            if (packet is ClientCustom custom) {
                if (custom.CustomId == "ib0t_lag") {
                    string data = Encoding.UTF8.GetString(custom.Data);
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

        protected override IPacket UnformatBasic(string message, string content)
        {
            if (message == "LAG")
                return new ClientCustom(null, "ib0t_lag", Encoding.UTF8.GetBytes(content));

            return base.UnformatBasic(message, content);
        }
    }
}
