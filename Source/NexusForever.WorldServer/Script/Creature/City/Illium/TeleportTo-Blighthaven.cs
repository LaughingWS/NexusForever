using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.City.Illium
{
    [Script(70382)]
    public class TeleportTo_Nursery_Trading_Post : CreatureScript
    {
        const uint WLOC_Nursery_Trading_Post = 49902;

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            WorldLocation2Entry entry = GameTableManager.Instance.WorldLocation2.GetEntry(WLOC_Nursery_Trading_Post);
            if (entry == null)
                return;

            if (!player.CanTeleport())
                return;

            var rotation = new Quaternion(entry.Facing0, entry.Facing1, entry.Facing2, entry.Facing3);
            player.Rotation = rotation.ToEulerDegrees();
            player.TeleportTo((ushort)entry.WorldId, entry.Position0, entry.Position1, entry.Position2);
        }
    }
}
