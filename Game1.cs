using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.States;

namespace MyIslandGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameStateManager _stateManager;
        private TimeManager _timeManager; // Add TimeManager field

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // Set window size (for development)
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            // Create the state manager
            _stateManager = new GameStateManager(this);
            
            // Initialize TimeManager
            _timeManager = new TimeManager();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create and add game states
            var playingState = new PlayingState(this, _stateManager, _timeManager); // Pass TimeManager to PlayingState
            _stateManager.AddState<PlayingState>(playingState);
            
            // Set the initial state
            _stateManager.ChangeState<PlayingState>();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update the current state
            _stateManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draw the current state
            _stateManager.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }
    }
}
