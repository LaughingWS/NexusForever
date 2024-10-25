using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Entity
{
    public class EntityTemplate : IEntityTemplate
    {
        public uint CreatureId { get; set; }
        public EntityType Type { get; set; }
        public uint DisplayInfoId { get; set; }
        public ushort OutfitInfoId { get; set; }
        public Faction Faction1 { get; set; }
        public Faction Faction2 { get; set; }

        public List<IEntityTemplateProperty> Properties { get; set; } = [];
        public List<IEntityTemplateStat> Stats { get; set; } = [];
    }
}
