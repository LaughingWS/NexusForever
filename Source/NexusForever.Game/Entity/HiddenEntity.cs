﻿using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;

namespace NexusForever.Game.Entity
{
    public class HiddenEntity : WorldEntity, IHiddenEntity
    {
        public override EntityType Type => EntityType.Hidden;

        #region Dependency Injection

        public HiddenEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory)
            : base(movementManager, entitySummonFactory)
        {
        }

        #endregion

        protected override IEntityModel BuildEntityModel()
        {
            return new HiddenEntityModel
            {
                CreatureId = CreatureId
            };
        }
    }
}
