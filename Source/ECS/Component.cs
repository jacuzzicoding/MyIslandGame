using System;

namespace MyIslandGame.ECS
{
    /// <summary>
    /// Base class for all components in the Entity Component System.
    /// Components contain data but no logic.
    /// </summary>
    public abstract class Component
    {
        /// <summary>
        /// Gets the entity that owns this component.
        /// </summary>
        public Entity Owner { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating whether this component is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Called when the component is first attached to an entity.
        /// </summary>
        public virtual void OnAttached() { }
        
        /// <summary>
        /// Called when the component is removed from an entity.
        /// </summary>
        public virtual void OnDetached() { }
        
        /// <summary>
        /// Attempts to get another component from the owner entity.
        /// </summary>
        /// <typeparam name="T">The type of component to get.</typeparam>
        /// <returns>The component, or null if it doesn't exist.</returns>
        protected T GetComponent<T>() where T : Component
        {
            if (Owner == null)
            {
                return null;
            }
            
            return Owner.GetComponent<T>();
        }
        
        /// <summary>
        /// Checks if the owner entity has a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to check for.</typeparam>
        /// <returns>True if the component exists, otherwise false.</returns>
        protected bool HasComponent<T>() where T : Component
        {
            if (Owner == null)
            {
                return false;
            }
            
            return Owner.HasComponent<T>();
        }
    }
}
