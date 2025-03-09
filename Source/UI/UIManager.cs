using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private SpriteFont _debugFont;
        
        // UI Components
        private InventoryUI _inventoryUI;
        private CraftingUI _craftingUI;

        public enum Layer
        {
            Bottom,
            Middle,
            Top
        }

        private Dictionary<string, (Action<SpriteBatch> drawAction, Layer layer)> _uiElements 
            = new Dictionary<string, (Action<SpriteBatch>, Layer)>();
        
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
                _debugFont);
            
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
            try
            {
                Console.WriteLine("Attempting to load font: Fonts/DebugFont");
                _debugFont = content.Load<SpriteFont>("Fonts/DebugFont");
                Console.WriteLine("Successfully loaded font");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading font: {ex.Message}");
                Console.WriteLine($"Content root directory: {content.RootDirectory}");
                
                // Try to list available content files
                try {
                    var contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, "Fonts");
                    Console.WriteLine($"Looking in: {contentPath}");
                    if (Directory.Exists(contentPath)) {
                        Console.WriteLine("Files in directory:");
                        foreach (var file in Directory.GetFiles(contentPath)) {
                            Console.WriteLine($"  - {Path.GetFileName(file)}");
                        }
                    } else {
                        Console.WriteLine("Directory does not exist!");
                    }
                } catch { /* Ignore errors in diagnostic code */ }
                
                throw; // Re-throw the exception
            }
            
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
            // IMPORTANT: Don't call SpriteBatch.Begin or SpriteBatch.End here
            // The calling code should handle this properly
            
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

            // Draw elements in order by layer
            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
            {
                foreach (var element in _uiElements.Where(e => e.Value.layer == layer))
                {
                    element.Value.drawAction(_spriteBatch);
                }
            }
        }

        /// <summary>
        /// Sets the default font to use for text.
        /// </summary>
        /// <param name="font">The sprite font to use.</param>
        public void SetDebugFont(SpriteFont font)
        {
            _debugFont = font ?? throw new ArgumentNullException(nameof(font));
        }
        
        /// <summary>
        /// Gets available debug fonts for use by UI elements.
        /// </summary>
        /// <returns>An array of available fonts.</returns>
        public SpriteFont[] GetDebugFonts()
        {
            if (_debugFont != null)
                return new[] { _debugFont };
            
            return Array.Empty<SpriteFont>();
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
            if (_debugFont == null)
            {
                throw new InvalidOperationException("Default font not set. Call SetDebugFont first.");
            }
            
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_debugFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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
            if (_debugFont == null)
            {
                throw new InvalidOperationException("Default font not set. Call SetDebugFont first.");
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
                Vector2 size = _debugFont.MeasureString(text);
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
                    _debugFont,
                    text,
                    new Vector2(position.X + padding, currentY),
                    textColor.Value);
                
                currentY += _debugFont.MeasureString(text).Y + padding / 2;
            }
            
            _spriteBatch.End();
            
            // Clean up the temporary texture
            pixel.Dispose();
        }

        public void RegisterUIElement(string id, Action<SpriteBatch> drawAction, Layer layer)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
                
            _uiElements[id] = (drawAction, layer);
        }
    }
}
