using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Centralized manager for rendering UI elements with proper layer management
    /// and SpriteBatch handling. This class ensures consistent rendering of UI elements
    /// by managing SpriteBatch sessions and maintaining layer-based rendering order.
    /// </summary>
    public class UIRenderManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<UILayer, List<IUIElement>> _layeredElements;
        private readonly Dictionary<UILayer, SpriteBatchSettings> _layerSettings;
        
        /// <summary>
        /// Gets the total count of registered UI elements.
        /// </summary>
        public int ElementCount => _layeredElements.Values.Sum(layer => layer.Count);
        
        /// <summary>
        /// Gets the counts of elements by layer.
        /// </summary>
        public IReadOnlyDictionary<UILayer, int> ElementCountsByLayer => 
            _layeredElements.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIRenderManager"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used for rendering.</param>
        public UIRenderManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = new SpriteBatch(graphicsDevice);
            
            // Initialize layer collections
            _layeredElements = new Dictionary<UILayer, List<IUIElement>>();
            
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _layeredElements[layer] = new List<IUIElement>();
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
        /// Registers a UI element to be managed by this render manager.
        /// </summary>
        /// <param name="element">The UI element to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the element is null.</exception>
        public void RegisterElement(IUIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
                
            if (!_layeredElements.ContainsKey(element.Layer))
            {
                _layeredElements[element.Layer] = new List<IUIElement>();
                Console.WriteLine($"Created new layer collection for {element.Layer}");
            }
            
            if (!_layeredElements[element.Layer].Contains(element))
            {
                _layeredElements[element.Layer].Add(element);
                Console.WriteLine($"Registered UI element to {element.Layer} layer (Total: {_layeredElements[element.Layer].Count})");
            }
            else
            {
                Console.WriteLine($"UI element already registered in {element.Layer} layer");
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
                bool removed = _layeredElements[element.Layer].Remove(element);
                if (removed)
                {
                    Console.WriteLine($"Unregistered UI element from {element.Layer} layer");
                }
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
        /// Draws all registered UI elements in their appropriate layers.
        /// Each layer is rendered with its own SpriteBatch Begin/End session.
        /// </summary>
        public void Draw()
        {
            Console.WriteLine("UIRenderManager.Draw: Starting UI rendering process");
            
            // Show counts of elements in each layer
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                int totalElements = _layeredElements[layer].Count;
                int visibleElements = _layeredElements[layer].Count(e => e.IsVisible);
                Console.WriteLine($"UIRenderManager.Draw: Layer {layer} has {totalElements} elements ({visibleElements} visible)");
            }
            
            // Draw each layer with proper Begin/End
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                // Skip empty layers
                if (_layeredElements[layer].Count == 0 || !_layeredElements[layer].Any(e => e.IsVisible))
                {
                    Console.WriteLine($"UIRenderManager.Draw: Skipping empty or invisible layer {layer}");
                    continue;
                }
                
                Console.WriteLine($"UIRenderManager.Draw: Drawing layer {layer} with {_layeredElements[layer].Count(e => e.IsVisible)} visible elements");
                
                try
                {
                    // Get settings for this layer
                    var settings = _layerSettings[layer];
                    
                    // Begin SpriteBatch with layer-specific settings
                    _spriteBatch.Begin(
                        settings.SortMode,
                        settings.BlendState,
                        settings.SamplerState,
                        settings.DepthStencilState,
                        settings.RasterizerState,
                        settings.Effect,
                        settings.TransformMatrix);
                    
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
                                Console.WriteLine($"Error drawing UI element of type {element.GetType().Name}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during layer {layer} rendering: {ex.Message}");
                }
                finally
                {
                    // Ensure SpriteBatch is always ended properly
                    _spriteBatch.End();
                }
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
                            Console.WriteLine($"Error updating UI element of type {element.GetType().Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Processes input for all active UI elements, prioritizing elements in higher layers.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if any element handled the input, otherwise false.</returns>
        public bool HandleInput(InputManager inputManager)
        {
            // Process input for elements from top to bottom layer
            // This ensures that elements in higher layers get input priority
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)).Cast<UILayer>().Reverse())
            {
                foreach (IUIElement element in _layeredElements[layer])
                {
                    if (element.IsActive && element.IsVisible)
                    {
                        try
                        {
                            if (element.HandleInput(inputManager))
                            {
                                return true; // Input was handled
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error handling input for UI element: {ex.Message}");
                        }
                    }
                }
            }
            
            return false; // No element handled the input
        }
        
        /// <summary>
        /// Gets all UI elements in a specific layer.
        /// </summary>
        /// <param name="layer">The layer to get elements from.</param>
        /// <returns>A read-only collection of UI elements in the specified layer.</returns>
        public IReadOnlyCollection<IUIElement> GetElementsInLayer(UILayer layer)
        {
            return _layeredElements[layer].AsReadOnly();
        }
    }
    
    /// <summary>
    /// Encapsulates SpriteBatch.Begin parameters for a consistent rendering configuration.
    /// </summary>
    public class SpriteBatchSettings
    {
        /// <summary>
        /// Gets the sprite sort mode.
        /// </summary>
        public SpriteSortMode SortMode { get; }
        
        /// <summary>
        /// Gets the blend state.
        /// </summary>
        public BlendState BlendState { get; }
        
        /// <summary>
        /// Gets the sampler state.
        /// </summary>
        public SamplerState SamplerState { get; }
        
        /// <summary>
        /// Gets the depth stencil state.
        /// </summary>
        public DepthStencilState DepthStencilState { get; }
        
        /// <summary>
        /// Gets the rasterizer state.
        /// </summary>
        public RasterizerState RasterizerState { get; }
        
        /// <summary>
        /// Gets the shader effect.
        /// </summary>
        public Effect Effect { get; }
        
        /// <summary>
        /// Gets the transform matrix.
        /// </summary>
        public Matrix? TransformMatrix { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteBatchSettings"/> class with default values.
        /// </summary>
        public SpriteBatchSettings()
            : this(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteBatchSettings"/> class with specified values.
        /// </summary>
        /// <param name="sortMode">The sprite sort mode.</param>
        /// <param name="blendState">The blend state.</param>
        /// <param name="samplerState">The sampler state.</param>
        /// <param name="depthStencilState">The depth stencil state.</param>
        /// <param name="rasterizerState">The rasterizer state.</param>
        /// <param name="effect">The shader effect.</param>
        /// <param name="transformMatrix">The transform matrix.</param>
        public SpriteBatchSettings(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null)
        {
            SortMode = sortMode;
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = samplerState ?? SamplerState.PointClamp;
            DepthStencilState = depthStencilState;
            RasterizerState = rasterizerState;
            Effect = effect;
            TransformMatrix = transformMatrix;
        }
    }
}
