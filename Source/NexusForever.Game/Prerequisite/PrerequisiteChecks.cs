using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using Path = NexusForever.Game.Static.Entity.Path;

namespace NexusForever.Game.Prerequisite
{
    public sealed partial class PrerequisiteManager
    {


        [PrerequisiteCheck(PrerequisiteType.PurchasedTitle)]
        private static bool PrerequisiteCheckPurchasedTitle(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return player.TitleManager.HasTitle((ushort)objectId);
                case PrerequisiteComparison.NotEqual:
                    return !player.TitleManager.HasTitle((ushort)objectId);
                default:
                    log.Warn($"Unhandled PrerequisiteComparison {comparison} for {(PrerequisiteType)288}!");
                    return true;
            }
        }
    }
}
