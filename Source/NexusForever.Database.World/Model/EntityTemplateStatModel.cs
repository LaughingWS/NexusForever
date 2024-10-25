using NexusForever.Game.Static.Entity;

namespace NexusForever.Database.World.Model
{
    public class EntityTemplateStatModel
    {
        public uint Id { get; set; }
        public Stat Stat { get; set; }
        public float Value { get; set; }

        public EntityTemplateModel Entity { get; set; }
    }
}
