using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IEntityTemplateStat
    {
        Stat Stat { get; set; }
        float Value { get; set; }
    }
}
