using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Crafting;

namespace MyIslandGame.UI
{
    /// <summary>
    /// A simplified emergency crafting UI for fallback rendering
    /// </summary>
    public class EmergencyCraftingUI
    {
        private readonly CraftingSystem _craftingSystem;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteFont _font;
        private Texture2D _pixelTexture;

        public EmergencyCraftingUI(CraftingSystem craftingSystem, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            _craftingSystem = craftingSystem ?? throw new ArgumentNullException(nameof(craftingSystem));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _font = font;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_craftingSystem.IsCraftingActive)
                return;
                
            // Safety check - don't call Begin/End here
            
            // Background panel
            DrawRectangle(
                spriteBatch, 
                new Rectangle(
                    _graphicsDevice.Viewport.Width / 2 - 200,
                    _graphicsDevice.Viewport.Height / 2 - 150, 
                    400, 300),
                new Color(255, 0, 255, 150)); // Bright purple so we know it's the emergency UI
            
            // Title text
            string title = "EMERGENCY CRAFTING UI!";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(
                _font,
                title,
                new Vector2(
                    _graphicsDevice.Viewport.Width / 2 - titleSize.X / 2,
                    _graphicsDevice.Viewport.Height / 2 - 130),
                Color.White);
                
            // Draw status text
            spriteBatch.DrawString(
                _font,
                $"Station type: {_craftingSystem.CurrentStation}",
                new Vector2(
                    _graphicsDevice.Viewport.Width / 2 - 150,
                    _graphicsDevice.Viewport.Height / 2 - 90),
                Color.White);
                
            // Draw grid size text
            int gridSize = _craftingSystem.CurrentStation == CraftingStationType.None ? 2 : 3;
            spriteBatch.DrawString(
                _font,
                $"Grid size: {gridSize}x{gridSize}",
                new Vector2(
                    _graphicsDevice.Viewport.Width / 2 - 150,
                    _graphicsDevice.Viewport.Height / 2 - 60),
                Color.White);
                
            // Draw craft grid
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // Draw grid cell
                    DrawRectangle(
                        spriteBatch,
                        new Rectangle(
                            _graphicsDevice.Viewport.Width / 2 - 150 + (x * 70),
                            _graphicsDevice.Viewport.Height / 2 - 30 + (y * 70),
                            60, 60),
                        new Color(200, 200, 255, 200));
                        
                    // Draw cell coordinates
                    spriteBatch.DrawString(
                        _font,
                        $"{x},{y}",
                        new Vector2(
                            _graphicsDevice.Viewport.Width / 2 - 145 + (x * 70),
                            _graphicsDevice.Viewport.Height / 2 - 25 + (y * 70)),
                        Color.Black);
                }
            }
            
            // Draw result slot
            DrawRectangle(
                spriteBatch,
                new Rectangle(
                    _graphicsDevice.Viewport.Width / 2 + 100,
                    _graphicsDevice.Viewport.Height / 2,
                    60, 60),
                new Color(255, 215, 0, 200)); // Gold
                
            // Draw result text
            spriteBatch.DrawString(
                _font,
                "Result",
                new Vector2(
                    _graphicsDevice.Viewport.Width / 2 + 100,
                    _graphicsDevice.Viewport.Height / 2 - 20),
                Color.White);
        }
        
        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            spriteBatch.Draw(_pixelTexture, rectangle, color);
        }
    }
}
