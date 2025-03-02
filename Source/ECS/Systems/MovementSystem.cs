using Microsoft.Xna.Framework;
using MyIslandGame.ECS.Components;

namespace MyIslandGame.ECS.Systems
{
    /// <summary>
    /// System that handles moving entities based on their velocity.
    /// </summary>
    public class MovementSystem : System
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        public MovementSystem(EntityManager entityManager)
            : base(entityManager)
        {
        }
        
        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            foreach (var entity in GetInterestingEntities())
            {
                var transform = entity.GetComponent<TransformComponent>();
                var velocity = entity.GetComponent<VelocityComponent>();
                
                if (!velocity.Enabled)
                {
                    continue;
                }
                
                // Apply friction
                if (velocity.Friction > 0 && velocity.Velocity != Vector2.Zero)
                {
                    velocity.Velocity *= (1f - velocity.Friction);
                    
                    // If velocity is very small, set it to zero to avoid tiny movements
                    if (velocity.Velocity.Length() < 0.01f)
                    {
                        velocity.Velocity = Vector2.Zero;
                    }
                }
                
                // Update position based on velocity
                transform.Position += velocity.Velocity * deltaTime;
            }
        }
        
        /// <summary>
        /// Determines whether this system is interested in the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the system is interested, otherwise false.</returns>
        public override bool IsInterestedIn(Entity entity)
        {
            return entity.HasComponent<TransformComponent>() && entity.HasComponent<VelocityComponent>();
        }
    }
}
