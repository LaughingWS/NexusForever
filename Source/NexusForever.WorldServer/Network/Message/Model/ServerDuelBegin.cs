﻿using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerDuelBegin)]
    public class ServerDuelBegin : IWritable
    {
        public uint ChallengerId { get; set; }
        public uint OpponentId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(ChallengerId);
            writer.Write(OpponentId);
        }
    }
}
