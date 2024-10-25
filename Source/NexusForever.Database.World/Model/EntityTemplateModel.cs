using System.Collections.Generic;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Database.World.Model
{
    public class EntityTemplateModel
    {
        public uint Id { get; set; }
        public EntityType Type { get; set; }
        public uint DisplayInfo { get; set; }
        public ushort OutfitInfo { get; set; }
        public Faction Faction1 { get; set; }
        public Faction Faction2 { get; set; }

        public ICollection<EntityTemplatePropertyModel> EntityProperty { get; set; } = [];
        public ICollection<EntityTemplateStatModel> EntityStat { get; set; } = [];
    }
}
