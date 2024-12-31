using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Event;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Event;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.ColdbloodCitadel
{
    [ScriptFilterOwnerId(907)]
    public class ColdbloodCitadelEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint gatherRingGuid;
        private uint gatherRingTriggerGuid;

        private uint IceDoorGuid;

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;
        private readonly IEntitySummonFactory entitySummonFactory;
        private readonly IEntityTemplateManager entityTemplateManager;

        public ColdbloodCitadelEventScript(
            IGlobalQuestManager globalQuestManager,
            IEntitySummonFactory entitySummonFactory,
            IEntityTemplateManager entityTemplateManager)
        {
            this.globalQuestManager = globalQuestManager;
            this.entitySummonFactory = entitySummonFactory;
            this.entityTemplateManager = entityTemplateManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            mapInstance = publicEvent.Map as IMapInstance;

            publicEvent.SetPhase(PublicEventPhase.Enter);
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is added to the map the public event is on.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnAddToMapWorldEntity(worldEntity);
                    break;
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnAddToMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }

        private void OnAddToMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.GatherRing:
                    gatherRingGuid = worldEntity.Guid;
                    break;               
            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 75624:
                    gatherRingTriggerGuid = worldLocationEntity.Guid;
                    break;
            }
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is removed from the map the public event is on.
        /// </summary>
        public void OnRemoveFromMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnRemoveFromMapWorldEntity(worldEntity);
                    break;
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnRemoveFromMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }

        private void OnRemoveFromMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.GatherRing:
                    gatherRingGuid = 0;
                    break;
            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 75624:
                    gatherRingTriggerGuid = 0;
                    break;
            }
        }

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.Enter:
                    OnPhaseEnter();
                    break;
                case PublicEventPhase.HailStoneGatecrasher:
                    OnPhaseHailStoneGatecrasher();
                    break;
                case PublicEventPhase.IceBloodCover:
                    OnPhaseIceBloodCover();
                    break;
                case PublicEventPhase.RiserHarizog:
                    OnPhaseRiserHarizog();
                    break;
            }
        }

        private void OnPhaseEnter()
        {
            //TODO: get it to get player count
            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(53206, 8656);
            triggerEntity.AddToMap(mapInstance, new Vector3(604.33f, -475.452f, -322.957f));
        }

        private void OnPhaseHailStoneGatecrasher()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatHailStoneGatecrasher);

            var triggerEntity = mapInstance.GetEntity<IWorldLocationVolumeGridTriggerEntity>(gatherRingTriggerGuid);
            triggerEntity?.RemoveFromMap();

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherRingGuid);
            gatherRing?.RemoveFromMap();

            publicEvent.ActivateObjective(PublicEventObjective.SavePellFightingTheOsun);

            Random rnd1 = new();
            uint[] Object1 = [0, 1]; //TODO: make a min and max number RNG and not this
            int Index1 = rnd1.Next(Object1.Length);
            if (Index1 == 1) publicEvent.ActivateObjective(PublicEventObjective.StealSampleOfLiquidSoulfrost);

            Random rnd2 = new();
            uint[] Object2 = [0, 1];
            int Index2 = rnd2.Next(Object1.Length);
            if (Index2 == 1) publicEvent.ActivateObjective(PublicEventObjective.GatherSoulfrostShards);
            //TODO finish adding all the objectives

            // this looks like it as a short delay, I need to deal with this later
            BroadcastCommunicatorMessage(CommunicatorMessage.TowerEngineerRenhakul1);
        }

        private void OnPhaseIceBloodCover()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheIcebloodCoven);
            publicEvent.ActivateObjective(PublicEventObjective.RescueThePellArchitect);
            publicEvent.ActivateObjective(PublicEventObjective.KillKrovakSummonersAndTheirFrostguards);
            publicEvent.ActivateObjective(PublicEventObjective.ConcurrentCovenCollapse);

            BroadcastCommunicatorMessage(CommunicatorMessage.TowerEngineerRenhakul11);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(IceDoorGuid);
            door?.OpenDoor();

            //TODO: Add random objects
        }

        private void OnPhaseRiserHarizog()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheRisenHarizog);

            BroadcastCommunicatorMessage(CommunicatorMessage.HarizonColdblood2);// the message changes based on the last boss killed, So it need to check first
            BroadcastCommunicatorMessage(CommunicatorMessage.TowerEngineerRenhakul4);
        }

        private void BroadcastCommunicatorMessage(CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage?.Send(player.Session);
        }

        private void SendCommunicatorMessage(IPlayer player, CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            communicatorMessage?.Send(player.Session);
        }

        /// <summary>
        /// Invoked when the <see cref="IPublicEventObjective"/> status changes.
        /// </summary>
        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.FindThePellAttackingColdbloodCitadel:
                    publicEvent.SetPhase(PublicEventPhase.HailStoneGatecrasher);
                    break;
                case PublicEventObjective.DefeatHailStoneGatecrasher:
                    publicEvent.SetPhase(PublicEventPhase.IceBloodCover);
                    break;
                case PublicEventObjective.DefeatTheIcebloodCoven:
                    publicEvent.SetPhase(PublicEventPhase.RiserHarizog);
                    break;
                case PublicEventObjective.DefeatTheRisenHarizog:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
