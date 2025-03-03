using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Core;
using MyIslandGame.ECS.Components;

namespace MyIslandGame.ECS.Systems
{
    /// <summary>
    /// System that manages environmental objects, including growth and regeneration.
    /// </summary>
    public class EnvironmentalObjectSystem : System
    {
        private readonly TimeManager _timeManager;
        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentalObjectSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="timeManager">The time manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public EnvironmentalObjectSystem(EntityManager entityManager, TimeManager timeManager, GraphicsDevice graphicsDevice)
            : base(entityManager)
        {
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }

        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Get all entities with environmental object components
            var entities = GetInterestingEntities();

            foreach (var entity in entities)
            {
                var environmentalComponent = entity.GetComponent<EnvironmentalObjectComponent>();
                
                // Update growth and regeneration
                environmentalComponent.Update(gameTime, _timeManager);
            }
        }

        /// <summary>
        /// Determines whether this system is interested in the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the system is interested, otherwise false.</returns>
        public override bool IsInterestedIn(Entity entity)
        {
            return entity.HasComponent<EnvironmentalObjectComponent>();
        }
    }
}
