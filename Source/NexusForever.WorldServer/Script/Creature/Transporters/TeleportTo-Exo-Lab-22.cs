using NexusForever.Shared.GameTable;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Reputation.Static;
using System.Collections.Generic;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(25886)]
    public class TeleportTo_EXOLab_22 : CreatureScript
{
        const ushort WLOC_EXO_Lab_22 = 870;
        private Vector3 LOC_EXO_Lab_22 = new Vector3(-7079.58f, -1045.62f, -1037.51f);
        private Vector3 ROT_EXO_Lab_22 = new Vector3(0f, 0f, 0f);

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

            player.Rotation = ROT_EXO_Lab_22;
            player.TeleportTo(WLOC_EXO_Lab_22, LOC_EXO_Lab_22.X, LOC_EXO_Lab_22.Y, LOC_EXO_Lab_22.Z);

        }
    }
}
