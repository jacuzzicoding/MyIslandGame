using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Central manager for UI elements.
    /// </summary>
    public class UIManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly UICanvas _rootCanvas;
        private readonly Dictionary<string, IUIElement> _registeredElements = new Dictionary<string, IUIElement>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIManager"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public UIManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _rootCanvas = new UICanvas(graphicsDevice);
        }
        
        /// <summary>
        /// Initializes the UI manager and all registered elements.
        /// </summary>
        public void Initialize()
        {
            _rootCanvas.Initialize();
        }
        
        /// <summary>
        /// Updates all active UI elements.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            _rootCanvas.Update(gameTime);
        }
        
        /// <summary>
        /// Draws all visible UI elements.
        /// </summary>
        public void Draw()
        {
            _rootCanvas.Draw();
        }
        
        /// <summary>
        /// Handles input for all active UI elements.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if any element handled the input, otherwise false.</returns>
        public bool HandleInput(InputManager inputManager)
        {
            return _rootCanvas.HandleInput(inputManager);
        }
        
        /// <summary>
        /// Registers a UI element with the manager.
        /// </summary>
        /// <param name="id">The unique identifier for the element.</param>
        /// <param name="element">The element to register.</param>
        public void RegisterElement(string id, IUIElement element)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
                
            if (element == null)
                throw new ArgumentNullException(nameof(element));
                
            _registeredElements[id] = element;
        }
        
        /// <summary>
        /// Unregisters a UI element from the manager.
        /// </summary>
        /// <param name="id">The unique identifier of the element to unregister.</param>
        public void UnregisterElement(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _registeredElements.Remove(id);
            }
        }
        
        /// <summary>
        /// Gets a registered UI element by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the element.</param>
        /// <returns>The element if found, otherwise null.</returns>
        public IUIElement GetElement(string id)
        {
            return _registeredElements.TryGetValue(id, out var element) ? element : null;
        }
        
        /// <summary>
        /// Gets a registered UI element by its ID, cast to a specific type.
        /// </summary>
        /// <typeparam name="T">The type to cast the element to.</typeparam>
        /// <param name="id">The unique identifier of the element.</param>
        /// <returns>The element if found and of the correct type, otherwise null.</returns>
        public T GetElement<T>(string id) where T : class, IUIElement
        {
            var element = GetElement(id);
            return element as T;
        }
        
        /// <summary>
        /// Adds a UI element to the root canvas.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="layer">The layer to add the element to.</param>
        public void AddToCanvas(IUIElement element, UILayer layer = UILayer.Middle)
        {
            _rootCanvas.AddElement(element, layer);
        }
        
        /// <summary>
        /// Removes a UI element from the root canvas.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void RemoveFromCanvas(IUIElement element)
        {
            _rootCanvas.RemoveElement(element);
        }
        
        /// <summary>
        /// Configures SpriteBatch settings for a specific UI layer.
        /// </summary>
        /// <param name="layer">The layer to configure settings for.</param>
        /// <param name="settings">The SpriteBatch settings to use.</param>
        public void ConfigureLayerSettings(UILayer layer, SpriteBatchSettings settings)
        {
            _rootCanvas.ConfigureLayerSettings(layer, settings);
        }
    }
}