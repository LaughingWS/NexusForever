using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using System.Numerics;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    [ScriptFilterCreatureId(73735)]

    public class CombatHologramEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        public CombatHologramEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator, IGridEntity entity)
        {
            Vector3 position = activator.Faction2 switch
            {
                Faction.Exile => new Vector3(-172.937f, -879.5415f, 263.561f),
                _ => throw new NotImplementedException()
            };

            activator.TeleportToLocal(position);

            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<INoviceTutorialCombatProjector>());
        }
    }

}
