using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.Crafting;

namespace MyIslandGame.UI
{
    /// <summary>
    /// A direct rendering crafting UI that bypasses the regular UI system
    /// for debugging and emergency use.
    /// </summary>
    public class DirectCraftingUI
    {
        private readonly CraftingSystem _craftingSystem;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private Texture2D _pixelTexture;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectCraftingUI"/> class.
        /// </summary>
        /// <param name="craftingSystem">The crafting system.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="font">The font to use for drawing text.</param>
        public DirectCraftingUI(CraftingSystem craftingSystem, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _craftingSystem = craftingSystem ?? throw new ArgumentNullException(nameof(craftingSystem));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _font = font;
            
            // Create our own SpriteBatch to ensure independence from other UI systems
            _spriteBatch = new SpriteBatch(graphicsDevice);
            
            // Create the pixel texture for drawing rectangles
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            Console.WriteLine("DirectCraftingUI: Initialized");
        }
        
        /// <summary>
        /// Draws the crafting UI directly to the screen.
        /// </summary>
        public void Draw()
        {
            // Skip if crafting is not active
            if (!_craftingSystem.IsCraftingActive)
            {
                //Console.WriteLine("DirectCraftingUI: Crafting not active, skipping");
                return;
            }
            
            Console.WriteLine("DirectCraftingUI: Drawing direct UI");
            
            try
            {
                // Handle Begin/End here as we're completely independent
                _spriteBatch.Begin();
                
                // Background panel
                DrawRectangle(
                    new Rectangle(
                        _graphicsDevice.Viewport.Width / 2 - 200,
                        _graphicsDevice.Viewport.Height / 2 - 150, 
                        400, 300),
                    new Color(0, 128, 0, 200)); // Green for direct UI
                
                // Title text
                string title = "DIRECT CRAFTING INTERFACE";
                Vector2 titleSize = _font.MeasureString(title);
                _spriteBatch.DrawString(
                    _font,
                    title,
                    new Vector2(
                        _graphicsDevice.Viewport.Width / 2 - titleSize.X / 2,
                        _graphicsDevice.Viewport.Height / 2 - 130),
                    Color.White);
                    
                // Draw status text
                _spriteBatch.DrawString(
                    _font,
                    $"Station type: {_craftingSystem.CurrentStation}",
                    new Vector2(
                        _graphicsDevice.Viewport.Width / 2 - 150,
                        _graphicsDevice.Viewport.Height / 2 - 90),
                    Color.White);
                    
                // Draw grid size text
                int gridSize = _craftingSystem.CurrentStation == CraftingStationType.None ? 2 : 3;
                _spriteBatch.DrawString(
                    _font,
                    $"Grid size: {gridSize}x{gridSize}",
                    new Vector2(
                        _graphicsDevice.Viewport.Width / 2 - 150,
                        _graphicsDevice.Viewport.Height / 2 - 60),
                    Color.White);
                
                // Hint text
                _spriteBatch.DrawString(
                    _font,
                    "Press ESC to close crafting",
                    new Vector2(
                        _graphicsDevice.Viewport.Width / 2 - 100,
                        _graphicsDevice.Viewport.Height / 2 + 120),
                    Color.Yellow);
                    
                // Draw craft grid
                for (int y = 0; y < gridSize; y++)
                {
                    for (int x = 0; x < gridSize; x++)
                    {
                        // Draw grid cell
                        DrawRectangle(
                            new Rectangle(
                                _graphicsDevice.Viewport.Width / 2 - 150 + (x * 70),
                                _graphicsDevice.Viewport.Height / 2 - 30 + (y * 70),
                                60, 60),
                            new Color(200, 200, 255, 200));
                            
                        // Draw cell contents if not empty
                        var slot = _craftingSystem.CraftingGrid.GetSlot(x, y);
                        if (!slot.IsEmpty)
                        {
                            // Draw item name
                            string itemName = slot.Item.Name;
                            _spriteBatch.DrawString(
                                _font,
                                itemName,
                                new Vector2(
                                    _graphicsDevice.Viewport.Width / 2 - 145 + (x * 70),
                                    _graphicsDevice.Viewport.Height / 2 - 25 + (y * 70)),
                                Color.White);
                            
                            // Draw quantity
                            if (slot.Quantity > 1)
                            {
                                _spriteBatch.DrawString(
                                    _font,
                                    slot.Quantity.ToString(),
                                    new Vector2(
                                        _graphicsDevice.Viewport.Width / 2 - 145 + (x * 70) + 45,
                                        _graphicsDevice.Viewport.Height / 2 - 25 + (y * 70) + 45),
                                    Color.Yellow);
                            }
                        }
                        else
                        {
                            // Draw cell coordinates
                            _spriteBatch.DrawString(
                                _font,
                                $"{x},{y}",
                                new Vector2(
                                    _graphicsDevice.Viewport.Width / 2 - 145 + (x * 70),
                                    _graphicsDevice.Viewport.Height / 2 - 25 + (y * 70)),
                                Color.DarkBlue);
                        }
                    }
                }
                
                // Draw result slot
                DrawRectangle(
                    new Rectangle(
                        _graphicsDevice.Viewport.Width / 2 + 100,
                        _graphicsDevice.Viewport.Height / 2,
                        60, 60),
                    new Color(255, 215, 0, 200)); // Gold
                    
                // Draw result text
                _spriteBatch.DrawString(
                    _font,
                    "Result",
                    new Vector2(
                        _graphicsDevice.Viewport.Width / 2 + 100,
                        _graphicsDevice.Viewport.Height / 2 - 20),
                    Color.White);
            }
            finally
            {
                // Always end the SpriteBatch
                _spriteBatch.End();
            }
        }
        
        /// <summary>
        /// Draws a rectangle using the pixel texture.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw.</param>
        /// <param name="color">The color to use.</param>
        private void DrawRectangle(Rectangle rectangle, Color color)
        {
            _spriteBatch.Draw(_pixelTexture, rectangle, color);
        }
        
        /// <summary>
        /// Updates the crafting UI, handling input if necessary.
        /// </summary>
        public void Update()
        {
            if (!_craftingSystem.IsCraftingActive)
                return;
            
            // Check for mouse input
            MouseState mouseState = Mouse.GetState();
            
            // Simple grid size for calculations
            int gridSize = _craftingSystem.CurrentStation == CraftingStationType.None ? 2 : 3;
            
            // Check for clicking in grid cells
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    Rectangle cellRect = new Rectangle(
                        _graphicsDevice.Viewport.Width / 2 - 150 + (x * 70),
                        _graphicsDevice.Viewport.Height / 2 - 30 + (y * 70),
                        60, 60);
                    
                    // If mouse clicked on this cell
                    if (cellRect.Contains(mouseState.Position) && 
                        mouseState.LeftButton == ButtonState.Pressed)
                    {
                        Console.WriteLine($"DirectCraftingUI: Clicked on grid cell {x},{y}");
                        // Handle cell click (implement if needed)
                    }
                }
            }
            
            // Check for clicking result slot
            Rectangle resultRect = new Rectangle(
                _graphicsDevice.Viewport.Width / 2 + 100,
                _graphicsDevice.Viewport.Height / 2,
                60, 60);
            
            if (resultRect.Contains(mouseState.Position) && 
                mouseState.LeftButton == ButtonState.Pressed)
            {
                // Try to craft
                bool crafted = _craftingSystem.TryCraft();
                if (crafted)
                {
                    Console.WriteLine("DirectCraftingUI: Item crafted!");
                }
            }
            
            // Close crafting on Escape
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _craftingSystem.CloseCrafting();
                Console.WriteLine("DirectCraftingUI: Closed crafting via Escape key");
            }
        }
    }
}
