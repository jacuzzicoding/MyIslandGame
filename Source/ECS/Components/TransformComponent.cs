using Microsoft.Xna.Framework;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that handles position, rotation, and scale of an entity.
    /// </summary>
    public class TransformComponent : Component
    {
        /// <summary>
        /// Gets or sets the position of the entity.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// Gets or sets the rotation of the entity in radians.
        /// </summary>
        public float Rotation { get; set; }
        
        /// <summary>
        /// Gets or sets the scale of the entity.
        /// </summary>
        public Vector2 Scale { get; set; } = Vector2.One;
        
        /// <summary>
        /// Gets or sets the depth layer of the entity (used for rendering order).
        /// </summary>
        public float Layer { get; set; } = 0f;
        
        /// <summary>
        /// Gets the world matrix for this transform.
        /// </summary>
        public Matrix WorldMatrix => Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
                                    Matrix.CreateRotationZ(Rotation) *
                                    Matrix.CreateTranslation(Position.X, Position.Y, 0f);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformComponent"/> class.
        /// </summary>
        public TransformComponent()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformComponent"/> class with a specified position.
        /// </summary>
        /// <param name="position">The initial position.</param>
        public TransformComponent(Vector2 position)
        {
            Position = position;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformComponent"/> class with specified position, rotation, and scale.
        /// </summary>
        /// <param name="position">The initial position.</param>
        /// <param name="rotation">The initial rotation in radians.</param>
        /// <param name="scale">The initial scale.</param>
        public TransformComponent(Vector2 position, float rotation, Vector2 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
