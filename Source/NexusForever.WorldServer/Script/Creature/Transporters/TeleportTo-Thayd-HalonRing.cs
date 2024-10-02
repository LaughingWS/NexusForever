using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(46185)]
    public class TeleportTo_Thayd_HalonRing : CreatureScript
    {
        const uint WLOC_Thayd_HalonRing = 37772;

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            WorldLocation2Entry entry = GameTableManager.Instance.WorldLocation2.GetEntry(WLOC_Thayd_HalonRing);
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
