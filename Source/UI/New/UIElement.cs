using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Base implementation for UI elements.
    /// </summary>
    public abstract class UIElement : IUIElement
    {
        /// <summary>
        /// Gets the bounds of this UI element.
        /// </summary>
        public Rectangle Bounds { get; protected set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this element is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether this element is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets the UI layer this element belongs to.
        /// </summary>
        public UILayer Layer { get; protected set; } = UILayer.Middle;
        
        /// <summary>
        /// Gets or sets the parent container of this element.
        /// </summary>
        public IUIContainer Parent { get; set; }
        
        /// <summary>
        /// Initializes this UI element.
        /// </summary>
        public abstract void Initialize();
        
        /// <summary>
        /// Updates this UI element.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public abstract void Update(GameTime gameTime);
        
        /// <summary>
        /// Draws this UI element.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public abstract void Draw(SpriteBatch spriteBatch);
        
        /// <summary>
        /// Handles input for this UI element.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if input was handled, otherwise false.</returns>
        public abstract bool HandleInput(InputManager inputManager);
        
        /// <summary>
        /// Gets the absolute position of this UI element, accounting for parent containers.
        /// </summary>
        /// <returns>The absolute position.</returns>
        public Vector2 GetAbsolutePosition()
        {
            Vector2 position = new Vector2(Bounds.X, Bounds.Y);
            
            if (Parent != null)
            {
                position += Parent.GetAbsolutePosition();
            }
            
            return position;
        }
        
        /// <summary>
        /// Sets the position of this UI element.
        /// </summary>
        /// <param name="position">The new position.</param>
        public virtual void SetPosition(Vector2 position)
        {
            Bounds = new Rectangle((int)position.X, (int)position.Y, Bounds.Width, Bounds.Height);
        }
        
        /// <summary>
        /// Sets the size of this UI element.
        /// </summary>
        /// <param name="size">The new size.</param>
        public virtual void SetSize(Vector2 size)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y, (int)size.X, (int)size.Y);
        }
        
        /// <summary>
        /// Creates a texture filled with a solid color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="color">The color to fill the texture with.</param>
        /// <returns>A new texture filled with the specified color.</returns>
        protected static Texture2D CreateColorTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            
            texture.SetData(data);
            return texture;
        }
    }
}