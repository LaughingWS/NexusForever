using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Event;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.EvilFromTheEther.Script
{
    [ScriptFilterCreatureId(71037)]
    public class KatjaZarkhovEntityScript : INonPlayerScript, IOwnedScript<INonPlayerEntity>
    {
        private INonPlayerEntity entity;

        #region Dependency Injection

        private readonly IScriptEventFactory eventFactory;
        private readonly IScriptEventManager eventManager;
        private readonly IEntityTemplateManager entityTemplateManager;

        public KatjaZarkhovEntityScript(
            IScriptEventFactory eventFactory,
            IScriptEventManager eventManager,
            IEntityTemplateManager entityTemplateManager)
        {
            this.eventFactory          = eventFactory;
            this.eventManager          = eventManager;
            this.entityTemplateManager = entityTemplateManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(INonPlayerEntity owner)
        {
            entity = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            eventManager.Update(lastTick);
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public void OnDeath()
        {
            IEntityTemplate template = entityTemplateManager.GetEntityTemplate(71821);

            var @event = eventFactory.CreateEvent<IEntitySummonEvent<INonPlayerEntity>>();
            @event.Initialise(entity.SummonFactory, template, entity.Position, entity.Rotation);
            eventManager.EnqueueEvent(TimeSpan.FromSeconds(0), @event);
        }
    }
}
