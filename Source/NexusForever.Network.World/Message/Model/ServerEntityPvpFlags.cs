using NexusForever.Game.Static.Entity;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerEntityPvpFlags)]
    public class ServerEntityPvpFlags : IWritable
    {
        public uint UnitId { get; set; }
        public PvPFlag PvpFlags { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnitId);
            writer.Write(PvpFlags, 3u);
        }
    }
}
