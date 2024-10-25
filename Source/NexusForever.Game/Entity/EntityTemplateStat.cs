using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Entity
{
    public class EntityTemplateStat : IEntityTemplateStat
    {
        public Stat Stat { get; set; }
        public float Value { get; set; }
    }
}
