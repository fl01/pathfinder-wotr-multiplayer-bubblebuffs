using ProtoBuf;
using WOTRMultiplayer.Logging.Attributes;
using WOTRMultiplayer.Networking.Messages;

namespace BubbleBuffs.Multiplayer.Networking.Messages
{
    /// <summary>
    /// You can use any MessageTypeId, but it must be unique across all registered mods.
    /// Ideally, it should be added to the multiplayer mod before release, as that serves as the primary source of truth.
    /// </summary>
    [ProtoContract]
    [BeetleX.Packets.MessageType((int)MessageTypes.Mods.Bubble.NotifyBubbleBuffsUsed)]
    public class NotifyBubbleBuffsUsed
    {
        [ProtoMember(1)]
        [LogMe]
        public BuffGroup Group { get; set; }

        [ProtoMember(2)]
        public string RawBufferState { get; set; }
    }
}
