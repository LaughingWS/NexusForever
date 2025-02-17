using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;

namespace NexusForever.Game.Entity
{
    public class DoorEntity : WorldEntity, IDoorEntity
    {
        public override EntityType Type => EntityType.Door;

        public bool IsOpen => GetStatEnum<StandState>(Stat.StandState) == StandState.State1;

        #region Dependency Injection

        private readonly IScriptManager scriptManager;

        public DoorEntity(
            IScriptManager scriptManager,
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory)
            : base(movementManager, entitySummonFactory)
        {
            this.scriptManager = scriptManager;
        }

        #endregion

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);

            SetStat(Stat.StandState, StandState.State0); // Closed on spawn
            SetBaseProperty(Property.BaseHealth, 101f); // Sniffs showed all doors had 101hp for me.
        }

        /// <summary>
        /// Initialise <see cref="IScriptCollection"/> for <see cref="IDoorEntity"/>.
        /// </summary>
        protected override void InitialiseScriptCollection(List<string> names)
        {
            scriptCollection = scriptManager.InitialiseOwnedCollection<IDoorEntity>(this);
            scriptManager.InitialiseEntityScripts<IDoorEntity>(scriptCollection, this, names);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new DoorEntityModel
            {
                CreatureId = CreatureId
            };
        }

        /// <summary>
        /// Used to open this <see cref="IDoorEntity"/>.
        /// </summary>
        public void OpenDoor()
        {
            SetStat(Stat.StandState, StandState.State1);
            EnqueueToVisible(new ServerEmote
            {
                Guid       = Guid,
                StandState = StandState.State1
            });

            scriptCollection.Invoke<IDoorEntityScript>(script => script.OnOpenDoor());
        }

        /// <summary>
        /// Used to close this <see cref="IDoorEntity"/>.
        /// </summary>
        public void CloseDoor()
        {
            SetStat(Stat.StandState, StandState.State0);
            EnqueueToVisible(new ServerEmote
            {
                Guid       = Guid,
                StandState = StandState.State0
            });

            scriptCollection.Invoke<IDoorEntityScript>(script => script.OnDoorClose());
        }
    }
}
