using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(28454)]
    public class TeleportTo_EXOLab_71 : CreatureScript
{       
        const uint WLOC_ExoLab_in   = 21760;
        const uint WLOC_ExoLab_2out = 21761;
        
        public override void OnCreate(WorldEntity me)
        {
            me.RangeCheck = 2f;
        }

        public override void OnEnterRange(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (!player.CanTeleport())
                return;

            if (me.Position.Y - 1f > activator.Position.Y)
                return;

            WorldLocation2Entry entry = GameTableManager.Instance.WorldLocation2.GetEntry(WLOC_ExoLab_in);
            if (entry == null)
                return;

                var rotation = new Quaternion(entry.Facing0, entry.Facing1, entry.Facing2, entry.Facing3);
                player.Rotation = rotation.ToEulerDegrees();
                player.TeleportTo((ushort)entry.WorldId, entry.Position0, entry.Position1, entry.Position2);
        }
    }
}