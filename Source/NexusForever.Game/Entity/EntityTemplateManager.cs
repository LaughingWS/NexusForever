using System.Collections.Immutable;
using NexusForever.Database;
using NexusForever.Database.World;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Shared;

namespace NexusForever.Game.Entity
{
    public class EntityTemplateManager : IEntityTemplateManager
    {
        private ImmutableDictionary<uint, IEntityTemplate> entityTemplates;

        #region Dependency Injection

        private readonly IDatabaseManager databaseManager;
        private readonly IFactory<IEntityTemplate> entityTemplateFactory;
        private readonly IFactory<IEntityTemplateProperty> entityTemplatePropertyFactory;
        private readonly IFactory<IEntityTemplateStat> entityTemplateStatFactory;

        public EntityTemplateManager(
            IDatabaseManager databaseManager,
            IFactory<IEntityTemplate> entityTemplateFactory,
            IFactory<IEntityTemplateProperty> entityTemplatePropertyFactory,
            IFactory<IEntityTemplateStat> entityTemplateStatFactory)
        {
            this.databaseManager               = databaseManager;
            this.entityTemplateFactory         = entityTemplateFactory;
            this.entityTemplatePropertyFactory = entityTemplatePropertyFactory;
            this.entityTemplateStatFactory     = entityTemplateStatFactory;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IEntityTemplate"/>'s from the database.
        /// </summary>
        public void Initialise()
        {
            var builder = ImmutableDictionary.CreateBuilder<uint, IEntityTemplate>();
            foreach (EntityTemplateModel model in databaseManager.GetDatabase<WorldDatabase>().GetEntityTemplates())
            {
                IEntityTemplate template = entityTemplateFactory.Resolve();
                template.CreatureId    = model.Id;
                template.Type          = model.Type;
                template.DisplayInfoId = model.DisplayInfo;
                template.OutfitInfoId  = model.OutfitInfo;
                template.Faction1      = model.Faction1;
                template.Faction2      = model.Faction2;

                foreach (EntityTemplatePropertyModel propertyModel in model.EntityProperty)
                {
                    IEntityTemplateProperty templateProperty = entityTemplatePropertyFactory.Resolve();
                    templateProperty.Property = propertyModel.Property;
                    templateProperty.Value    = propertyModel.Value;
                    template.Properties.Add(templateProperty);
                }

                foreach (EntityTemplateStatModel statModel in model.EntityStat)
                {
                    IEntityTemplateStat templateStat = entityTemplateStatFactory.Resolve();
                    templateStat.Stat  = statModel.Stat;
                    templateStat.Value = statModel.Value;
                    template.Stats.Add(templateStat);
                }

                builder.Add(template.CreatureId, template);
            }

            entityTemplates = builder.ToImmutable();
        }

        /// <summary>
        /// Return <see cref="IEntityTemplate"/> with the supplied id.
        /// </summary>
        public IEntityTemplate GetEntityTemplate(uint id)
        {
            return entityTemplates.TryGetValue(id, out IEntityTemplate template) ? template : null;
        }
    }
}
