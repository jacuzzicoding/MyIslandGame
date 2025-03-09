using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Crafting;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.Input;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// Main UI system that coordinates all UI components for the game.
    /// </summary>
    public class UISystem
    {
        private readonly Game _game;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly UIManager _uiManager;
        private readonly EntityManager _entityManager;
        private readonly InputManager _inputManager;
        
        private SpriteFont _defaultFont;
        private SpriteFont _titleFont;
        
        private UIInventoryGrid _inventoryGrid;
        private UIInventoryGrid _hotbar;
        private UICraftingGrid _craftingGrid;
        
        private Entity _playerEntity;
        private InventoryComponent _playerInventory;
        private CraftingSystem _craftingSystem;
        
        private bool _inventoryVisible;
        private bool _craftingVisible;
        
        /// <summary>
        /// Gets the UI manager.
        /// </summary>
        public UIManager UIManager => _uiManager;
        
        /// <summary>
        /// Gets or sets a value indicating whether the inventory is visible.
        /// </summary>
        public bool InventoryVisible
        {
            get => _inventoryVisible;
            set
            {
                _inventoryVisible = value;
                
                if (_inventoryGrid != null)
                {
                    _inventoryGrid.IsVisible = value;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the crafting interface is visible.
        /// </summary>
        public bool CraftingVisible
        {
            get => _craftingVisible;
            set
            {
                _craftingVisible = value;
                
                if (_craftingGrid != null)
                {
                    _craftingGrid.IsVisible = value;
                }
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UISystem"/> class.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="inputManager">The input manager.</param>
        /// <param name="craftingSystem">The crafting system.</param>
        public UISystem(
            Game game,
            GraphicsDevice graphicsDevice,
            EntityManager entityManager,
            InputManager inputManager,
            CraftingSystem craftingSystem)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _craftingSystem = craftingSystem ?? throw new ArgumentNullException(nameof(craftingSystem));
            
            _uiManager = new UIManager(graphicsDevice);
        }
        
        /// <summary>
        /// Initializes the UI system and all UI components.
        /// </summary>
        public void Initialize()
        {
            // Find player entity and inventory
            FindPlayerEntity();
            
            if (_playerEntity == null || _playerInventory == null)
            {
                Console.WriteLine("Warning: Player entity or inventory not found during UI initialization");
                return;
            }
            
            // Initialize the UI manager
            _uiManager.Initialize();
        }
        
        /// <summary>
        /// Loads content for the UI system.
        /// </summary>
        /// <param name="content">The content manager.</param>
        public void LoadContent(ContentManager content)
        {
            try
            {
                // Load fonts
                _defaultFont = content.Load<SpriteFont>("Fonts/DebugFont");
                _titleFont = content.Load<SpriteFont>("Fonts/DefaultFont");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading UI fonts: {ex.Message}");
                
                // Create a simple default font if loading fails
                _defaultFont = null;
                _titleFont = null;
            }
            
            // Create UI components
            CreateUIComponents();
        }
        
        /// <summary>
        /// Updates the UI system and all UI components.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            // Handle toggling UI visibility
            HandleInputToggling();
            
            // Update the UI manager
            _uiManager.Update(gameTime);
        }
        
        /// <summary>
        /// Draws the UI system and all UI components.
        /// </summary>
        public void Draw()
        {
            _uiManager.Draw();
        }
        
        /// <summary>
        /// Handles input for the UI system.
        /// </summary>
        /// <returns>True if input was handled by the UI, otherwise false.</returns>
        public bool HandleInput()
        {
            return _uiManager.HandleInput(_inputManager);
        }
        
        /// <summary>
        /// Finds the player entity and inventory.
        /// </summary>
        private void FindPlayerEntity()
        {
            // Find player entity (assuming single player game)
            var playerEntities = _entityManager.GetEntitiesWithComponents(
                typeof(PlayerComponent), 
                typeof(InventoryComponent));
                
            var playerEnumerator = playerEntities.GetEnumerator();
            
            if (playerEnumerator.MoveNext())
            {
                _playerEntity = playerEnumerator.Current;
                _playerInventory = _playerEntity.GetComponent<InventoryComponent>();
            }
        }
        
        /// <summary>
        /// Creates all UI components.
        /// </summary>
        private void CreateUIComponents()
        {
            if (_playerInventory == null)
            {
                // Try to find player again
                FindPlayerEntity();
                
                if (_playerInventory == null)
                {
                    Console.WriteLine("Error: Player inventory not found, cannot create UI components");
                    return;
                }
            }
            
            // Get screen dimensions
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            
            // Create hotbar
            int hotbarWidth = _playerInventory.Inventory.HotbarSize * 45;
            int hotbarHeight = 50;
            
            _hotbar = new UIInventoryGrid(
                _graphicsDevice,
                new Rectangle(
                    (screenWidth - hotbarWidth) / 2,
                    screenHeight - hotbarHeight - 10,
                    hotbarWidth,
                    hotbarHeight),
                _defaultFont,
                _playerInventory,
                true);
                
            _uiManager.RegisterElement("hotbar", _hotbar);
            _uiManager.AddToCanvas(_hotbar, UILayer.HUD);
            
            // Create inventory grid (initially hidden)
            int inventoryWidth = 9 * 45;
            int inventoryHeight = 3 * 45;
            
            _inventoryGrid = new UIInventoryGrid(
                _graphicsDevice,
                new Rectangle(
                    (screenWidth - inventoryWidth) / 2,
                    (screenHeight - inventoryHeight) / 2,
                    inventoryWidth,
                    inventoryHeight),
                _defaultFont,
                _playerInventory);
                
            _inventoryGrid.IsVisible = false;
            _uiManager.RegisterElement("inventory", _inventoryGrid);
            _uiManager.AddToCanvas(_inventoryGrid, UILayer.Foreground);
            
            // Create crafting grid (initially hidden)
            int craftingWidth = 300;
            int craftingHeight = 200;
            
            _craftingGrid = new UICraftingGrid(
                _graphicsDevice,
                new Rectangle(
                    (screenWidth - craftingWidth) / 2,
                    (screenHeight - craftingHeight) / 2 - 50,
                    craftingWidth,
                    craftingHeight),
                _defaultFont,
                _craftingSystem,
                _playerInventory,
                _craftingSystem.GetResourceManager());
                
            _craftingGrid.IsVisible = false;
            _uiManager.RegisterElement("crafting", _craftingGrid);
            _uiManager.AddToCanvas(_craftingGrid, UILayer.Foreground);
            
            // Set initial visibility
            InventoryVisible = false;
            CraftingVisible = false;
        }
        
        /// <summary>
        /// Handles input for toggling UI visibility.
        /// </summary>
        private void HandleInputToggling()
        {
            // Toggle inventory with 'I' key
            if (_inputManager.WasKeyPressed(Microsoft.Xna.Framework.Input.Keys.I))
            {
                InventoryVisible = !InventoryVisible;
                
                // Close crafting when opening inventory
                if (InventoryVisible)
                {
                    CraftingVisible = false;
                }
            }
            
            // Toggle crafting with 'C' key
            if (_inputManager.WasKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
            {
                CraftingVisible = !CraftingVisible;
                
                // Close inventory when opening crafting
                if (CraftingVisible)
                {
                    InventoryVisible = false;
                }
            }
            
            // Close all UI with Escape key
            if (_inputManager.WasKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                InventoryVisible = false;
                CraftingVisible = false;
            }
        }
    }
}