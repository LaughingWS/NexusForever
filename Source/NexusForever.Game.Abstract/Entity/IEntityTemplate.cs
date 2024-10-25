using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IEntityTemplate
    {
        uint CreatureId { get; set; }
        EntityType Type { get; set; }
        uint DisplayInfoId { get; set; }
        ushort OutfitInfoId { get; set; }
        Faction Faction1 { get; set; }
        Faction Faction2 { get; set; }

        List<IEntityTemplateProperty> Properties { get; set; }
        List<IEntityTemplateStat> Stats { get; set; }
    }
}
