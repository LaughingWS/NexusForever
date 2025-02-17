﻿using System.Collections.Generic;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Database.World.Model
{
    public class EntityModel
    {
        public uint Id { get; set; }
        public EntityType Type { get; set; }
        public uint Creature { get; set; }
        public ushort World { get; set; }
        public ushort Area { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rx { get; set; }
        public float Ry { get; set; }
        public float Rz { get; set; }
        public uint DisplayInfo { get; set; }
        public ushort OutfitInfo { get; set; }
        public ushort Faction1 { get; set; }
        public ushort Faction2 { get; set; }
        public byte QuestChecklistIdx { get; set; }
        public ulong ActivePropId { get; set; }
        public ushort WorldSocketId { get; set; }

        public EntityEventModel EntityEvent { get; set; }
        public ICollection<EntityPropertyModel> EntityProperty { get; set; } = [];
        public ICollection<EntityScriptModel> EntityScript { get; set; } = [];
        public EntitySplineModel EntitySpline { get; set; }
        public EntityVendorModel EntityVendor { get; set; }
        public ICollection<EntityStatModel> EntityStat { get; set; } = new HashSet<EntityStatModel>();
        public ICollection<EntityVendorCategoryModel> EntityVendorCategory { get; set; } = new HashSet<EntityVendorCategoryModel>();
        public ICollection<EntityVendorItemModel> EntityVendorItem { get; set; } = new HashSet<EntityVendorItemModel>();
    }
}
