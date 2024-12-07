﻿using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Entity.Movement;
using NexusForever.Game.Entity.Trigger;
using NexusForever.Shared;

namespace NexusForever.Game.Entity
{
    public static class ServiceCollectionExtentions
    {
        public static void AddGameEntity(this IServiceCollection sc)
        {
            sc.AddGameEntityMovement();

            sc.AddTransient<IEntityFactory, EntityFactory>();
            sc.AddTransient<INonPlayerEntity, NonPlayerEntity>();
            sc.AddTransient<IChestEntity, ChestEntity>();
            sc.AddTransient<IDestructibleEntity, DestructibleEntity>();
            sc.AddTransient<IVehicleEntity, VehicleEntity>();
            sc.AddTransient<IDoorEntity, DoorEntity>();
            sc.AddTransient<IHarvestUnitEntity, HarvestUnitEntity>();
            sc.AddTransient<ICorpseUnitEntity, CorpseUnitEntity>();
            sc.AddTransient<IMountEntity, MountEntity>();
            sc.AddTransient<ICollectableUnitEntity, CollectableUnitEntity>();
            sc.AddTransient<ITaxiEntity, TaxiEntity>();
            sc.AddTransient<ISimpleEntity, SimpleEntity>();
            sc.AddTransient<IPlatformEntity, PlatformEntity>();
            sc.AddTransient<IMailboxEntity, MailboxEntity>();
            sc.AddTransient<IAiTurretEntity, AiTurretEntity>();
            sc.AddTransient<IInstancePortalEntity, InstancePortalEntity>();
            sc.AddTransient<IPlugEntity, PlugEntity>();
            sc.AddTransient<IResidenceEntity, ResidenceEntity>();
            sc.AddTransient<IStructuredPlugEntity, StructuredPlugEntity>();
            sc.AddTransient<IPinataLootEntity, PinataLootEntity>();
            sc.AddTransient<IBindPointEntity, BindPointEntity>();
            sc.AddTransient<IPlayer, Player>();
            sc.AddTransient<IHiddenEntity, HiddenEntity>();
            sc.AddTransient<ITriggerEntity, TriggerEntity>();
            sc.AddTransient<IGhostEntity, GhostEntity>();
            sc.AddTransient<IPetEntity, PetEntity>();
            sc.AddTransient<IEsperPetEntity, EsperPetEntity>();
            sc.AddTransient<IWorldUnitEntity, WorldUnitEntity>();
            sc.AddTransient<IScannerUnitEntity, ScannerUnitEntity>();
            sc.AddTransient<ICameraEntity, CameraEntity>();
            sc.AddTransient<ITrapEntity, TrapEntity>();
            sc.AddTransient<IDestructibleDoorEntity, DestructibleDoorEntity>();
            sc.AddTransient<IPickupEntity, PickupEntity>();
            sc.AddTransient<ISimpleCollidableEntity, SimpleCollidableEntity>();
            sc.AddTransient<IHousingMannequinEntity, HousingMannequinEntity>();
            sc.AddTransient<IHousingHarvestPlugEntity, HousingHarvestPlugEntity>();
            sc.AddTransient<IHousingPlantEntity, HousingPlantEntity>();
            sc.AddTransient<ILockboxEntity, LockboxEntity>();

            sc.AddTransient<IGridTriggerEntity, GridTriggerEntity>();
            sc.AddTransient<ITurnstileGridTriggerEntity, TurnstileTriggerEntity>();
            sc.AddTransient<IVolumeGridTriggerEntity, VolumeGridTriggerEntity>();
            sc.AddTransient<IWorldLocationVolumeGridTriggerEntity, WorldLocationVolumeGridTriggerEntity>();

            sc.AddSingletonLegacy<IBuybackManager, BuybackManager>();
            sc.AddSingletonLegacy<IEntityManager, EntityManager>();
            sc.AddSingletonLegacy<IPlayerManager, PlayerManager>();

            sc.AddTransient<IEntitySummonFactory, EntitySummonFactory>();
            sc.AddTransientFactory<IEntityTemplate, EntityTemplate>();
            sc.AddTransientFactory<IEntityTemplateProperty, EntityTemplateProperty>();
            sc.AddTransientFactory<IEntityTemplateStat, EntityTemplateStat>();
            sc.AddSingleton<IEntityTemplateManager, EntityTemplateManager>();

            sc.AddTransient<ICurrencyManager, CurrencyManager>();
            sc.AddTransient<IResurrectionManager, ResurrectionManager>();
        }
    }
}
