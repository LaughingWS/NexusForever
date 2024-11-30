using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Event;
using NexusForever.Game.Static.Spell;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Damage)]
    public class SpellEffectDamageHandler : ISpellEffectApplyHandler
    {
        #region Dependency Injection

        private readonly IFactory<IDamageCalculator> damageCalculatorFactory;

        public SpellEffectDamageHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory)
        {
            this.damageCalculatorFactory = damageCalculatorFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            if (!target.CanAttack(spell.Caster))
                return;

            IDamageCalculator damageCalculator = damageCalculatorFactory.Resolve();
            damageCalculator.CalculateDamage(spell.Caster, target, spell, info);

            if (info.Damage == null)
                return;

            target.TakeDamage(spell.Caster, info.Damage);

            if (spell.Caster is IPlayer player)
                player.Map.PublicEventManager.UpdateStat(player, PublicEventStat.Damage, info.Damage.AdjustedDamage);
        }
    }
}
