using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Interface for all UI elements, providing a standard API.
    /// </summary>
    public interface IUIElement
    {
        /// <summary>
        /// Gets the bounds of this UI element.
        /// </summary>
        Rectangle Bounds { get; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this element is visible.
        /// </summary>
        bool IsVisible { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this element is active.
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Gets the UI layer this element belongs to.
        /// </summary>
        UILayer Layer { get; }
        
        /// <summary>
        /// Initializes this UI element.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Updates this UI element.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        void Update(GameTime gameTime);
        
        /// <summary>
        /// Draws this UI element.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        void Draw(SpriteBatch spriteBatch);
        
        /// <summary>
        /// Handles input for this UI element.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if input was handled, otherwise false.</returns>
        bool HandleInput(InputManager inputManager);
    }
}
