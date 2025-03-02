using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.Core;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.ECS.Systems;
using MyIslandGame.Input;
using MyIslandGame.Rendering;
using MyIslandGame.UI;

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
        
        private Entity _playerEntity;
        private Texture2D _playerTexture;
        private Texture2D _grassTexture;
        
        // World bounds for camera clamping
        private Rectangle _worldBounds;
        
        // Time and UI management
        private TimeManager _timeManager;
        private UIManager _uiManager;
        private SpriteFont _debugFont;
        private Texture2D _lightOverlayTexture;
        
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
            _timeManager = new TimeManager(1440f, 72f, 480f); // Start at 8:00 AM
            
            // Initialize UI manager
            _uiManager = new UIManager(game.GraphicsDevice);
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
            
            // Add systems to entity manager
            _entityManager.AddSystem(_movementSystem);
            _entityManager.AddSystem(_collisionSystem);
            
            // Set up collision handlers
            _collisionSystem.CollisionOccurred += HandleCollision;
            _collisionSystem.TriggerEntered += HandleTriggerEnter;
            _collisionSystem.TriggerExited += HandleTriggerExit;
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
            
            var playerTransform = new TransformComponent(
                new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2));
            
            var playerSprite = new SpriteComponent(_playerTexture)
            {
                Origin = new Vector2(_playerTexture.Width / 2f, _playerTexture.Height / 2f)
            };
            
            var playerVelocity = new VelocityComponent(200f, 0.1f);
            
            var playerCollider = new ColliderComponent(
                new Vector2(_playerTexture.Width, _playerTexture.Height),
                Vector2.Zero,
                ColliderType.Rectangle);
            
            _playerEntity.AddComponent(playerTransform);
            _playerEntity.AddComponent(playerSprite);
            _playerEntity.AddComponent(playerVelocity);
            _playerEntity.AddComponent(playerCollider);
            
            // Create some obstacle entities for testing collision
            CreateObstacle(new Vector2(500, 300), new Vector2(50, 50), Color.Red);
            CreateObstacle(new Vector2(300, 500), new Vector2(50, 100), Color.Yellow);
            CreateObstacle(new Vector2(700, 400), new Vector2(100, 50), Color.Orange);
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
            _timeManager.Update(gameTime);
            
            // Time control keys for testing
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                // Fast forward time (5x speed)
                _timeManager.Update(new GameTime(gameTime.TotalGameTime, TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds * 4)));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                // Reset time to 8:00 AM
                _timeManager.SetTime(8, 0);
            }
            
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
            
            // Define world size (for now, will be replaced with proper world generation)
            int worldWidth = 1600;
            int worldHeight = 1200;
            _worldBounds = new Rectangle(-200, -200, worldWidth, worldHeight);
            
            // Draw grass tiles
            _spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointClamp, 
                null, 
                null, 
                null, 
                _renderSystem.Camera.TransformMatrix);
            
            // Draw grass tiles for the entire world
            int tileSize = 64;
            for (int x = _worldBounds.Left; x < _worldBounds.Right; x += tileSize)
            {
                for (int y = _worldBounds.Top; y < _worldBounds.Bottom; y += tileSize)
                {
                    _spriteBatch.Draw(_grassTexture, new Vector2(x, y), Color.White);
                }
            }
            
            _spriteBatch.End();
            
            // Draw entities using the render system
            _renderSystem.Update(gameTime);
            
            // Apply day/night lighting overlay
            Color ambientColor = _timeManager.AmbientLightColor;
            float alpha = 1.0f - _timeManager.SunIntensity * 0.8f; // Allow some visibility at night
            Color overlayColor = new Color(
                (byte)(255 - ambientColor.R * (1 - alpha)),
                (byte)(255 - ambientColor.G * (1 - alpha)),
                (byte)(255 - ambientColor.B * (1 - alpha)),
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
            
            // Draw debug information
            if (_debugFont != null)
            {
                List<string> debugInfo = new List<string>
                {
                    $"Time: {_timeManager.GetTimeString()} ({_timeManager.CurrentTimeOfDay})",
                    $"Day: {_timeManager.CurrentDay}",
                    $"Sun Intensity: {_timeManager.SunIntensity:F2}",
                    $"Camera Position: {_renderSystem.Camera.Position.X:F0}, {_renderSystem.Camera.Position.Y:F0}",
                    $"Camera Zoom: {_renderSystem.Camera.Zoom:F2}",
                    $"Player Position: {_playerEntity.GetComponent<TransformComponent>().Position.X:F0}, {_playerEntity.GetComponent<TransformComponent>().Position.Y:F0}",
                    $"Controls: T=Fast time, R=Reset to 8:00 AM",
                    $"Controls: +/- = Zoom, WASD = Move"
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
    }
}
