using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyIslandGame.ECS.Components;

namespace MyIslandGame.ECS.Systems
{
    /// <summary>
    /// System that handles collision detection between entities.
    /// </summary>
    public class CollisionSystem : System
    {
        /// <summary>
        /// Event raised when two entities collide.
        /// </summary>
        public event Action<Entity, Entity> CollisionOccurred;
        
        /// <summary>
        /// Event raised when two entities with at least one trigger collider collide.
        /// </summary>
        public event Action<Entity, Entity> TriggerEntered;
        
        /// <summary>
        /// Event raised when two entities with at least one trigger collider stop colliding.
        /// </summary>
        public event Action<Entity, Entity> TriggerExited;
        
        private readonly HashSet<(Guid, Guid)> _currentCollisions = new();
        private readonly HashSet<(Guid, Guid)> _previousCollisions = new();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CollisionSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        public CollisionSystem(EntityManager entityManager)
            : base(entityManager)
        {
        }
        
        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Swap collision sets
            var temp = _previousCollisions;
            _previousCollisions.Clear();
            foreach (var collision in _currentCollisions)
            {
                _previousCollisions.Add(collision);
            }
            _currentCollisions.Clear();
            
            // Get all entities with colliders
            var entities = new List<Entity>();
            foreach (var entity in GetInterestingEntities())
            {
                entities.Add(entity);
            }
            
            // Check for collisions between all pairs of entities
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = i + 1; j < entities.Count; j++)
                {
                    CheckCollision(entities[i], entities[j]);
                }
            }
            
            // Check for entities that are no longer colliding
            foreach (var collision in _previousCollisions)
            {
                if (!_currentCollisions.Contains(collision))
                {
                    // Collision ended
                    var entityA = EntityManager.GetEntityById(collision.Item1);
                    var entityB = EntityManager.GetEntityById(collision.Item2);
                    
                    if (entityA != null && entityB != null)
                    {
                        var colliderA = entityA.GetComponent<ColliderComponent>();
                        var colliderB = entityB.GetComponent<ColliderComponent>();
                        
                        if (colliderA.IsTrigger || colliderB.IsTrigger)
                        {
                            TriggerExited?.Invoke(entityA, entityB);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks for collision between two entities.
        /// </summary>
        /// <param name="entityA">The first entity.</param>
        /// <param name="entityB">The second entity.</param>
        private void CheckCollision(Entity entityA, Entity entityB)
        {
            var colliderA = entityA.GetComponent<ColliderComponent>();
            var colliderB = entityB.GetComponent<ColliderComponent>();
            
            if (!colliderA.Enabled || !colliderB.Enabled)
            {
                return;
            }
            
            bool colliding = false;
            
            // Check collision based on collider types
            if (colliderA.Type == ColliderType.Rectangle && colliderB.Type == ColliderType.Rectangle)
            {
                // Rectangle-Rectangle collision
                var rectA = colliderA.GetBoundingRectangle();
                var rectB = colliderB.GetBoundingRectangle();
                colliding = rectA.Intersects(rectB);
            }
            else if (colliderA.Type == ColliderType.Circle && colliderB.Type == ColliderType.Circle)
            {
                // Circle-Circle collision
                var transformA = entityA.GetComponent<TransformComponent>();
                var transformB = entityB.GetComponent<TransformComponent>();
                
                float radiusA = colliderA.GetRadius();
                float radiusB = colliderB.GetRadius();
                
                Vector2 centerA = transformA.Position + colliderA.Offset;
                Vector2 centerB = transformB.Position + colliderB.Offset;
                
                float distance = Vector2.Distance(centerA, centerB);
                colliding = distance < (radiusA + radiusB);
            }
            else
            {
                // Rectangle-Circle collision (simplified)
                ColliderComponent rectCollider, circleCollider;
                Entity rectEntity, circleEntity;
                
                if (colliderA.Type == ColliderType.Rectangle)
                {
                    rectCollider = colliderA;
                    circleCollider = colliderB;
                    rectEntity = entityA;
                    circleEntity = entityB;
                }
                else
                {
                    rectCollider = colliderB;
                    circleCollider = colliderA;
                    rectEntity = entityB;
                    circleEntity = entityA;
                }
                
                var rect = rectCollider.GetBoundingRectangle();
                var transformCircle = circleEntity.GetComponent<TransformComponent>();
                
                Vector2 circleCenter = transformCircle.Position + circleCollider.Offset;
                float radius = circleCollider.GetRadius();
                
                // Find closest point on rectangle to circle center
                float closestX = MathHelper.Clamp(circleCenter.X, rect.Left, rect.Right);
                float closestY = MathHelper.Clamp(circleCenter.Y, rect.Top, rect.Bottom);
                
                // Calculate distance from closest point to circle center
                float distance = Vector2.Distance(new Vector2(closestX, closestY), circleCenter);
                
                colliding = distance < radius;
            }
            
            if (colliding)
            {
                // Create collision pair (always ordered by ID to avoid duplicates)
                var pair = entityA.Id.CompareTo(entityB.Id) < 0
                    ? (entityA.Id, entityB.Id)
                    : (entityB.Id, entityA.Id);
                
                _currentCollisions.Add(pair);
                
                bool isNewCollision = !_previousCollisions.Contains(pair);
                
                // Handle collision based on collider types
                if (colliderA.IsTrigger || colliderB.IsTrigger)
                {
                    // Trigger collision
                    if (isNewCollision)
                    {
                        TriggerEntered?.Invoke(entityA, entityB);
                    }
                }
                else
                {
                    // Physical collision
                    CollisionOccurred?.Invoke(entityA, entityB);
                    
                    // In a more advanced system, we would apply collision resolution here
                }
            }
        }
        
        /// <summary>
        /// Determines whether this system is interested in the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the system is interested, otherwise false.</returns>
        public override bool IsInterestedIn(Entity entity)
        {
            return entity.HasComponent<TransformComponent>() && entity.HasComponent<ColliderComponent>();
        }
    }
}
