using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.ECS.Systems;
using MyIslandGame.Input;

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
            
            // Draw grass tiles
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            for (int x = 0; x < GraphicsDevice.Viewport.Width; x += 64)
            {
                for (int y = 0; y < GraphicsDevice.Viewport.Height; y += 64)
                {
                    _spriteBatch.Draw(_grassTexture, new Vector2(x, y), Color.White);
                }
            }
            
            _spriteBatch.End();
            
            // Draw entities using the render system
            _renderSystem.Update(gameTime);
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
