using Microsoft.Xna.Framework;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that defines collision boundaries for an entity.
    /// </summary>
    public class ColliderComponent : Component
    {
        /// <summary>
        /// Gets or sets the type of collider.
        /// </summary>
        public ColliderType Type { get; set; } = ColliderType.Rectangle;
        
        /// <summary>
        /// Gets or sets the size of the collider.
        /// </summary>
        public Vector2 Size { get; set; }
        
        /// <summary>
        /// Gets or sets the offset from the entity's position.
        /// </summary>
        public Vector2 Offset { get; set; } = Vector2.Zero;
        
        /// <summary>
        /// Gets or sets a value indicating whether this collider is a trigger.
        /// Triggers generate collision events but don't cause physical responses.
        /// </summary>
        public bool IsTrigger { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether collision is enabled.
        /// </summary>
        public new bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Gets the bounding rectangle in world space based on the entity's position.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        public Rectangle GetBoundingRectangle()
        {
            var transformComponent = Owner.GetComponent<TransformComponent>();
            if (transformComponent == null)
            {
                return Rectangle.Empty;
            }
            
            Vector2 position = transformComponent.Position + Offset;
            Vector2 halfSize = Size / 2f;
            
            return new Rectangle(
                (int)(position.X - halfSize.X),
                (int)(position.Y - halfSize.Y),
                (int)Size.X,
                (int)Size.Y);
        }
        
        /// <summary>
        /// Gets the radius of the collider (if it's a circle).
        /// </summary>
        /// <returns>The radius.</returns>
        public float GetRadius()
        {
            return Size.X / 2f;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ColliderComponent"/> class.
        /// </summary>
        public ColliderComponent()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ColliderComponent"/> class with a specified size.
        /// </summary>
        /// <param name="size">The size of the collider.</param>
        /// <param name="type">The type of collider.</param>
        public ColliderComponent(Vector2 size, ColliderType type = ColliderType.Rectangle)
        {
            Size = size;
            Type = type;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ColliderComponent"/> class with a specified size and offset.
        /// </summary>
        /// <param name="size">The size of the collider.</param>
        /// <param name="offset">The offset from the entity's position.</param>
        /// <param name="type">The type of collider.</param>
        public ColliderComponent(Vector2 size, Vector2 offset, ColliderType type = ColliderType.Rectangle)
        {
            Size = size;
            Offset = offset;
            Type = type;
        }
    }
    
    /// <summary>
    /// Defines the different types of colliders.
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// A rectangle collider.
        /// </summary>
        Rectangle,
        
        /// <summary>
        /// A circle collider.
        /// </summary>
        Circle
    }
}
