using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that contains information for rendering a sprite.
    /// </summary>
    public class SpriteComponent : Component
    {
        /// <summary>
        /// Gets or sets the texture to draw.
        /// </summary>
        public Texture2D Texture { get; set; }
        
        /// <summary>
        /// Gets or sets the rectangle within the texture to draw (null for the entire texture).
        /// </summary>
        public Rectangle? SourceRectangle { get; set; }
        
        /// <summary>
        /// Gets or sets the color to tint the sprite with.
        /// </summary>
        public Color Color { get; set; } = Color.White;
        
        /// <summary>
        /// Gets or sets the sprite effects to apply.
        /// </summary>
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        
        /// <summary>
        /// Gets or sets the origin point for rotation and scaling (relative to the source rectangle).
        /// </summary>
        public Vector2 Origin { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the sprite should be visible.
        /// </summary>
        public bool Visible { get; set; } = true;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
        /// </summary>
        public SpriteComponent()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteComponent"/> class with a specified texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        public SpriteComponent(Texture2D texture)
        {
            Texture = texture;
            
            if (texture != null)
            {
                // Set default origin to center of texture
                Origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteComponent"/> class with a specified texture and source rectangle.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="sourceRectangle">The rectangle within the texture to draw.</param>
        public SpriteComponent(Texture2D texture, Rectangle sourceRectangle)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            
            // Set default origin to center of source rectangle
            Origin = new Vector2(sourceRectangle.Width / 2f, sourceRectangle.Height / 2f);
        }
        
        /// <summary>
        /// Called when the component is attached to an entity.
        /// </summary>
        public override void OnAttached()
        {
            // If texture exists and we don't have a source rectangle, use the whole texture
            if (Texture != null && !SourceRectangle.HasValue)
            {
                SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            }
        }
    }
}
