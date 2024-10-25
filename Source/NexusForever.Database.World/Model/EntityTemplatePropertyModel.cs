using NexusForever.Game.Static.Entity;

namespace NexusForever.Database.World.Model
{
    public class EntityTemplatePropertyModel
    {
        public uint Id { get; set; }
        public Property Property { get; set; }
        public float Value { get; set; }

        public EntityTemplateModel Entity { get; set; }
    }
}
