using System.Diagnostics;
using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Map;
using NexusForever.Game.Map.Search;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;
using NexusForever.Shared;
using NexusForever.Shared.Game.Events;

namespace NexusForever.Game.Entity
{
    public abstract class GridEntity : IGridEntity
    {
        public uint Guid { get; protected set; }
        public IBaseMap Map { get; private set; }
        public Vector3 Position { get; protected set; }

        public IMapInfo PreviousMap { get; private set; }

        /// <summary>
        /// Determines if the <see cref="IGridEntity"/> is on a <see cref="IBaseMap"/>.
        /// </summary>
        public bool InWorld => Map != null;

        /// <summary>
        /// Determines if the <see cref="IGridEntity"/> is pending removal from the <see cref="IBaseMap"/>.
        /// </summary>
        public bool PendingRemoval { get; private set; }

        /// <summary>
        /// Distance between <see cref="IGridEntity"/> and a <see cref="IMapGrid"/> for activation.
        /// </summary>
        public float ActivationRange { get; protected set; }

        public float? RangeCheck { get; private set; }

        private readonly Dictionary<uint, IGridEntity> inRangeEntities = [];

        protected readonly Dictionary<uint, IGridEntity> visibleEntities = new();
        private readonly HashSet<(uint GridX, uint GridZ)> visibleGrids = new();

        protected IScriptCollection scriptCollection;

        protected readonly EventQueue eventQueue = new();

        /// <summary>
        /// Initialise <see cref="IGridEntity"/>
        /// </summary>
        public void Initialise()
        {
            InitialiseScriptCollection(null);
        }

        /// <summary>
        /// Initialise <see cref="IScriptCollection"/> for <see cref="IGridEntity"/>.
        /// </summary>
        protected abstract void InitialiseScriptCollection(List<string> names);

        public virtual void Dispose()
        {
            if (scriptCollection != null)
                ScriptManager.Instance.Unload(scriptCollection);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public virtual void Update(double lastTick)
        {
            eventQueue.Update(lastTick);
            scriptCollection?.Invoke<IUpdate>(s => s.Update(lastTick));
        }

        /// <summary>
        /// Invoke <see cref="Action{T}"/> against <see cref="IGridEntity"/> script collection.
        /// </summary>
        public void InvokeScriptCollection<T>(Action<T> action)
        {
            scriptCollection?.Invoke(action);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for addition to the <see cref="IBaseMap"/>.
        /// </summary>
        public void AddToMap(IBaseMap map, Vector3 position, OnAddDelegate callback = null)
        {
            Debug.Assert(Map == null);

            if (callback != null)
            {
                map.EnqueueAdd(this, position, (map, guid, vector) =>
                {
                    OnAddToMap(map, guid, vector);
                    callback(map, guid, vector);
                });
            }
            else
                map.EnqueueAdd(this, position, OnAddToMap);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for removal from the <see cref="IBaseMap"/>.
        /// </summary>
        public void RemoveFromMap(OnRemoveDelegate callback = null)
        {
            Debug.Assert(Map != null);

            if (PendingRemoval)
                return;

            PendingRemoval = true;

            if (callback != null)
            {
                Map.EnqueueRemove(this, () =>
                {
                    OnRemoveFromMap();
                    callback();
                });
            }
            else
                Map.EnqueueRemove(this, OnRemoveFromMap);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for relocation on the <see cref="IBaseMap"/>.
        /// </summary>
        public void RelocateOnMap(Vector3 position, OnRelocateDelegate callback = null)
        {
            Debug.Assert(Map != null);

            if (callback != null)
            {
                Map.EnqueueRelocate(this, position, (vector) =>
                {
                    OnRelocate(vector);
                    callback(vector);
                });
            }
            else
                Map.EnqueueRelocate(this, position, OnRelocate);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is enqueued to be added to <see cref="IBaseMap"/>.
        /// </summary>
        public virtual void OnEnqueueAddToMap()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public virtual void OnAddToMap(IBaseMap map, uint guid, Vector3 vector)
        {
            Guid     = guid;
            Map      = map;
            Position = vector;

            UpdateVision();
            UpdateGridVision();
            UpdateRangeChecks();

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnAddToMap(map));
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is enqueued to be removed from <see cref="IBaseMap"/>.
        /// </summary>
        public virtual void OnEnqueueRemoveFromMap()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is removed from <see cref="IBaseMap"/>.
        /// </summary>
        protected virtual void OnRemoveFromMap()
        {
            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnRemoveFromMap(Map));

            foreach (IGridEntity entity in visibleEntities.Values.ToList())
                entity.RemoveVisible(this);

            visibleEntities.Clear();

            foreach ((uint gridX, uint gridZ) in visibleGrids.ToList())
                RemoveVisible(gridX, gridZ);

            visibleGrids.Clear();

            inRangeEntities.Clear();

            PreviousMap = new MapInfo
            {
                Entry = Map.Entry
            };

            Guid           = 0;
            Map            = null;
            PendingRemoval = false;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is relocated.
        /// </summary>
        protected virtual void OnRelocate(Vector3 vector)
        {
            Position = vector;

            UpdateVision();
            UpdateGridVision();
            UpdateRangeChecks();
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> changes zone in the current <see cref="IBaseMap"/>.
        /// </summary>
        protected virtual void OnZoneUpdate()
        {
            // deliberately empty
        }

        /// <summary>
        /// Returns if <see cref="IGridEntity"/> can see supplied <see cref="IGridEntity"/>.
        /// </summary>
        public virtual bool CanSeeEntity(IGridEntity entity)
        {
            return true;
        }

        /// <summary>
        /// Add tracked <see cref="IGridEntity"/> that is in vision range.
        /// </summary>
        public virtual void AddVisible(IGridEntity entity)
        {
            if (!CanSeeEntity(entity))
                return;

            visibleEntities.Add(entity.Guid, entity);

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnAddVisibleEntity(entity));

            CheckEntityInRange(entity);
        }

        /// <summary>
        /// Remove tracked <see cref="IGridEntity"/> that is no longer in vision range.
        /// </summary>
        public virtual void RemoveVisible(IGridEntity entity)
        {
            visibleEntities.Remove(entity.Guid);

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnRemoveVisibleEntity(entity));

            CheckEntityInRange(entity);
        }

        /// <summary>
        /// Return visible <see cref="IGridEntity"/> by supplied guid.
        /// </summary>
        public T GetVisible<T>(uint guid) where T : IGridEntity
        {
            if (!visibleEntities.TryGetValue(guid, out IGridEntity entity))
                return default;
            return (T)entity;
        }

        /// <summary>
        /// Return visible <see cref="IWorldEntity"/> by supplied creature id.
        /// </summary>
        public IEnumerable<T> GetVisibleCreature<T>(uint creatureId) where T : IWorldEntity
        {
            foreach (IGridEntity entity in visibleEntities.Values)
                if (entity is IWorldEntity worldEntity && worldEntity.CreatureId == creatureId)
                    yield return (T)entity;
        }

        /// <summary>
        /// Update all <see cref="IGridEntity"/>'s in vision range.
        /// </summary>
        private void UpdateVision()
        {
            var check = new SearchCheckRange<IGridEntity>();
            check.Initialise(Position, Map.VisionRange);

            List<IGridEntity> entities = Map.Search(Position, Map.VisionRange, check).ToList();

            // new entities now in vision range
            foreach (IGridEntity entity in entities.Except(visibleEntities.Values))
            {
                AddVisible(entity);
                if (entity != this)
                    entity.AddVisible(this);
            }

            // old entities now out of vision range
            foreach (IGridEntity entity in visibleEntities.Values.Except(entities).ToList())
            {
                RemoveVisible(entity);
                entity.RemoveVisible(this);
            }
        }

        /// <summary>
        /// Update all <see cref="IMapGrid"/>'s in vision range.
        /// </summary>
        private void UpdateGridVision()
        {
            Map.GridSearch(Position, Map.VisionRange, out List<IMapGrid> intersectedGrids);
            List<(uint X, uint Z)> visibleGridCoords = intersectedGrids
                .Select(g => g.Coord)
                .ToList();

            // new grids now in vision range
            foreach ((uint gridX, uint gridZ) in visibleGridCoords.Except(visibleGrids))
                AddVisible(gridX, gridZ);

            // old grids now out of vision range
            foreach ((uint gridX, uint gridZ) in visibleGrids.Except(visibleGridCoords).ToList())
                RemoveVisible(gridX, gridZ);
        }

        protected virtual void AddVisible(uint gridX, uint gridZ)
        {
            visibleGrids.Add((gridX, gridZ));
        }

        protected virtual void RemoveVisible(uint gridX, uint gridZ)
        {
            visibleGrids.Remove((gridX, gridZ));
        }

        protected void UpdateRangeChecks()
        {
            foreach (IGridEntity entity in visibleEntities.Values)
                entity.CheckEntityInRange(this);
        }

        /// <summary>
        /// Set range check for <see cref="IGridEntity"/>.
        /// </summary>
        public void SetInRangeCheck(float range)
        {
            if (Map?.VisionRange < range)
                throw new ArgumentOutOfRangeException();

            RangeCheck = range;
        }

        /// <summary>
        /// Checks if the provided <see cref="IGridEntity"/> is at a range to trigger an event on this <see cref="IGridEntity"/>.
        /// </summary>
        public virtual void CheckEntityInRange(IGridEntity target)
        {
            if (!RangeCheck.HasValue)
                return;

            if (!HasEnteredRange(target) && IsInRange(target))
                AddToRange(target);
            else if (HasEnteredRange(target) && !IsInRange(target))
                RemoveFromRange(target);
        }

        private bool IsInRange(IGridEntity target)
        {
            if (target is IUnitEntity unitEntity)
                if (!unitEntity.IsAlive)
                    return false;

            return Position.GetDistance(target.Position) < RangeCheck;
        }

        private bool HasEnteredRange(IGridEntity target)
        {
            return inRangeEntities.ContainsKey(target.Guid);
        }

        protected virtual void AddToRange(IGridEntity entity)
        {
            inRangeEntities.Add(entity.Guid, entity);

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnEnterRange(entity));
        }

        protected virtual void RemoveFromRange(IGridEntity entity)
        {
            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnExitRange(entity));

            inRangeEntities.Remove(entity.Guid);
        }

        /// <summary>
        /// Returns all <see cref="IGridEntity"/> in range of this <see cref="IGridEntity"/>.
        /// </summary>
        public IEnumerable<T> GetInRange<T>(uint guid) where T : IGridEntity
        {
            return visibleEntities.Values.Cast<T>();
        }
    }
}
