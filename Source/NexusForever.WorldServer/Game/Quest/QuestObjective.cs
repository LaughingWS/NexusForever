﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Quest.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Quest
{
    public class QuestObjective : IUpdate
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        
        public QuestInfo QuestInfo { get; }
        public QuestObjectiveInfo ObjectiveInfo { get; }

        public QuestObjectiveType Type => (QuestObjectiveType)ObjectiveInfo.Type;

        public byte Index { get; }

        public uint Progress
        {
            get => progress;
            set
            {
                saveMask |= QuestObjectiveSaveMask.Progress;
                progress = value;
            }
        }

        private uint progress;

        public uint? Timer
        {
            get => timer;
            set
            {
                saveMask |= QuestObjectiveSaveMask.Timer;
                timer = value;
            }
        }

        private uint? timer;

        private List<uint> targetIds = new List<uint>();

        private QuestObjectiveSaveMask saveMask;

        /// <summary>
        /// Create a new <see cref="QuestObjective"/> from an existing database model.
        /// </summary>
        public QuestObjective(QuestInfo questInfo, QuestObjectiveInfo objectiveInfo, CharacterQuestObjectiveModel model)
        {
            QuestInfo     = questInfo;
            ObjectiveInfo = objectiveInfo;
            Index         = model.Index;
            progress      = model.Progress;
            timer         = model.Timer;

            if (IsChecklist() || UsesTargetGroups())
                BuildTargets();
        }

        /// <summary>
        /// Create a new <see cref="QuestObjective"/> from supplied <see cref="QuestObjectiveEntry"/>.
        /// </summary>
        public QuestObjective(QuestInfo questInfo, QuestObjectiveInfo objectiveInfo, byte index)
        {
            QuestInfo     = questInfo;
            ObjectiveInfo = objectiveInfo;
            Index         = index;

            if (objectiveInfo.Entry.MaxTimeAllowedMS != 0u)
            {
                // TODO
            }

            if (IsChecklist() || UsesTargetGroups())
                BuildTargets();

            saveMask = QuestObjectiveSaveMask.Create;
        }

        /// <summary>
        /// Builds the target ID list for this <see cref="QuestObjective"/>.
        /// </summary>
        private void BuildTargets()
        {
            uint targetGroupId = ObjectiveInfo.Entry.Data > 0 ? ObjectiveInfo.Entry.Data : ObjectiveInfo.Entry.TargetGroupIdRewardPane;
            if (targetGroupId == 0u)
                throw new InvalidOperationException();

            targetIds = AssetManager.Instance.GetQuestObjectiveTargetIds(ObjectiveInfo.Id).ToList();
            if (targetIds.Count == 0u)
                return;

            if (targetIds.Count < ObjectiveInfo.Entry.Count)
                for (int i = targetIds.Count - 1; i < ObjectiveInfo.Entry.Count; i++)
                    targetIds.Add(targetIds[0]);
        }

        public void Save(CharacterContext context, ulong characterId)
        {
            if (saveMask == QuestObjectiveSaveMask.None)
                return;

            if ((saveMask & QuestObjectiveSaveMask.Create) != 0)
            {
                context.Add(new CharacterQuestObjectiveModel
                {
                    Id       = characterId,
                    QuestId  = (ushort)QuestInfo.Entry.Id,
                    Index    = Index,
                    Progress = Progress
                });
            }
            else
            {
                var model = new CharacterQuestObjectiveModel
                {
                    Id      = characterId,
                    QuestId = (ushort)QuestInfo.Entry.Id,
                    Index   = Index
                };

                EntityEntry<CharacterQuestObjectiveModel> entity = context.Entry(model);
                if ((saveMask & QuestObjectiveSaveMask.Progress) != 0)
                {
                    model.Progress = Progress;
                    entity.Property(p => p.Progress).IsModified = true;
                }

                if ((saveMask & QuestObjectiveSaveMask.Timer) != 0)
                {
                    // TODO
                }
            }

            saveMask = QuestObjectiveSaveMask.None;
        }

        public void Update(double lastTick)
        {
            // TODO: update timer
        }

        private bool IsDynamic()
        {
            // dynamic objectives have their progress based on percentage rather than count
            return ObjectiveInfo.Type is QuestObjectiveType.KillCreature
                    or QuestObjectiveType.KillTargetGroups
                    or QuestObjectiveType.Unknown15
                    or QuestObjectiveType.KillTargetGroup
                    or QuestObjectiveType.KillCreature2
                && ObjectiveInfo.Entry.Count > 1u
                && !ObjectiveInfo.HasUnknown0200();
        }

        /// <summary>
        /// Return if this <see cref="QuestObjective"/> is a checklist type.
        /// </summary>
        /// <remarks>
        /// Checklist progress is calculated using the QuestChecklistIdx of the entity, and applying that to a bitmask.
        /// Completion is calculated by the number of bits in a bitmask. e.g. 0b01010111 would have a progress of 5 towards the objective count.
        /// </remarks>
        public bool IsChecklist()
        {
            // TODO: Determine other Types that are also Checklists
            return Type == QuestObjectiveType.ActivateTargetGroupChecklist
                || Type == QuestObjectiveType.Unknown10;
        }

        /// <summary>
        /// Return if this <see cref="QuestObjective"/> uses target group data to complete.
        /// </summary>
        public bool UsesTargetGroups()
        {
            // TODO: Determine other Types that are also using Target Groups
            return (Type == QuestObjectiveType.ActivateTargetGroup ||
               Type == QuestObjectiveType.ActivateTargetGroupChecklist ||
               Type == QuestObjectiveType.KillTargetGroup ||
               Type == QuestObjectiveType.KillTargetGroups ||
               Type == QuestObjectiveType.TalkToTargetGroup ||
               Type == QuestObjectiveType.ActivateEntity && ObjectiveInfo.Entry.TargetGroupIdRewardPane != 0u ||
               Type == QuestObjectiveType.Unknown10);
        }

        /// <summary>
        /// Return if the <see cref="QuestObjective"/> has been completed.
        /// </summary>
        public bool IsComplete()
        {
            if (IsChecklist())
                return BitOperations.PopCount(progress) >= GetMaxValue();

            return progress >= GetMaxValue();
        }

        /// <summary>
        /// Return if the <see cref="QuestObjective"/> requires the given ID to complete.
        /// </summary>
        public bool IsTarget(uint id)
        {
            return (ObjectiveInfo.Entry.Data == id || targetIds.Contains(id));
        }

        private uint GetMaxValue()
        {
            if (IsDynamic())
                return 1000u;

            return ObjectiveInfo.Entry.Count;
        }

        /// <summary>
        /// Update object progress with supplied update.
        /// </summary>
        public void ObjectiveUpdate(uint update)
        {
            if (IsChecklist() && IsDynamic())
                throw new QuestException("Unhandled objective update. Objective is Checklist and Dynamic.");

            if (IsChecklist())
            {
                Progress = progress + (uint)(1 << (int)update);
                return;
            }

            if (IsDynamic())
                update = (uint)(((float)update / ObjectiveInfo.Entry.Count) * 1000f);

            Progress = Math.Min(progress + update, GetMaxValue());
        }

        /// <summary>
        /// Complete this <see cref="QuestObjective"/>.
        /// </summary>
        public void Complete()
        {
            if (IsChecklist())
            {
                uint update = 0;
                for (int i = 0; i < GetMaxValue(); i++)
                    update += (uint)(1 << i);

                Progress = update;
                return;
            }

            Progress = GetMaxValue();
        }
    }
}
