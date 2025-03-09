using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Canvas for grouping UI elements by layer.
    /// </summary>
    public class UICanvas
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<UILayer, List<IUIElement>> _elements = new Dictionary<UILayer, List<IUIElement>>();
        private readonly Dictionary<UILayer, SpriteBatchSettings> _layerSettings;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UICanvas"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public UICanvas(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = new SpriteBatch(graphicsDevice);
            
            // Initialize layer collections
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _elements[layer] = new List<IUIElement>();
            }
            
            // Initialize layer-specific SpriteBatch settings
            _layerSettings = new Dictionary<UILayer, SpriteBatchSettings>
            {
                // Default settings for each layer
                [UILayer.Background] = new SpriteBatchSettings(
                    SpriteSortMode.Deferred, 
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp),
                    
                [UILayer.Middle] = new SpriteBatchSettings(
                    SpriteSortMode.Deferred, 
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp),
                    
                [UILayer.Foreground] = new SpriteBatchSettings(
                    SpriteSortMode.Deferred, 
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp),
                    
                [UILayer.HUD] = new SpriteBatchSettings(
                    SpriteSortMode.Deferred, 
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp),
                    
                [UILayer.Debug] = new SpriteBatchSettings(
                    SpriteSortMode.Deferred, 
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp)
            };
        }
        
        /// <summary>
        /// Initializes all UI elements.
        /// </summary>
        public void Initialize()
        {
            foreach (var layerElements in _elements.Values)
            {
                foreach (var element in layerElements)
                {
                    element.Initialize();
                }
            }
        }
        
        /// <summary>
        /// Updates all active UI elements.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            foreach (var layerElements in _elements.Values)
            {
                foreach (var element in layerElements)
                {
                    if (element.IsActive)
                    {
                        element.Update(gameTime);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws all visible UI elements.
        /// </summary>
        public void Draw()
        {
            // Draw each layer with a separate SpriteBatch session
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerElements = _elements[layer];
                var settings = _layerSettings[layer];
                
                if (layerElements.Count == 0 || !layerElements.Any(e => e.IsVisible))
                    continue;
                    
                try
                {
                    // Begin SpriteBatch with layer-specific settings
                    _spriteBatch.Begin(
                        settings.SortMode,
                        settings.BlendState,
                        settings.SamplerState,
                        settings.DepthStencilState,
                        settings.RasterizerState,
                        settings.Effect,
                        settings.TransformMatrix);
                    
                    // Draw visible elements
                    foreach (var element in layerElements)
                    {
                        if (element.IsVisible)
                        {
                            element.Draw(_spriteBatch);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error drawing UI layer {layer}: {ex.Message}");
                }
                finally
                {
                    // Ensure SpriteBatch is ended properly
                    _spriteBatch.End();
                }
            }
        }
        
        /// <summary>
        /// Handles input for all active UI elements, prioritizing elements in higher layers.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if any element handled the input, otherwise false.</returns>
        public bool HandleInput(InputManager inputManager)
        {
            // Process input for elements from top to bottom layer
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)).Cast<UILayer>().Reverse())
            {
                var layerElements = _elements[layer];
                
                for (int i = layerElements.Count - 1; i >= 0; i--)
                {
                    var element = layerElements[i];
                    
                    if (element.IsActive && element.IsVisible)
                    {
                        if (element.HandleInput(inputManager))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Adds a UI element to a specific layer.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="layer">The layer to add the element to.</param>
        public void AddElement(IUIElement element, UILayer layer = UILayer.Middle)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
                
            _elements[layer].Add(element);
        }
        
        /// <summary>
        /// Removes a UI element from all layers.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void RemoveElement(IUIElement element)
        {
            if (element == null)
                return;
                
            foreach (var layerElements in _elements.Values)
            {
                layerElements.Remove(element);
            }
        }
        
        /// <summary>
        /// Configures SpriteBatch settings for a specific UI layer.
        /// </summary>
        /// <param name="layer">The layer to configure settings for.</param>
        /// <param name="settings">The SpriteBatch settings to use.</param>
        public void ConfigureLayerSettings(UILayer layer, SpriteBatchSettings settings)
        {
            _layerSettings[layer] = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        /// <summary>
        /// Clears all elements from all layers.
        /// </summary>
        public void Clear()
        {
            foreach (var layerElements in _elements.Values)
            {
                layerElements.Clear();
            }
        }
    }
}