﻿using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Combat;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NexusForever.WorldServer.Game.AI
{
    public class UnitAI : IUpdate
    {
        protected UnitEntity me { get; set; }

        private bool resettingFromCombat = false;

        public bool CanLeash() => this is not PetAI;
        public bool IsLeashing() => resettingFromCombat;

        protected float MAX_ATTACK_RANGE { get; set; } = 5f;
        protected List<uint> autoAttacks { get; set; } = new()
        {
            28704,
            28705
        };
        private int attackIndex;

        protected UpdateTimer autoTimer { get; set; } = new UpdateTimer(1.5d);
        protected UpdateTimer combatTimer { get; set; } = new UpdateTimer(6d, false);
        protected Spell4Entry specialAbility { get; set; } = GameTableManager.Instance.Spell4.GetEntry(55311);
        protected bool useSpecialAbility { get; set; } = true;

        private double executionTimer = 0d;

        public UnitAI(UnitEntity unit)
        {
            me = unit;
        }

        public virtual void Update(double lastTick)
        {
            if (me.IsAlive)
            {
                autoTimer.Update(lastTick);

                if (HasVictim(out UnitEntity victim))
                {
                    if (resettingFromCombat)
                        return;

                    if (IsOutsideBoundary())
                        return;

                    if (me.IsCasting())
                        return;

                    me.MovementManager?.Chase(victim, me.GetPropertyValue(Property.MoveSpeedMultiplier) * 7f, MAX_ATTACK_RANGE);

                    if (executionTimer > 0d)
                    {
                        executionTimer -= lastTick;
                        if (executionTimer < double.Epsilon)
                            executionTimer = 0d;
                        else
                            return;
                    }
                    
                    if (combatTimer.IsTicking || combatTimer.HasElapsed)
                    {
                        combatTimer.Update(lastTick);

                        if (combatTimer.HasElapsed)
                        {
                            if (me.Position.GetDistance(victim.Position) <= specialAbility.TargetMaxRange)
                            {
                                me.MovementManager?.StopSpline();
                                me.MovementManager?.BroadcastCommands();
                                me.MovementManager?.SetRotation(me.Position.GetRotationTo(victim.Position));
                                if (useSpecialAbility)
                                    me.CastSpell(specialAbility.Id, new SpellParameters
                                    {
                                        PrimaryTargetId = victim.Guid
                                    });
                                combatTimer.Reset();
                                return;
                            }
                        }
                    }

                    DoAutoAttack();
                }

                if (resettingFromCombat)
                {
                    if (CanLeash())
                    {
                        if (me.MovementManager?.MoveTo(me.LeashPosition, me.GetPropertyValue(Property.MoveSpeedMultiplier) * 10f * 1.5f) ?? true)
                            Reset();
                    }
                    else
                        Reset();
                }
            }
        }

        /// <summary>
        /// Returns whether this <see cref="UnitEntity"/> has a target victim to attack. This checks the threat list's highest entity and returns it if one exists.
        /// </summary>
        protected virtual bool HasVictim(out UnitEntity victim)
        {
            victim = null;

            if (!me.InCombat)
                return false;

            if (me.GetCurrentVictim(out victim))
                if (victim.IsAlive)
                    return true;

            ExitCombat();

            return false;
        }

        /// <summary>
        /// Returns true if this <see cref="UnitEntity"/> is beyond Leash Range and will return.
        /// </summary>
        /// <remarks>This can be overridden or updated to account for things like room boundaries, or non radial checks.</remarks>
        protected virtual bool IsOutsideBoundary()
        {
            if (me.Position.GetDistance(me.LeashPosition) > me.LeashRange)
            {
                ExitCombat();
                return true;
            }

            return false;
        }

        public void ExitCombat()
        {
            combatTimer.Reset(false);
            resettingFromCombat = true;
            me.ThreatManager.ClearThreatList();
        }

        private void Reset()
        {
            me.MovementManager?.SetRotation(me.LeashRotation, true);
            me.ModifyHealth(me.MaxHealth);
            resettingFromCombat = false;
        }

        public virtual void OnEnterCombat()
        {
            combatTimer.Reset(true);
        }

        public virtual void OnExitCombat()
        {
            ExitCombat();
        }

        public virtual void OnDeath(UnitEntity killer)
        {
            me.MovementManager?.StopSpline();
            me.MovementManager?.BroadcastCommands();

            Vector3 rotation = Vector3.Zero;
            if (me.GetCurrentVictim(out UnitEntity victim))
                rotation = me.Position.GetRotationTo(victim.Position);
            else
                rotation = me.Rotation;

            me.MovementManager?.SetRotation(rotation);
        }

        protected void DoAutoAttack()
        {
            if (me.IsCasting() || !me.InCombat || !me.IsAlive)
                return;

            if (!me.GetCurrentVictim(out UnitEntity victim))
                return;

            uint spell4Id = autoAttacks[attackIndex];
            Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(spell4Id);
            if (spell4Entry == null)
                throw new ArgumentOutOfRangeException("spell4Id", $"{spell4Id} not found in Spell4 Entries.");

            if (me.Position.GetDistance(victim.Position) > spell4Entry.TargetMaxRange)
                return;

            if (!autoTimer.HasElapsed)
                return;

            me.CastSpell(spell4Id, new SpellParameters
            {
                PrimaryTargetId = victim.Guid
            });
            if (attackIndex == autoAttacks.Count - 1)
                attackIndex = 0;
            else
                attackIndex++;
            autoTimer.Reset(true);
        }

        /// <summary>
        /// For use by the NpcExecutionDelay <see cref="SpellEffectType"/> that delays this entity's AI Update loop.
        /// </summary>
        public void AddExecutionDelay(double delay)
        {
            delay /= 1000d;

            if (delay > executionTimer)
                executionTimer = delay;
        }
    }
}
