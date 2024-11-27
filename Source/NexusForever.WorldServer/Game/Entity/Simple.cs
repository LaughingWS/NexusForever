using System;
using System.Linq;
using System.Numerics;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Reputation.Static;
using NexusForever.WorldServer.Script;
using EntityModel = NexusForever.Database.World.Model.EntityModel;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.Simple)]
    public class Simple : UnitEntity
    {
        public uint ImprovementGroupId { get; private set; }

        public Action<Simple> afterAddToMap;

        public Simple()
            : base(EntityType.Simple)
        {
        }

        public Simple(uint creatureId, Action<Simple> actionAfterAddToMap = null)
            : base(EntityType.Simple)
        {
            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(creatureId);
            if (entry == null)
                throw new ArgumentNullException();

            CreatureId = creatureId;
            afterAddToMap = actionAfterAddToMap;

            SetBaseProperty(Property.BaseHealth, 101.0f);

            SetStat(Stat.Health, 101u);
            SetStat(Stat.Level, 1u);

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.
                Creature2DisplayGroupEntry.
                Entries.
                FirstOrDefault(d => d.Creature2DisplayGroupId == entry.Creature2DisplayGroupId);
            if (displayGroupEntry != null)
                DisplayInfo = displayGroupEntry.Creature2DisplayInfoId;
        }

        public Simple(Creature2Entry entry, long propId, ushort plugId)
            : base(EntityType.Simple)
        {
            CreatureId = entry.Id;
            ActivePropId = propId;
            WorldSocketId = plugId;
            QuestChecklistIdx = 255;
            Faction1 = (Faction)entry.FactionId;
            Faction2 = (Faction)entry.FactionId;

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.Creature2DisplayGroupEntry.Entries.FirstOrDefault(i => i.Creature2DisplayGroupId == entry.Creature2DisplayGroupId);
            if (displayGroupEntry != null)
                DisplayInfo = displayGroupEntry.Creature2DisplayInfoId;

            CreateFlags |= EntityCreateFlag.SpawnAnimation;
        }
        public Simple(Creature2Entry entry) 
            : base(EntityType.Simple)
        {
            CreatureId = entry.Id;
            Faction1 = (Faction)entry.FactionId;
            Faction2 = (Faction)entry.FactionId;

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.Creature2DisplayGroupEntry.Entries.FirstOrDefault(i => i.Creature2DisplayGroupId == entry.Creature2DisplayGroupId);
            if (displayGroupEntry != null)
                DisplayInfo = displayGroupEntry.Creature2DisplayInfoId;

            Creature2OutfitGroupEntryEntry outfitGroupEntry = GameTableManager.Instance.Creature2OutfitGroupEntry.Entries.FirstOrDefault(i => i.Creature2OutfitGroupId == entry.Creature2OutfitGroupId);
            if (outfitGroupEntry != null)
                OutfitInfo = (ushort)outfitGroupEntry.Creature2OutfitInfoId;

            Properties.Add(Property.BaseHealth, new PropertyValue(Property.BaseHealth, 100f, 109f));
            stats.Add(Stat.Health, new StatValue(Stat.Health, 100u));
            stats.Add(Stat.Level, new StatValue(Stat.Level, 1u));

            CreateFlags |= EntityCreateFlag.SpawnAnimation;
        }

        /*public Simple(Creature2Entry creature)
            : base (EntityType.Simple)
        {
            if (creature == null)
                throw new ArgumentNullException(nameof(creature));

            CreatureId = creature.Id;
            Faction1 = (Faction)creature.FactionId;
            Faction2 = (Faction)creature.FactionId;
        }*/

        public Simple(uint creatureId, uint groupId, uint displayInfo)
            : base (EntityType.Simple)
        {
            CreatureId = creatureId;
            ImprovementGroupId = groupId;

            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(creatureId);
            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.Creature2DisplayGroupEntry.GetEntry(entry.Creature2DisplayGroupId);
            DisplayInfo = displayInfo > 0 ? displayInfo : displayGroupEntry.Creature2DisplayInfoId;

            Faction1 = (Faction)entry.FactionId;
            Faction2 = (Faction)entry.FactionId;
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);
            if (Health == 0u)
            {
                MaxHealth = 101u;
                ModifyHealth((long)MaxHealth);
            }
            QuestChecklistIdx = model.QuestChecklistIdx;

            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnCreate(this);
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            afterAddToMap?.Invoke(this);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new SimpleEntityModel
            {
                CreatureId        = CreatureId,
                QuestChecklistIdx = QuestChecklistIdx
            };
        }

        public override void OnInteract(Player activator)
        {
            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);
            if (entry.DatacubeId != 0u)
                activator.DatacubeManager.AddDatacube((ushort)entry.DatacubeId, int.MaxValue);

            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnActivate(this, activator);
        }

        public override void OnActivateSuccess(Player activator)
        {
            uint progress = (uint)(1 << QuestChecklistIdx);

            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);
            if (entry.DatacubeId != 0u)
            {
                Datacube datacube = activator.DatacubeManager.GetDatacube((ushort)entry.DatacubeId, DatacubeType.Datacube);
                if (datacube == null)
                    activator.DatacubeManager.AddDatacube((ushort)entry.DatacubeId, progress);
                else
                {
                    datacube.Progress |= progress;
                    activator.DatacubeManager.SendDatacube(datacube);
                }
            }

            if (entry.DatacubeVolumeId != 0u)
            {
                Datacube datacube = activator.DatacubeManager.GetDatacube((ushort)entry.DatacubeVolumeId, DatacubeType.Journal);
                if (datacube == null)
                    activator.DatacubeManager.AddDatacubeVolume((ushort)entry.DatacubeVolumeId, progress);
                else
                {
                    datacube.Progress |= progress;
                    activator.DatacubeManager.SendDatacubeVolume(datacube);
                }
            }

            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateEntity, CreatureId, 1u);
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.SucceedCSI, CreatureId, 1u);
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown28, CreatureId, 1u);
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateTargetGroupChecklist, CreatureId, QuestChecklistIdx);
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateTargetGroup, CreatureId, 1u);
            
            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnActivateSuccess(this, activator);
        }
    }
}
