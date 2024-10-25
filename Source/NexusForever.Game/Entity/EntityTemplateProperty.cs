using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Entity
{
    public class EntityTemplateProperty : IEntityTemplateProperty
    {
        public Property Property { get; set; }
        public float Value { get; set; }
    }
}
