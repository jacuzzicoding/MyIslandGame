using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MyIslandGame.Crafting;
using MyIslandGame.Core.Resources;
using MyIslandGame.ECS;
using MyIslandGame.Input;
using MyIslandGame.ECS.Systems;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Manages UI elements and rendering.
    /// </summary>
    public class UIManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private SpriteFont _defaultFont;
        
        // UI Components
        private InventoryUI _inventoryUI;
        private CraftingUI _craftingUI;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIManager"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public UIManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }
        
        /// <summary>
        /// Initialize UI components
        /// </summary>
        public void Initialize(
            EntityManager entityManager, 
            InputManager inputManager,
            ResourceManager resourceManager,
            CraftingSystem craftingSystem,
            InventorySystem inventorySystem)
        {
            // Initialize inventory UI
            _inventoryUI = new InventoryUI(
                _spriteBatch, 
                _graphicsDevice, 
                inventorySystem, 
                entityManager, 
                _defaultFont);
            
            // Initialize crafting UI
            _craftingUI = new CraftingUI(
                craftingSystem,
                inputManager,
                entityManager,
                this,
                _graphicsDevice,
                resourceManager);
        }
        
        /// <summary>
        /// Loads content for UI components
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _defaultFont = content.Load<SpriteFont>("Fonts/DefaultFont");
            
            // Load content for UI components
            if (_craftingUI != null)
            {
                _craftingUI.LoadContent(content);
            }
        }
        
        /// <summary>
        /// Updates all UI components
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Update inventory UI
            if (_inventoryUI != null)
            {
                _inventoryUI.Update();
            }
            
            // Update crafting UI
            if (_craftingUI != null)
            {
                _craftingUI.Update(gameTime);
            }
        }
        
        /// <summary>
        /// Draws all UI components
        /// </summary>
        public void Draw()
        {
            // Draw inventory UI
            if (_inventoryUI != null)
            {
                _inventoryUI.Draw();
            }
            
            // Draw crafting UI if active
            if (_craftingUI != null)
            {
                _craftingUI.Draw(_spriteBatch);
            }
        }

        /// <summary>
        /// Sets the default font to use for text.
        /// </summary>
        /// <param name="font">The sprite font to use.</param>
        public void SetDefaultFont(SpriteFont font)
        {
            _defaultFont = font ?? throw new ArgumentNullException(nameof(font));
        }
        
        /// <summary>
        /// Draws a text string on the screen.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position on screen.</param>
        /// <param name="color">The text color.</param>
        /// <param name="scale">The text scale.</param>
        public void DrawText(string text, Vector2 position, Color color, float scale = 1.0f)
        {
            if (_defaultFont == null)
            {
                throw new InvalidOperationException("Default font not set. Call SetDefaultFont first.");
            }
            
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_defaultFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
        
        /// <summary>
        /// Draws a debug panel with the given text strings.
        /// </summary>
        /// <param name="debugTexts">The list of debug text strings.</param>
        /// <param name="position">The position of the debug panel.</param>
        /// <param name="padding">The padding around text.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="textColor">The text color.</param>
        public void DrawDebugPanel(
            IEnumerable<string> debugTexts,
            Vector2 position,
            float padding = 10f,
            Color? backgroundColor = null,
            Color? textColor = null)
        {
            if (_defaultFont == null)
            {
                throw new InvalidOperationException("Default font not set. Call SetDefaultFont first.");
            }
            
            // Use default colors if not specified
            backgroundColor ??= new Color(0, 0, 0, 150);
            textColor ??= Color.White;
            
            // Calculate panel dimensions
            float maxWidth = 0;
            float totalHeight = 0;
            List<string> textList = new List<string>();
            
            foreach (string text in debugTexts)
            {
                textList.Add(text);
                Vector2 size = _defaultFont.MeasureString(text);
                maxWidth = Math.Max(maxWidth, size.X);
                totalHeight += size.Y;
            }
            
            // Add padding
            Rectangle panelRect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)(maxWidth + padding * 2),
                (int)(totalHeight + padding * 2 + (textList.Count - 1) * padding / 2));
            
            // Draw panel background
            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            _spriteBatch.Begin();
            
            // Draw background
            _spriteBatch.Draw(pixel, panelRect, backgroundColor.Value);
            
            // Draw text
            float currentY = position.Y + padding;
            foreach (string text in textList)
            {
                _spriteBatch.DrawString(
                    _defaultFont,
                    text,
                    new Vector2(position.X + padding, currentY),
                    textColor.Value);
                
                currentY += _defaultFont.MeasureString(text).Y + padding / 2;
            }
            
            _spriteBatch.End();
            
            // Clean up the temporary texture
            pixel.Dispose();
        }
    }
}
