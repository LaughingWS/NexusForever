﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Configuration.Model;
using NexusForever.Database.World.Model;
using NLog;

namespace NexusForever.Database.World
{
    [Database(DatabaseType.World)]
    public class WorldDatabase : IDatabase
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private IConnectionString config;

        public void Initialise(IConnectionString connectionString)
        {
            config = connectionString;
        }

        public void Migrate()
        {
            using var context = new WorldContext(config);

            List<string> migrations = context.Database.GetPendingMigrations().ToList();
            if (migrations.Count > 0)
            {
                log.Info($"Applying {migrations.Count} authentication database migration(s)...");
                foreach (string migration in migrations)
                    log.Info(migration);

                context.Database.Migrate();
            }
        }

        private IQueryable<EntityModel> EntitiesInclude(IQueryable<EntityModel> entities)
        {
            return entities
                .Include(e => e.EntityEvent)
                .Include(e => e.EntitySpline)
                .Include(e => e.EntityVendor)
                .Include(e => e.EntityVendorCategory)
                .Include(e => e.EntityVendorItem)
                .Include(e => e.EntityStat)
                .Include(e => e.EntityScript);
        }

        public ImmutableList<EntityModel> GetEntities(ushort world)
        {
            using var context = new WorldContext(config);
            return EntitiesInclude(context.Entity.Where(e => e.World == world && e.EntityEvent == null))
                .AsSplitQuery()
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<EntityModel> GetEntitiesPublicEvent(uint publicEventId)
        {
            using var context = new WorldContext(config);
            return EntitiesInclude(context.Entity.Where(e => e.EntityEvent != null && e.EntityEvent.EventId == publicEventId))
                .AsSplitQuery()
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<EntityModel> GetEntitiesWithSpline()
        {
            using var context = new WorldContext(config);
            return context.Entity.Where(e => e.EntitySpline != null)
                .Include(e => e.EntitySpline)
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<EntityModel> GetEntitiesWithoutArea()
        {
            using var context = new WorldContext(config);
            return context.Entity.Where(e => e.Area == 0)
                .AsNoTracking()
                .ToImmutableList();
        }

        public void UpdateEntities(IEnumerable<EntityModel> models)
        {
            using var context = new WorldContext(config);
            foreach (EntityModel model in models)
            {
                EntityEntry<EntityModel> entity = context.Attach(model);
                entity.State = EntityState.Modified;
            }

            context.SaveChanges();
        }

        public ImmutableList<TutorialModel> GetTutorialTriggers()
        {
            using var context = new WorldContext(config);
            return context.Tutorial.ToImmutableList();
        }

        public ImmutableList<DisableModel> GetDisables()
        {
            using var context = new WorldContext(config);
            return context.Disable.ToImmutableList();
        }

        public ImmutableList<StoreCategoryModel> GetStoreCategories()
        {
            using var context = new WorldContext(config);
            return context.StoreCategory
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<StoreOfferGroupModel> GetStoreOfferGroups()
        {
            using var context = new WorldContext(config);
            return context.StoreOfferGroup
                .Include(e => e.StoreOfferGroupCategory)
                .Include(e => e.StoreOfferItem)
                    .ThenInclude(e => e.StoreOfferItemData)
                .Include(e => e.StoreOfferItem)
                    .ThenInclude(e => e.StoreOfferItemPrice)
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<MapEntranceModel> GetMapEntrances()
        {
            using var context = new WorldContext(config);
            return context.MapEntrance
                .AsNoTracking()
                .ToImmutableList();
        }
    }
}
