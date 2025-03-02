using Microsoft.Xna.Framework;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Systems;

namespace MyIslandGame.States
{
    public class PlayingState : State
    {
        private readonly TimeManager _timeManager; // Add TimeManager field
        private readonly RenderSystem _renderSystem;

        public PlayingState(Game1 game, GameStateManager stateManager, TimeManager timeManager) : base(game, stateManager)
        {
            _timeManager = timeManager; // Initialize TimeManager
            var entityManager = new EntityManager();
            _renderSystem = new RenderSystem(entityManager, game.SpriteBatch, game.GraphicsDevice, _timeManager);
        }

        public override void Update(GameTime gameTime)
        {
            _timeManager.Update(gameTime); // Update TimeManager
            _renderSystem.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _renderSystem.Draw(gameTime);
        }
    }
}
