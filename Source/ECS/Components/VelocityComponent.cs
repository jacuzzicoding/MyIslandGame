using Microsoft.Xna.Framework;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that handles velocity and movement of an entity.
    /// </summary>
    public class VelocityComponent : Component
    {
        /// <summary>
        /// Gets or sets the current velocity vector.
        /// </summary>
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        
        /// <summary>
        /// Gets or sets the maximum speed of the entity.
        /// </summary>
        public float MaxSpeed { get; set; } = 200f;
        
        /// <summary>
        /// Gets or sets the friction coefficient (0-1).
        /// Higher values cause the entity to slow down more quickly.
        /// </summary>
        public float Friction { get; set; } = 0.1f;
        
        /// <summary>
        /// Gets or sets a value indicating whether physics calculations should be applied.
        /// </summary>
        public new bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityComponent"/> class.
        /// </summary>
        public VelocityComponent()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityComponent"/> class with a specified maximum speed.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed of the entity.</param>
        public VelocityComponent(float maxSpeed)
        {
            MaxSpeed = maxSpeed;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityComponent"/> class with a specified maximum speed and friction.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed of the entity.</param>
        /// <param name="friction">The friction coefficient.</param>
        public VelocityComponent(float maxSpeed, float friction)
        {
            MaxSpeed = maxSpeed;
            Friction = friction;
        }
        
        /// <summary>
        /// Applies a force to the entity, changing its velocity.
        /// </summary>
        /// <param name="force">The force vector to apply.</param>
        public void ApplyForce(Vector2 force)
        {
            Velocity += force;
            
            // Clamp to max speed
            if (Velocity.Length() > MaxSpeed)
            {
                Velocity.Normalize();
                Velocity *= MaxSpeed;
            }
        }
    }
}
