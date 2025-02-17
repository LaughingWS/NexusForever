﻿using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Entity
{
    public class Location : ILocation
    {
        public WorldEntry World { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }

        public Location(WorldEntry world, Vector3 position, Vector3 rotation)
        {
            World    = world;
            Position = position;
            Rotation = rotation;
        }
    }
}