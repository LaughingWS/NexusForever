using NexusForever.Game.Static.Entity.Movement.Command;

namespace NexusForever.Network.World.Entity.Command
{
    [EntityCommand(EntityCommand.SetMoveDefaults)]
    public class SetMoveDefaultsCommand : IEntityCommandModel
    {
        public bool Blend { get; set; }

        public void Read(GamePacketReader reader)
        {
            Blend = reader.ReadBit();
        }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Blend);
        }
    }
}
