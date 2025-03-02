using System;
using System.Collections.Generic;
using System.Linq;

namespace MyIslandGame.ECS
{
    /// <summary>
    /// Represents a game entity which is a container for components.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Gets the unique identifier for this entity.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets a value indicating whether this entity is active.
        /// Inactive entities are not processed by systems.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether this entity is marked for destruction.
        /// </summary>
        public bool IsDestroyed { get; private set; } = false;

        /// <summary>
        /// Gets the collection of components attached to this entity.
        /// </summary>
        private readonly Dictionary<Type, Component> _components = new();

        /// <summary>
        /// Reference to the entity manager that owns this entity.
        /// </summary>
        internal EntityManager EntityManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        public Entity()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Adds a component to this entity.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>This entity for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if a component of the same type already exists.</exception>
        public Entity AddComponent(Component component)
        {
            var componentType = component.GetType();
            
            if (_components.ContainsKey(componentType))
            {
                throw new ArgumentException($"Component of type {componentType.Name} already exists on entity {Id}");
            }

            _components[componentType] = component;
            component.Owner = this;
            component.OnAttached();
            
            // Notify the entity manager if it exists
            EntityManager?.OnComponentAdded(this, component);
            
            return this;
        }

        /// <summary>
        /// Removes a component from this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to remove.</typeparam>
        /// <returns>This entity for method chaining.</returns>
        public Entity RemoveComponent<T>() where T : Component
        {
            var componentType = typeof(T);
            
            if (_components.TryGetValue(componentType, out var component))
            {
                component.OnDetached();
                _components.Remove(componentType);
                
                // Notify the entity manager if it exists
                EntityManager?.OnComponentRemoved(this, component);
            }
            
            return this;
        }

        /// <summary>
        /// Gets a component of the specified type from this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to get.</typeparam>
        /// <returns>The component, or null if no component of the specified type exists.</returns>
        public T GetComponent<T>() where T : Component
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                return (T)component;
            }
            
            return null;
        }
        
        /// <summary>
        /// Checks if this entity has a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to check for.</typeparam>
        /// <returns>True if the entity has a component of the specified type, otherwise false.</returns>
        public bool HasComponent<T>() where T : Component
        {
            return _components.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// Checks if this entity has all of the specified component types.
        /// </summary>
        /// <param name="componentTypes">The component types to check for.</param>
        /// <returns>True if the entity has all specified component types, otherwise false.</returns>
        public bool HasComponents(params Type[] componentTypes)
        {
            return componentTypes.All(type => _components.ContainsKey(type));
        }
        
        /// <summary>
        /// Gets all components attached to this entity.
        /// </summary>
        /// <returns>An enumerable collection of components.</returns>
        public IEnumerable<Component> GetAllComponents()
        {
            return _components.Values;
        }
        
        /// <summary>
        /// Marks this entity for destruction.
        /// </summary>
        public void Destroy()
        {
            IsDestroyed = true;
        }
        
        /// <summary>
        /// Internal method called when entity is about to be destroyed.
        /// </summary>
        internal void OnDestroy()
        {
            foreach (var component in _components.Values)
            {
                component.OnDetached();
            }
            
            _components.Clear();
            EntityManager = null;
        }
    }
}
