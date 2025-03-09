using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.Crafting;
using MyIslandGame.ECS;
using MyIslandGame.Input;
using MyIslandGame.UI;
using MyIslandGame.UI.New;

namespace MyIslandGame
{
    /// <summary>
    /// This is an example Game1 class showing how to integrate the new UI system.
    /// This is not meant to be used directly - instead, copy the relevant sections
    /// into your existing Game1 class.
    /// </summary>
    public class GameIntegrationExample : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Game systems
        private EntityManager _entityManager;
        private InputManager _inputManager;
        private CraftingSystem _craftingSystem;
        
        // UI system (new)
        private UISystem _uiSystem;
        
        // Debug UI elements
        private UIPanel _debugPanel;
        private UILabel _fpsLabel;
        private UILabel _positionLabel;
        private int _frameCounter;
        private TimeSpan _elapsedTime;
        
        /// <summary>
        /// Initializes a new instance of the game.
        /// </summary>
        public GameIntegrationExample()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        
        /// <summary>
        /// Initializes the game.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize core game systems
            _entityManager = new EntityManager();
            _inputManager = new InputManager();
            
            // Create dependency for CraftingSystem
            var recipeManager = new RecipeManager(null, GraphicsDevice);
            var resourceManager = new Core.Resources.ResourceManager(GraphicsDevice);
            
            _craftingSystem = new CraftingSystem(_entityManager, recipeManager, _inputManager, resourceManager);
            
            // Create and initialize the UI system
            _uiSystem = new UISystem(
                this,
                GraphicsDevice,
                _entityManager,
                _inputManager,
                _craftingSystem);
                
            _uiSystem.Initialize();
            
            base.Initialize();
        }
        
        /// <summary>
        /// Loads game content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load content for game systems
            // ...
            
            // Load content for UI system
            _uiSystem.LoadContent(Content);
            
            // Create debug UI
            CreateDebugUI();
        }
        
        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update input first
            _inputManager.Update();
            
            // Check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            
            // First handle UI input
            bool uiHandledInput = _uiSystem.HandleInput();
            
            // Only process game input if UI didn't handle it and no UI is visible
            if (!uiHandledInput && !_uiSystem.InventoryVisible && !_uiSystem.CraftingVisible)
            {
                // Process game input...
                ProcessGameInput();
            }
            
            // Update game systems
            _entityManager.Update(gameTime);
            
            // Update UI
            _uiSystem.Update(gameTime);
            
            // Update debug info
            UpdateDebugInfo(gameTime);
            
            base.Update(gameTime);
        }
        
        /// <summary>
        /// Draws the game.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Draw game world
            // ...
            
            // Draw UI on top
            _uiSystem.Draw();
            
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Creates the debug UI panel.
        /// </summary>
        private void CreateDebugUI()
        {
            // Load debug font if not already loaded
            SpriteFont debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
            
            // Create debug panel
            _debugPanel = new UIPanel(
                GraphicsDevice,
                new Rectangle(10, 10, 200, 80),
                new Color(0, 0, 0, 128),
                Color.Gray,
                1);
                
            // Create FPS label
            _fpsLabel = new UILabel(
                debugFont,
                "FPS: 0",
                new Vector2(10, 10),
                Color.White);
                
            // Create position label
            _positionLabel = new UILabel(
                debugFont,
                "Pos: 0, 0",
                new Vector2(10, 30),
                Color.White);
                
            // Add labels to panel
            _debugPanel.AddChild(_fpsLabel);
            _debugPanel.AddChild(_positionLabel);
            
            // Add button to toggle debug panel
            var toggleButton = new UIButton(
                GraphicsDevice,
                new Rectangle(10, 50, 180, 20),
                new Color(40, 40, 40, 180),
                new Color(60, 60, 60, 180),
                new Color(80, 80, 80, 180),
                debugFont,
                "Toggle Debug Info",
                Color.White);
                
            toggleButton.Click += (sender, args) => {
                _debugPanel.IsVisible = !_debugPanel.IsVisible;
            };
            
            _debugPanel.AddChild(toggleButton);
            
            // Register and add to canvas
            _uiSystem.UIManager.RegisterElement("debugPanel", _debugPanel);
            _uiSystem.UIManager.AddToCanvas(_debugPanel, UILayer.Debug);
        }
        
        /// <summary>
        /// Updates the debug information.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        private void UpdateDebugInfo(GameTime gameTime)
        {
            // Update FPS counter
            _elapsedTime += gameTime.ElapsedGameTime;
            
            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                float fps = _frameCounter / (float)_elapsedTime.TotalSeconds;
                _fpsLabel.Text = $"FPS: {fps:0.##}";
                
                _frameCounter = 0;
                _elapsedTime = TimeSpan.Zero;
            }
            
            _frameCounter++;
            
            // Update position label
            // Example: Get player position from entity manager
            Vector2 playerPos = GetPlayerPosition();
            _positionLabel.Text = $"Pos: {playerPos.X:0.#}, {playerPos.Y:0.#}";
        }
        
        /// <summary>
        /// Gets the player position.
        /// </summary>
        /// <returns>The player position.</returns>
        private Vector2 GetPlayerPosition()
        {
            // Example: Get player position from entity manager
            // In a real implementation, this would come from the player entity
            return Vector2.Zero;
        }
        
        /// <summary>
        /// Processes game input when UI is not active.
        /// </summary>
        private void ProcessGameInput()
        {
            // Example: Process game input when UI is not handling it
            // This would include player movement, interaction, etc.
            
            // Check for UI toggle keys (these are also handled in UISystem.HandleInputToggling)
            // but we include them here as an example of how to interact with the UI system
            
            // Show inventory
            if (_inputManager.WasKeyPressed(Keys.I))
            {
                _uiSystem.InventoryVisible = !_uiSystem.InventoryVisible;
            }
            
            // Show crafting
            if (_inputManager.WasKeyPressed(Keys.C))
            {
                _uiSystem.CraftingVisible = !_uiSystem.CraftingVisible;
            }
            
            // Add a custom UI element dynamically as an example
            if (_inputManager.WasKeyPressed(Keys.F1))
            {
                CreateTooltip("Press F2 to close this tooltip", TimeSpan.FromSeconds(5));
            }
            
            // Remove the tooltip
            if (_inputManager.WasKeyPressed(Keys.F2))
            {
                var tooltip = _uiSystem.UIManager.GetElement("tooltip");
                if (tooltip != null)
                {
                    _uiSystem.UIManager.RemoveFromCanvas(tooltip);
                    _uiSystem.UIManager.UnregisterElement("tooltip");
                }
            }
        }
        
        /// <summary>
        /// Creates a temporary tooltip.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        /// <param name="duration">How long to display the tooltip.</param>
        private void CreateTooltip(string text, TimeSpan duration)
        {
            // Load font if needed
            SpriteFont font = Content.Load<SpriteFont>("Fonts/DebugFont");
            
            // Measure text to determine panel size
            Vector2 textSize = font.MeasureString(text);
            
            // Create tooltip panel
            var tooltip = new UIPanel(
                GraphicsDevice,
                new Rectangle(
                    (GraphicsDevice.Viewport.Width - (int)textSize.X - 20) / 2,
                    20,
                    (int)textSize.X + 20,
                    (int)textSize.Y + 20),
                new Color(0, 0, 0, 200),
                Color.Yellow,
                2);
                
            // Add text
            var label = new UILabel(
                font,
                text,
                new Vector2(10, 10),
                Color.White);
                
            tooltip.AddChild(label);
            
            // Register and add to canvas
            _uiSystem.UIManager.RegisterElement("tooltip", tooltip);
            _uiSystem.UIManager.AddToCanvas(tooltip, UILayer.HUD);
            
            // Set up a timer to remove the tooltip after the duration
            var timer = new System.Threading.Timer(
                (state) => {
                    // Remove tooltip
                    _uiSystem.UIManager.RemoveFromCanvas(tooltip);
                    _uiSystem.UIManager.UnregisterElement("tooltip");
                },
                null,
                duration,
                System.Threading.Timeout.InfiniteTimeSpan);
        }
    }
}
