using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IEntityTemplateProperty
    {
        Property Property { get; set; }
        float Value { get; set; }
    }
}
