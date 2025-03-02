using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.ECS.Components;
using MyIslandGame.Rendering;

namespace MyIslandGame.ECS.Systems
{
    /// <summary>
    /// System that handles rendering entities with sprite components.
    /// </summary>
    public class RenderSystem : System
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private Camera _camera;
        private TimeManager _timeManager; // Add TimeManager field
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="spriteBatch">The sprite batch to use for rendering.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <summary>
        /// Gets the camera used by this render system.
        /// </summary>
        public Camera Camera => _camera;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="spriteBatch">The sprite batch to use for rendering.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public RenderSystem(EntityManager entityManager, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
            : base(entityManager)
        {
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _camera = new Camera(_graphicsDevice.Viewport);
            _timeManager = /* get reference to time manager */; // Initialize TimeManager
        }
        
        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Get all entities with both transform and sprite components
            var renderableEntities = GetInterestingEntities().ToList();
            
            // Sort entities by their layer (depth)
            renderableEntities.Sort((a, b) =>
            {
                var transformA = a.GetComponent<TransformComponent>();
                var transformB = b.GetComponent<TransformComponent>();
                return transformA.Layer.CompareTo(transformB.Layer);
            });
            
            // Begin sprite batch with transformation matrix from camera
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                _camera.TransformMatrix);
            
            // Draw each entity
            foreach (var entity in renderableEntities)
            {
                var transform = entity.GetComponent<TransformComponent>();
                var sprite = entity.GetComponent<SpriteComponent>();
                
                // Skip invisible sprites
                if (!sprite.Visible || sprite.Texture == null)
                {
                    continue;
                }
                
                // Draw the sprite with the ambient light color applied
                _spriteBatch.Draw(
                    sprite.Texture,
                    transform.Position,
                    sprite.SourceRectangle,
                    _timeManager.AmbientLightColor, // Apply ambient light color here
                    transform.Rotation,
                    sprite.Origin,
                    transform.Scale,
                    sprite.Effects,
                    0f);
            }
            
            // End sprite batch
            _spriteBatch.End();
        }
        
        /// <summary>
        /// Determines whether this system is interested in the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the system is interested, otherwise false.</returns>
        public override bool IsInterestedIn(Entity entity)
        {
            return entity.HasComponent<TransformComponent>() && entity.HasComponent<SpriteComponent>();
        }
    }
}
