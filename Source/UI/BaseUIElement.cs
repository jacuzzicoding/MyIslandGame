using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Base class for UI elements providing common functionality.
    /// </summary>
    public abstract class BaseUIElement : IUIElement
    {
        protected GraphicsDevice _graphicsDevice;
        
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
        /// Initializes a new instance of the <see cref="BaseUIElement"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        protected BaseUIElement(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }
        
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
        /// Creates a texture filled with a solid color.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="color">The color to fill the texture with.</param>
        /// <returns>A new texture filled with the specified color.</returns>
        protected Texture2D CreateColorTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(_graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Determines whether the specified point is within the bounds of this element.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is within the bounds, otherwise false.</returns>
        protected bool ContainsPoint(Point point)
        {
            return Bounds.Contains(point);
        }
        
        /// <summary>
        /// Draws text with a shadow effect.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position to draw at.</param>
        /// <param name="color">The text color.</param>
        /// <param name="shadowColor">The shadow color.</param>
        /// <param name="shadowOffset">The shadow offset.</param>
        protected void DrawTextWithShadow(
            SpriteBatch spriteBatch,
            SpriteFont font,
            string text,
            Vector2 position,
            Color color,
            Color? shadowColor = null,
            Vector2? shadowOffset = null)
        {
            if (font == null || string.IsNullOrEmpty(text))
                return;
                
            Color effectiveShadowColor = shadowColor ?? new Color(0, 0, 0, 128);
            Vector2 effectiveShadowOffset = shadowOffset ?? new Vector2(1, 1);
            
            // Draw shadow
            spriteBatch.DrawString(font, text, position + effectiveShadowOffset, effectiveShadowColor);
            
            // Draw text
            spriteBatch.DrawString(font, text, position, color);
        }
    }
}
