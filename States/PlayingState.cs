using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Systems;
using MyIslandGame.Core;

namespace MyIslandGame.States
{
    public class PlayingState : State
    {
        private readonly TimeManager _timeManager; // Add TimeManager field
        private readonly RenderSystem _renderSystem;

        public PlayingState(Game1 game, GameStateManager stateManager, TimeManager timeManager) : base(game, stateManager)
            var entityManager = new EntityManager();
            _renderSystem = new RenderSystem(entityManager, spriteBatch, game.GraphicsDevice, _timeManager);
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
