using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MyIslandGame.ECS
{
    /// <summary>
    /// Base class for all systems in the Entity Component System.
    /// Systems contain logic that operates on entities with specific component combinations.
    /// </summary>
    public abstract class System
    {
        /// <summary>
        /// Gets the entity manager that this system works with.
        /// </summary>
        protected EntityManager EntityManager { get; }
        
        /// <summary>
        /// Gets a value indicating whether this system is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Gets a value indicating whether this system is initialized.
        /// </summary>
        public bool Initialized { get; private set; } = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="System"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager to use.</param>
        protected System(EntityManager entityManager)
        {
            EntityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
        }
        
        /// <summary>
        /// Initializes this system.
        /// </summary>
        public virtual void Initialize()
        {
            Initialized = true;
        }
        
        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public abstract void Update(GameTime gameTime);
        
        /// <summary>
        /// Determines whether this system is interested in the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the system is interested, otherwise false.</returns>
        public abstract bool IsInterestedIn(Entity entity);
        
        /// <summary>
        /// Gets all entities that this system is interested in.
        /// </summary>
        /// <returns>An enumerable collection of entities.</returns>
        protected IEnumerable<Entity> GetInterestingEntities()
        {
            foreach (var entity in EntityManager.GetEntities())
            {
                if (entity.IsActive && IsInterestedIn(entity))
                {
                    yield return entity;
                }
            }
        }
    }
}
