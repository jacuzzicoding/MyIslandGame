using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Centralized manager for rendering UI elements with proper layer management
    /// and SpriteBatch handling.
    /// </summary>
    public class UIRenderManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<UILayer, List<IUIElement>> _layeredElements = new Dictionary<UILayer, List<IUIElement>>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIRenderManager"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public UIRenderManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = new SpriteBatch(graphicsDevice);
            
            // Initialize layer collections
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _layeredElements[layer] = new List<IUIElement>();
            }
        }
        
        /// <summary>
        /// Registers a UI element to be managed by this render manager.
        /// </summary>
        /// <param name="element">The UI element to register.</param>
        public void RegisterElement(IUIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
                
            if (!_layeredElements.ContainsKey(element.Layer))
            {
                _layeredElements[element.Layer] = new List<IUIElement>();
            }
            
            if (!_layeredElements[element.Layer].Contains(element))
            {
                _layeredElements[element.Layer].Add(element);
            }
        }
        
        /// <summary>
        /// Unregisters a UI element from this render manager.
        /// </summary>
        /// <param name="element">The UI element to unregister.</param>
        public void UnregisterElement(IUIElement element)
        {
            if (element == null)
                return;
                
            if (_layeredElements.ContainsKey(element.Layer))
            {
                _layeredElements[element.Layer].Remove(element);
            }
        }
        
        /// <summary>
        /// Draws all registered UI elements in their appropriate layers.
        /// </summary>
        public void Draw()
        {
            // Draw each layer with proper Begin/End
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                // Use appropriate SpriteBatch parameters for each layer
                SpriteSortMode sortMode = SpriteSortMode.Deferred;
                BlendState blendState = BlendState.AlphaBlend;
                SamplerState samplerState = SamplerState.PointClamp;
                
                _spriteBatch.Begin(sortMode, blendState, samplerState, null, null, null, null);
                
                // Draw each visible element in this layer
                foreach (IUIElement element in _layeredElements[layer])
                {
                    if (element.IsVisible)
                    {
                        try 
                        {
                            element.Draw(_spriteBatch);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue drawing other elements
                            Console.WriteLine($"Error drawing UI element: {ex.Message}");
                        }
                    }
                }
                
                _spriteBatch.End();
            }
        }
        
        /// <summary>
        /// Updates all active UI elements.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            // Update all active elements
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                foreach (IUIElement element in _layeredElements[layer])
                {
                    if (element.IsActive)
                    {
                        try
                        {
                            element.Update(gameTime);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue updating other elements
                            Console.WriteLine($"Error updating UI element: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
