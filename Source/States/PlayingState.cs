using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.Core;
using MyIslandGame.Core.Resources;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.ECS.Factories;
using MyIslandGame.ECS.Systems;
using MyIslandGame.Input;
using MyIslandGame.Inventory;
using MyIslandGame.Rendering;
using MyIslandGame.UI;
using MyIslandGame.World;

namespace MyIslandGame.States
{
    /// <summary>
    /// The main gameplay state where the player explores and interacts with the island.
    /// </summary>
    public class PlayingState : GameState
    {
        private EntityManager _entityManager;
        private InputManager _inputManager;
        private SpriteBatch _spriteBatch;
        
        private RenderSystem _renderSystem;
        private MovementSystem _movementSystem;
        private CollisionSystem _collisionSystem;
        private EnvironmentalObjectSystem _environmentalObjectSystem;
        private InventorySystem _inventorySystem;
        private GatheringSystem _gatheringSystem;
        
        private Entity _playerEntity;
        private Texture2D _playerTexture;
        private Texture2D _grassTexture;
        
        // World bounds for camera clamping
        private Rectangle _worldBounds;
        
        // Time and UI management
        private TimeManager _gameTimeManager;
        private UIManager _uiManager;
        private SpriteFont _debugFont;
        private Texture2D _lightOverlayTexture;
        private InventoryUI _inventoryUI;
    
        // World and map
        private TileMap _tileMap;
        private WorldGenerator _worldGenerator;
        
        // Resource management
        private ResourceManager _resourceManager;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayingState"/> class.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="stateManager">The game state manager.</param>
        public PlayingState(Game game, GameStateManager stateManager)
            : base(game, stateManager)
        {
            _entityManager = new EntityManager();
            _inputManager = new InputManager();
            
            // Initialize time manager (24 minutes per day with 20 real seconds per game day)
            _gameTimeManager = new TimeManager(1440f, 72f, 480f); // Start at 8:00 AM
            
            // Initialize UI manager
            _uiManager = new UIManager(game.GraphicsDevice);
            
            // Initialize world generator
            _worldGenerator = new WorldGenerator(game.GraphicsDevice);
            
            // Initialize resource manager
            _resourceManager = new ResourceManager(game.GraphicsDevice);
        }
        
        /// <summary>
        /// Initializes this state.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            // Initialize input actions
            _inputManager.RegisterAction("MoveUp", new InputAction().MapKey(Keys.W).MapKey(Keys.Up));
            _inputManager.RegisterAction("MoveDown", new InputAction().MapKey(Keys.S).MapKey(Keys.Down));
            _inputManager.RegisterAction("MoveLeft", new InputAction().MapKey(Keys.A).MapKey(Keys.Left));
            _inputManager.RegisterAction("MoveRight", new InputAction().MapKey(Keys.D).MapKey(Keys.Right));
            _inputManager.RegisterAction("Interact", new InputAction().MapKey(Keys.E).MapKey(Keys.Space));
            
            // Create systems
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderSystem = new RenderSystem(_entityManager, _spriteBatch, GraphicsDevice);
            _movementSystem = new MovementSystem(_entityManager);
            _collisionSystem = new CollisionSystem(_entityManager);
            _environmentalObjectSystem = new EnvironmentalObjectSystem(_entityManager, _gameTimeManager, GraphicsDevice);
            _inventorySystem = new InventorySystem(_entityManager, _inputManager);
            _gatheringSystem = new GatheringSystem(_entityManager, _inputManager, _resourceManager);
            
            // Add systems to entity manager
            _entityManager.AddSystem(_movementSystem);
            _entityManager.AddSystem(_collisionSystem);
            _entityManager.AddSystem(_environmentalObjectSystem);
            _entityManager.AddSystem(_inventorySystem);
            _entityManager.AddSystem(_gatheringSystem);
            
            // Set up collision handlers
            _collisionSystem.CollisionOccurred += HandleCollision;
            _collisionSystem.TriggerEntered += HandleTriggerEnter;
            _collisionSystem.TriggerExited += HandleTriggerExit;
            
            // Set up resource gathering feedback
            _gatheringSystem.ResourceGathered += OnResourceGathered;
        }
        
        /// <summary>
        /// Loads content for this state.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
            
            // Create textures
            _playerTexture = CreateColoredTexture(32, 32, Color.Blue);
            _grassTexture = CreateColoredTexture(64, 64, Color.Green);
            
            // Create a white texture for lighting effects
            _lightOverlayTexture = CreateColoredTexture(1, 1, Color.White);
            
            // Load font (or create a fallback)
            try
            {
                _debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
            }
            catch (Exception)
            {
                // Fallback: Create a temporary debug font message
                Console.WriteLine("Warning: DebugFont not found. Add a SpriteFont to Content/Fonts/DebugFont");
            }
            
            // Set the font in the UI manager
            if (_debugFont != null)
            {
                _uiManager.SetDefaultFont(_debugFont);
            }
            
            // Create player entity
            _playerEntity = _entityManager.CreateEntity();
            
            // Generate the tile map (30x30 tiles, 64 pixels each)
            _tileMap = _worldGenerator.GenerateTestMap(30, 30, 64);
            
            // Set world bounds based on the tile map
            _worldBounds = _tileMap.GetWorldBounds();
            
            // Position player in the center of the map
            Vector2 mapCenter = new Vector2(_worldBounds.Width / 2f, _worldBounds.Height / 2f);
            var playerTransform = new TransformComponent(mapCenter);
            
            var playerSprite = new SpriteComponent(_playerTexture)
            {
                Origin = new Vector2(_playerTexture.Width / 2f, _playerTexture.Height / 2f)
            };
            
            var playerVelocity = new VelocityComponent(200f, 0.1f);
            
            var playerCollider = new ColliderComponent(
                new Vector2(_playerTexture.Width, _playerTexture.Height),
                Vector2.Zero,
                ColliderType.Rectangle);
            
            // Add player component and inventory component
            var playerComponent = new PlayerComponent("Player", 200f);
            var inventoryComponent = new InventoryComponent(27, 9);
            
            _playerEntity.AddComponent(playerTransform);
            _playerEntity.AddComponent(playerSprite);
            _playerEntity.AddComponent(playerVelocity);
            _playerEntity.AddComponent(playerCollider);
            _playerEntity.AddComponent(playerComponent);
            _playerEntity.AddComponent(inventoryComponent);
            
            // Initialize inventory UI
            _inventoryUI = new InventoryUI(_spriteBatch, GraphicsDevice, _inventorySystem, _entityManager, _debugFont);
            
            // Add some starter tools to player inventory
            inventoryComponent.Inventory.TryAddItem(Tool.CreateAxe(GraphicsDevice));
            inventoryComponent.Inventory.TryAddItem(Tool.CreatePickaxe(GraphicsDevice));
            
            // Create environmental objects
            PopulateWorldWithEnvironmentalObjects();
        }
        
        /// <summary>
        /// Creates a simple obstacle entity.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="size">The size.</param>
        /// <param name="color">The color.</param>
        private void CreateObstacle(Vector2 position, Vector2 size, Color color)
        {
            var texture = CreateColoredTexture((int)size.X, (int)size.Y, color);
            
            var entity = _entityManager.CreateEntity();
            
            entity.AddComponent(new TransformComponent(position));
            entity.AddComponent(new SpriteComponent(texture)
            {
                Origin = new Vector2(texture.Width / 2f, texture.Height / 2f)
            });
            entity.AddComponent(new ColliderComponent(size, Vector2.Zero, ColliderType.Rectangle));
        }
        
        /// <summary>
        /// Creates a texture filled with a solid color.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        /// <returns>The created texture.</returns>
        private Texture2D CreateColoredTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(GraphicsDevice, width, height);
            var colorData = new Color[width * height];
            
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = color;
            }
            
            texture.SetData(colorData);
            return texture;
        }
        
        /// <summary>
        /// Called when this state becomes the active state.
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
        }
        
        /// <summary>
        /// Updates this state.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Update input
            _inputManager.Update();
            
            // Handle player movement based on input
            if (_playerEntity != null)
            {
                var velocity = _playerEntity.GetComponent<VelocityComponent>();
                var transform = _playerEntity.GetComponent<TransformComponent>();
                
                Vector2 direction = Vector2.Zero;
                
                if (_inputManager.IsActionActive("MoveUp"))
                {
                    direction.Y -= 1;
                }
                
                if (_inputManager.IsActionActive("MoveDown"))
                {
                    direction.Y += 1;
                }
                
                if (_inputManager.IsActionActive("MoveLeft"))
                {
                    direction.X -= 1;
                }
                
                if (_inputManager.IsActionActive("MoveRight"))
                {
                    direction.X += 1;
                }
                
                // Normalize the direction vector if necessary
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }
                
                // Apply movement to velocity
                velocity.Velocity = direction * velocity.MaxSpeed;
                
                // Check for tile collisions
                Vector2 nextPosition = transform.Position + velocity.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Point nextTile = _tileMap.WorldToTile(nextPosition);
                
                // Check for water tile
                Tile tile = _tileMap.GetTile(nextTile.X, nextTile.Y);
                if (tile != null && tile.IsWater)
                {
                    velocity.Velocity = Vector2.Zero;
                    return;  // Option 1: Return from the method
                    // OR
                    // Option 2: Just remove the continue statement entirely
                }

                // If the next position would be on an impassable tile, stop movement in that direction
                if (!_tileMap.IsTilePassable(nextTile.X, nextTile.Y))
                {
                    // Try horizontal movement only
                    Vector2 horizontalMove = new Vector2(velocity.Velocity.X, 0) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Point horizontalTile = _tileMap.WorldToTile(transform.Position + horizontalMove);
                    
                    if (!_tileMap.IsTilePassable(horizontalTile.X, horizontalTile.Y))
                    {
                        velocity.Velocity = new Vector2(0, velocity.Velocity.Y);
                    }
                    
                    // Try vertical movement only
                    Vector2 verticalMove = new Vector2(0, velocity.Velocity.Y) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Point verticalTile = _tileMap.WorldToTile(transform.Position + verticalMove);
                    
                    if (!_tileMap.IsTilePassable(verticalTile.X, verticalTile.Y))
                    {
                        velocity.Velocity = new Vector2(velocity.Velocity.X, 0);
                    }
                }
                
                // Prevent player from going outside world boundaries
                Rectangle bounds = _worldBounds;
                Vector2 nextPos = transform.Position + velocity.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Calculate player boundaries considering size
                float playerHalfWidth = _playerEntity.GetComponent<ColliderComponent>().Size.X / 2f;
                float playerHalfHeight = _playerEntity.GetComponent<ColliderComponent>().Size.Y / 2f;
                
                // Create boundary rectangle with padding for player size
                Rectangle playerBounds = new Rectangle(
                    bounds.X + (int)playerHalfWidth,
                    bounds.Y + (int)playerHalfHeight,
                    bounds.Width - (int)playerHalfWidth * 2,
                    bounds.Height - (int)playerHalfHeight * 2);
                
                // Clamp player position to boundaries
                if (nextPos.X < playerBounds.Left)
                {
                    nextPos.X = playerBounds.Left;
                    velocity.Velocity = new Vector2(0, velocity.Velocity.Y);
                }
                else if (nextPos.X > playerBounds.Right)
                {
                    nextPos.X = playerBounds.Right;
                    velocity.Velocity = new Vector2(0, velocity.Velocity.Y);
                }
                
                if (nextPos.Y < playerBounds.Top)
                {
                    nextPos.Y = playerBounds.Top;
                    velocity.Velocity = new Vector2(velocity.Velocity.X, 0);
                }
                else if (nextPos.Y > playerBounds.Bottom)
                {
                    nextPos.Y = playerBounds.Bottom;
                    velocity.Velocity = new Vector2(velocity.Velocity.X, 0);
                }
                
                // Set position directly if it needed to be clamped
                if (nextPos != transform.Position + velocity.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds)
                {
                    transform.Position = nextPos;
                }
                
                // Update camera to follow player
                _renderSystem.Camera.FollowTarget(transform.Position, 5f);
                
                // Clamp camera to world bounds
                _renderSystem.Camera.ClampToBounds(_worldBounds);
            }
            
            // Handle camera zoom with keyboard controls (for testing)
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) || Keyboard.GetState().IsKeyDown(Keys.Add))
            {
                _renderSystem.Camera.ZoomBy(0.02f);
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) || Keyboard.GetState().IsKeyDown(Keys.Subtract))
            {
                _renderSystem.Camera.ZoomBy(-0.02f);
            }
            
            // Update time manager
            _gameTimeManager.Update(gameTime);
            
            // Time control keys for testing
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                // Fast forward time (5x speed)
                _gameTimeManager.Update(new GameTime(gameTime.TotalGameTime, TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds * 4)));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                // Reset time to 8:00 AM
                _gameTimeManager.SetTime(8, 0);
            }
            
            // Update UI
            _inventoryUI.Update();
            
            // Update entity manager (and all systems)
            _entityManager.Update(gameTime);
        }
        
        /// <summary>
        /// Draws this state.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Define the view rectangle based on camera position and viewport
            Rectangle viewRect = new Rectangle(
                (int)(_renderSystem.Camera.Position.X - GraphicsDevice.Viewport.Width / (2f * _renderSystem.Camera.Zoom)),
                (int)(_renderSystem.Camera.Position.Y - GraphicsDevice.Viewport.Height / (2f * _renderSystem.Camera.Zoom)),
                (int)(GraphicsDevice.Viewport.Width / _renderSystem.Camera.Zoom),
                (int)(GraphicsDevice.Viewport.Height / _renderSystem.Camera.Zoom));
            
            // Draw the tile map
            _spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointClamp, 
                null, 
                null, 
                null, 
                _renderSystem.Camera.TransformMatrix);
            
            _tileMap.Draw(_spriteBatch, viewRect);
            
            _spriteBatch.End();
            
            // Draw entities using the render system
            _renderSystem.Update(gameTime);
            
            // Apply day/night lighting overlay
            Color ambientColor = _gameTimeManager.AmbientLightColor;
            float alpha = 1.0f - _gameTimeManager.SunIntensity * 0.8f; // Allow some visibility at night
            Color overlayColor = new Color(
                (byte)(10), // Dark blue for night
                (byte)(10),
                (byte)(35),
                (byte)(alpha * 180)); // Semi-transparent
            
            // Draw lighting overlay if it's not full daylight
            if (alpha > 0.05f)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(
                    _lightOverlayTexture,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    overlayColor);
                _spriteBatch.End();
            }
            
            // Draw inventory UI
            _inventoryUI.Draw();
            
            // Draw debug information
            if (_debugFont != null)
            {
                List<string> debugInfo = new List<string>
                {
                    $"Time: {_gameTimeManager.GetTimeString()} ({_gameTimeManager.CurrentTimeOfDay})",
                    $"Day: {_gameTimeManager.CurrentDay}",
                    $"Sun Intensity: {_gameTimeManager.SunIntensity:F2}",
                    $"Camera Position: {_renderSystem.Camera.Position.X:F0}, {_renderSystem.Camera.Position.Y:F0}",
                    $"Camera Zoom: {_renderSystem.Camera.Zoom:F2}",
                    $"Player Position: {_playerEntity.GetComponent<TransformComponent>().Position.X:F0}, {_playerEntity.GetComponent<TransformComponent>().Position.Y:F0}",
                    $"Player Tile: {_tileMap.WorldToTile(_playerEntity.GetComponent<TransformComponent>().Position)}",
                    $"Map Size: {_tileMap.Width}x{_tileMap.Height} tiles ({_tileMap.PixelWidth}x{_tileMap.PixelHeight} pixels)",
                    $"Controls: E=Toggle Inventory, 1-9=Select Hotbar",
                    $"Controls: Click=Use/Gather, T=Fast time, +/- = Zoom"
                };
                
                _uiManager.DrawDebugPanel(debugInfo, new Vector2(10, 10));            
            }
        }
        
        /// <summary>
        /// Handles collision between two entities.
        /// </summary>
        /// <param name="entityA">The first entity.</param>
        /// <param name="entityB">The second entity.</param>
        private void HandleCollision(Entity entityA, Entity entityB)
        {
            // Simple collision response for player entity
            if (entityA == _playerEntity || entityB == _playerEntity)
            {
                var playerEntity = entityA == _playerEntity ? entityA : entityB;
                var otherEntity = entityA == _playerEntity ? entityB : entityA;
                
                var playerTransform = playerEntity.GetComponent<TransformComponent>();
                var playerVelocity = playerEntity.GetComponent<VelocityComponent>();
                var otherTransform = otherEntity.GetComponent<TransformComponent>();
                
                // Calculate push direction (away from obstacle)
                Vector2 pushDirection = playerTransform.Position - otherTransform.Position;
                if (pushDirection != Vector2.Zero)
                {
                    pushDirection.Normalize();
                    
                    // Push player away from obstacle
                    playerTransform.Position += pushDirection * 3f;
                    
                    // Stop velocity in collision direction
                    float dotProduct = Vector2.Dot(playerVelocity.Velocity, pushDirection);
                    if (dotProduct < 0)
                    {
                        playerVelocity.Velocity -= pushDirection * dotProduct;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles trigger enter events.
        /// </summary>
        /// <param name="entityA">The first entity.</param>
        /// <param name="entityB">The second entity.</param>
        private void HandleTriggerEnter(Entity entityA, Entity entityB)
        {
            // Handle trigger interactions here
        }
        
        /// <summary>
        /// Handles trigger exit events.
        /// </summary>
        /// <param name="entityA">The first entity.</param>
        /// <param name="entityB">The second entity.</param>
        private void HandleTriggerExit(Entity entityA, Entity entityB)
        {
            // Handle trigger exit here
        }

        /// <summary>
        /// Populates the world with environmental objects like trees and rocks.
        /// </summary>
        private void PopulateWorldWithEnvironmentalObjects()
        {
            // Use a deterministic seed for consistent results during development
            Random random = new Random(12345);
            
            // Calculate the safe area around the player spawn (center of map)
            Vector2 mapCenter = new Vector2(_worldBounds.Width / 2f, _worldBounds.Height / 2f);
            Rectangle safeArea = new Rectangle(
                (int)mapCenter.X - 150,
                (int)mapCenter.Y - 150,
                300,
                300);
            
            // Get the map bounds
            int mapWidth = _tileMap.PixelWidth;
            int mapHeight = _tileMap.PixelHeight;
            
            // Add trees (much less dense - about 1 tree per 15000 square pixels)
            int numTrees = (mapWidth * mapHeight) / 15000;
            Console.WriteLine($"Creating {numTrees} trees");
            
            // Create a grid to ensure better distribution and prevent clustering
            int gridSize = 150; // Minimum distance between trees
            bool[,] occupiedCells = new bool[(mapWidth / gridSize) + 1, (mapHeight / gridSize) + 1];
            
            int treesCreated = 0;
            int maxAttempts = numTrees * 3; // Limit the number of attempts to prevent infinite loops
            int attempts = 0;
            
            while (treesCreated < numTrees && attempts < maxAttempts)
            {
                attempts++;
                
                // Pick a random position
                int x = random.Next(50, mapWidth - 50);
                int y = random.Next(50, mapHeight - 50);
                Vector2 position = new Vector2(x, y);
                
                // Check if this grid cell is already occupied
                int gridX = x / gridSize;
                int gridY = y / gridSize;
                
                if (occupiedCells[gridX, gridY])
                    continue; // Cell already has a tree, try another position
                
                // Skip if in safe area
                if (safeArea.Contains(position))
                {
                    continue;
                }
                
                // Skip if not on a passable tile or if it's water
                Point tilePos = _tileMap.WorldToTile(position);
                Tile tile = _tileMap.GetTile(tilePos.X, tilePos.Y);
                if (tile == null || !tile.IsPassable || tile.IsWater)
                {
                    continue;
                }
                
                // Random growth level (70-100%)
                int growth = random.Next(70, 101);
                
                // Create tree with random growth level
                EnvironmentalObjectFactory.CreateTree(_entityManager, GraphicsDevice, position, growth);
                
                // Mark this grid cell as occupied
                occupiedCells[gridX, gridY] = true;
                treesCreated++;
            }
            
            // Add rocks (much less dense - about 1 rock per 30000 square pixels)
            int numRocks = (mapWidth * mapHeight) / 30000;
            Console.WriteLine($"Creating {numRocks} rocks");
            
            // Reset grid for rocks
            occupiedCells = new bool[(mapWidth / gridSize) + 1, (mapHeight / gridSize) + 1];
            
            int rocksCreated = 0;
            attempts = 0;
            maxAttempts = numRocks * 3;
            
            while (rocksCreated < numRocks && attempts < maxAttempts)
            {
                attempts++;
                
                // Pick a random position
                int x = random.Next(50, mapWidth - 50);
                int y = random.Next(50, mapHeight - 50);
                Vector2 position = new Vector2(x, y);
                
                // Check if this grid cell is already occupied
                int gridX = x / gridSize;
                int gridY = y / gridSize;
                
                if (occupiedCells[gridX, gridY])
                    continue; // Cell already has a rock, try another position
                
                // Skip if in safe area
                if (safeArea.Contains(position))
                {
                    continue;
                }
                
                // Skip if not on a passable tile or if it's water
                Point tilePos = _tileMap.WorldToTile(position);
                Tile tile = _tileMap.GetTile(tilePos.X, tilePos.Y);
                if (tile == null || !tile.IsPassable || tile.IsWater)
                {
                    continue;
                }
                
                // Random size (80-100%)
                int size = random.Next(80, 101);
                
                // Create rock with random size
                EnvironmentalObjectFactory.CreateRock(_entityManager, GraphicsDevice, position, size);
                
                // Mark this grid cell as occupied
                occupiedCells[gridX, gridY] = true;
                rocksCreated++;
            }
            
            // Add bushes (much less dense - about 1 bush per 20000 square pixels)
            int numBushes = (mapWidth * mapHeight) / 20000;
            Console.WriteLine($"Creating {numBushes} bushes");
            
            // Reset grid for bushes
            occupiedCells = new bool[(mapWidth / gridSize) + 1, (mapHeight / gridSize) + 1];
            
            int bushesCreated = 0;
            attempts = 0;
            maxAttempts = numBushes * 3;
            
            while (bushesCreated < numBushes && attempts < maxAttempts)
            {
                attempts++;
                
                // Pick a random position
                int x = random.Next(50, mapWidth - 50);
                int y = random.Next(50, mapHeight - 50);
                Vector2 position = new Vector2(x, y);
                
                // Check if this grid cell is already occupied
                int gridX = x / gridSize;
                int gridY = y / gridSize;
                
                if (occupiedCells[gridX, gridY])
                    continue; // Cell already has a bush, try another position
                
                // Skip if in safe area
                if (safeArea.Contains(position))
                {
                    continue;
                }
                
                // Skip if not on a passable tile or if it's water
                Point tilePos = _tileMap.WorldToTile(position);
                Tile tile = _tileMap.GetTile(tilePos.X, tilePos.Y);
                if (tile == null || !tile.IsPassable || tile.IsWater)
                {
                    continue;
                }
                
                // Random growth level (60-100%)
                int growth = random.Next(60, 101);
                
                // Create bush with random growth level
                EnvironmentalObjectFactory.CreateBush(_entityManager, GraphicsDevice, position, growth);
                
                // Mark this grid cell as occupied
                occupiedCells[gridX, gridY] = true;
                bushesCreated++;
            }
        }
        
        /// <summary>
        /// Handles resource gathering events.
        /// </summary>
        private void OnResourceGathered(object sender, ResourceGatheredEventArgs e)
        {
            // Add some feedback when resources are gathered (for future implementation)
            Console.WriteLine($"Gathered {e.Amount} {e.Resource.Name}");
            
            // In the future, we could add particle effects, sounds, etc.
        }
    }
}
