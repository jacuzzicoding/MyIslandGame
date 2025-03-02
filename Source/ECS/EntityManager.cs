using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MyIslandGame.ECS
{
    /// <summary>
    /// Manages all entities and systems in the game.
    /// </summary>
    public class EntityManager
    {
        private readonly List<Entity> _entities = new();
        private readonly List<Entity> _entitiesToAdd = new();
        private readonly List<Entity> _entitiesToRemove = new();
        
        private readonly List<System> _systems = new();
        private readonly List<System> _systemsToAdd = new();
        private readonly List<System> _systemsToRemove = new();
        
        private readonly Dictionary<Type, List<Entity>> _componentEntityMap = new();
        
        /// <summary>
        /// Event raised when a component is added to an entity.
        /// </summary>
        public event Action<Entity, Component> ComponentAdded;
        
        /// <summary>
        /// Event raised when a component is removed from an entity.
        /// </summary>
        public event Action<Entity, Component> ComponentRemoved;
        
        /// <summary>
        /// Event raised when an entity is added.
        /// </summary>
        public event Action<Entity> EntityAdded;
        
        /// <summary>
        /// Event raised when an entity is removed.
        /// </summary>
        public event Action<Entity> EntityRemoved;
        
        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <returns>The created entity.</returns>
        public Entity CreateEntity()
        {
            var entity = new Entity();
            entity.EntityManager = this;
            _entitiesToAdd.Add(entity);
            return entity;
        }
        
        /// <summary>
        /// Adds an existing entity to the manager.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <exception cref="ArgumentException">Thrown if the entity is already managed by another manager.</exception>
        public void AddEntity(Entity entity)
        {
            if (entity.EntityManager != null && entity.EntityManager != this)
            {
                throw new ArgumentException("Entity is already managed by another EntityManager", nameof(entity));
            }
            
            entity.EntityManager = this;
            _entitiesToAdd.Add(entity);
        }
        
        /// <summary>
        /// Gets an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        public Entity GetEntityById(Guid id)
        {
            return _entities.FirstOrDefault(e => e.Id == id);
        }
        
        /// <summary>
        /// Destroys an entity, removing it from management.
        /// </summary>
        /// <param name="entity">The entity to destroy.</param>
        public void DestroyEntity(Entity entity)
        {
            if (entity.EntityManager == this)
            {
                entity.Destroy();
                _entitiesToRemove.Add(entity);
            }
        }
        
        /// <summary>
        /// Adds a system to the manager.
        /// </summary>
        /// <param name="system">The system to add.</param>
        public void AddSystem(System system)
        {
            _systemsToAdd.Add(system);
        }
        
        /// <summary>
        /// Removes a system from the manager.
        /// </summary>
        /// <param name="system">The system to remove.</param>
        public void RemoveSystem(System system)
        {
            _systemsToRemove.Add(system);
        }
        
        /// <summary>
        /// Gets all entities managed by this manager.
        /// </summary>
        /// <returns>An enumerable collection of entities.</returns>
        public IEnumerable<Entity> GetEntities()
        {
            return _entities;
        }
        
        /// <summary>
        /// Gets entities that have all of the specified component types.
        /// </summary>
        /// <param name="componentTypes">The component types to filter by.</param>
        /// <returns>An enumerable collection of entities.</returns>
        public IEnumerable<Entity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            if (componentTypes.Length == 0)
            {
                return _entities;
            }
            
            IEnumerable<Entity> result = null;
            
            foreach (var type in componentTypes)
            {
                if (_componentEntityMap.TryGetValue(type, out var entities))
                {
                    if (result == null)
                    {
                        result = entities;
                    }
                    else
                    {
                        result = result.Intersect(entities);
                    }
                }
                else
                {
                    return Enumerable.Empty<Entity>();
                }
            }
            
            return result ?? Enumerable.Empty<Entity>();
        }
        
        /// <summary>
        /// Updates all systems.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            ProcessPendingEntities();
            ProcessPendingSystems();
            
            foreach (var system in _systems)
            {
                if (system.Enabled)
                {
                    if (!system.Initialized)
                    {
                        system.Initialize();
                    }
                    
                    system.Update(gameTime);
                }
            }
            
            ProcessPendingEntities();
        }
        
        /// <summary>
        /// Notifies the manager that a component was added to an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="component">The component.</param>
        internal void OnComponentAdded(Entity entity, Component component)
        {
            var componentType = component.GetType();
            
            if (!_componentEntityMap.TryGetValue(componentType, out var entities))
            {
                entities = new List<Entity>();
                _componentEntityMap[componentType] = entities;
            }
            
            if (!entities.Contains(entity))
            {
                entities.Add(entity);
            }
            
            ComponentAdded?.Invoke(entity, component);
        }
        
        /// <summary>
        /// Notifies the manager that a component was removed from an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="component">The component.</param>
        internal void OnComponentRemoved(Entity entity, Component component)
        {
            var componentType = component.GetType();
            
            if (_componentEntityMap.TryGetValue(componentType, out var entities))
            {
                entities.Remove(entity);
            }
            
            ComponentRemoved?.Invoke(entity, component);
        }
        
        /// <summary>
        /// Processes pending entity additions and removals.
        /// </summary>
        private void ProcessPendingEntities()
        {
            // Process additions
            foreach (var entity in _entitiesToAdd)
            {
                _entities.Add(entity);
                
                // Add to component maps
                foreach (var component in entity.GetAllComponents())
                {
                    var componentType = component.GetType();
                    
                    if (!_componentEntityMap.TryGetValue(componentType, out var entities))
                    {
                        entities = new List<Entity>();
                        _componentEntityMap[componentType] = entities;
                    }
                    
                    entities.Add(entity);
                }
                
                EntityAdded?.Invoke(entity);
            }
            
            _entitiesToAdd.Clear();
            
            // Process removals
            foreach (var entity in _entitiesToRemove)
            {
                _entities.Remove(entity);
                
                // Remove from component maps
                foreach (var componentType in _componentEntityMap.Keys.ToArray())
                {
                    if (_componentEntityMap.TryGetValue(componentType, out var entities))
                    {
                        entities.Remove(entity);
                    }
                }
                
                entity.OnDestroy();
                EntityRemoved?.Invoke(entity);
            }
            
            _entitiesToRemove.Clear();
        }
        
        /// <summary>
        /// Processes pending system additions and removals.
        /// </summary>
        private void ProcessPendingSystems()
        {
            // Process additions
            foreach (var system in _systemsToAdd)
            {
                _systems.Add(system);
            }
            
            _systemsToAdd.Clear();
            
            // Process removals
            foreach (var system in _systemsToRemove)
            {
                _systems.Remove(system);
            }
            
            _systemsToRemove.Clear();
        }
    }
}
