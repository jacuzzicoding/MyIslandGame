using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.States
{
    /// <summary>
    /// Base class for all game states such as main menu, playing, paused, etc.
    /// </summary>
    public abstract class GameState
    {
        /// <summary>
        /// Gets the game instance.
        /// </summary>
        protected Game Game { get; }
        
        /// <summary>
        /// Gets the game state manager.
        /// </summary>
        protected GameStateManager StateManager { get; }
        
        /// <summary>
        /// Gets the content manager for loading state-specific resources.
        /// </summary>
        protected ContentManager Content { get; }
        
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        protected GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
        
        /// <summary>
        /// Gets or sets a value indicating whether this state is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="stateManager">The game state manager.</param>
        protected GameState(Game game, GameStateManager stateManager)
        {
            Game = game;
            StateManager = stateManager;
            Content = new ContentManager(game.Services, "Content");
        }
        
        /// <summary>
        /// Initializes this state.
        /// </summary>
        public virtual void Initialize() { }
        
        /// <summary>
        /// Loads content for this state.
        /// </summary>
        public virtual void LoadContent() { }
        
        /// <summary>
        /// Unloads content for this state.
        /// </summary>
        public virtual void UnloadContent()
        {
            Content.Unload();
        }
        
        /// <summary>
        /// Called when this state becomes the active state.
        /// </summary>
        public virtual void OnEnter() { }
        
        /// <summary>
        /// Called when this state is no longer the active state.
        /// </summary>
        public virtual void OnExit() { }
        
        /// <summary>
        /// Updates this state.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public virtual void Update(GameTime gameTime) { }
        
        /// <summary>
        /// Draws this state.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
    }
}
