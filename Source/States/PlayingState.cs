using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.ECS;
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
        
        // Temporary variables for prototype
        private Entity _playerEntity;
        private Vector2 _playerPosition;
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
            
            // Initialize player position
            _playerPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        }
        
        /// <summary>
        /// Loads content for this state.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create a simple colored rectangle texture for the player
            _playerTexture = new Texture2D(GraphicsDevice, 32, 32);
            Color[] colorData = new Color[32 * 32];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = Color.Blue;
            }
            _playerTexture.SetData(colorData);
            
            // Create a simple colored rectangle texture for the grass
            _grassTexture = new Texture2D(GraphicsDevice, 64, 64);
            Color[] grassColorData = new Color[64 * 64];
            for (int i = 0; i < grassColorData.Length; i++)
            {
                grassColorData[i] = Color.Green;
            }
            _grassTexture.SetData(grassColorData);
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
            
            // Simple player movement
            float moveSpeed = 200f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;
            
            if (_inputManager.IsActionActive("MoveUp"))
            {
                movement.Y -= 1;
            }
            
            if (_inputManager.IsActionActive("MoveDown"))
            {
                movement.Y += 1;
            }
            
            if (_inputManager.IsActionActive("MoveLeft"))
            {
                movement.X -= 1;
            }
            
            if (_inputManager.IsActionActive("MoveRight"))
            {
                movement.X += 1;
            }
            
            // Normalize the movement vector if moving diagonally
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
            }
            
            _playerPosition += movement * moveSpeed;
            
            // Keep player within bounds
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, 0, GraphicsDevice.Viewport.Width - _playerTexture.Width);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, 0, GraphicsDevice.Viewport.Height - _playerTexture.Height);
            
            // Update entity manager
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
            
            // Draw the world
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            // Draw grass tiles
            for (int x = 0; x < GraphicsDevice.Viewport.Width; x += 64)
            {
                for (int y = 0; y < GraphicsDevice.Viewport.Height; y += 64)
                {
                    _spriteBatch.Draw(_grassTexture, new Vector2(x, y), Color.White);
                }
            }
            
            // Draw player
            _spriteBatch.Draw(_playerTexture, _playerPosition, Color.White);
            
            _spriteBatch.End();
        }
    }
}
