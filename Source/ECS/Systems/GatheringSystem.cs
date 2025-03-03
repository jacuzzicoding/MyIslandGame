using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyIslandGame.Core.Resources;
using MyIslandGame.ECS.Components;
using MyIslandGame.Input;

namespace MyIslandGame.ECS.Systems
{
    /// <summary>
    /// System for gathering resources from environmental objects.
    /// </summary>
    public class GatheringSystem : System
    {
        private readonly InputManager _inputManager;
        private readonly ResourceManager _resourceManager;
        private readonly Random _random = new Random();
        
        // Range for gathering interactions
        private const float GatheringRange = 100f;
        
        // Cooldown between gathering actions
        private readonly TimeSpan _gatheringCooldown = TimeSpan.FromSeconds(0.5);
        private TimeSpan _lastGatheringTime = TimeSpan.Zero;
        
        // Feedback
        public event EventHandler<ResourceGatheredEventArgs> ResourceGathered;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatheringSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="inputManager">The input manager.</param>
        /// <param name="resourceManager">The resource manager.</param>
        public GatheringSystem(EntityManager entityManager, InputManager inputManager, ResourceManager resourceManager)
            : base(entityManager)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <summary>
        /// Initializes this system.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            // Ensure gathering input actions are registered
            _inputManager.RegisterAction("Gather", new InputAction(MouseButton.Left));
        }

        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Check if player is trying to gather
            if (_inputManager.IsActionActive("Gather"))
            {
                // Check cooldown
                if (gameTime.TotalGameTime - _lastGatheringTime >= _gatheringCooldown)
                {
                    // Find player entity (assuming single player game)
                    var playerEntities = EntityManager.GetEntitiesWithComponents(typeof(PlayerComponent), typeof(TransformComponent), typeof(InventoryComponent));
                    var playerEntity = playerEntities.GetEnumerator();
                    
                    if (playerEntity.MoveNext())
                    {
                        var player = playerEntity.Current;
                        
                        // Try to gather from nearby resource
                        if (TryGatherResource(player, gameTime))
                        {
                            _lastGatheringTime = gameTime.TotalGameTime;
                        }
                    }
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
            // This system doesn't operate on specific entities, as it processes player interaction with environment
            return false;
        }

        /// <summary>
        /// Tries to gather resources from a nearby environmental object.
        /// </summary>
        /// <param name="playerEntity">The player entity.</param>
        /// <param name="gameTime">The current game time.</param>
        /// <returns>True if gathering was successful, otherwise false.</returns>
        private bool TryGatherResource(Entity playerEntity, GameTime gameTime)
        {
            // Find nearest gatherable entity
            var target = FindNearestGatherableEntity(playerEntity);
            
            if (target == null)
            {
                return false;
            }
            
            // Get the selected tool type
            var inventoryComponent = playerEntity.GetComponent<InventoryComponent>();
            string toolType = inventoryComponent.GetSelectedToolType();
            
            // Gather from environmental object
            return GatherFromEnvironmentalObject(playerEntity, target, toolType);
        }

        /// <summary>
        /// Finds the nearest environmental object that can be gathered from.
        /// </summary>
        /// <param name="playerEntity">The player entity.</param>
        /// <returns>The nearest gatherable entity, or null if none are in range.</returns>
        private Entity FindNearestGatherableEntity(Entity playerEntity)
        {
            var playerTransform = playerEntity.GetComponent<TransformComponent>();
            var playerPosition = playerTransform.Position;
            
            // Get all environmental objects
            var environmentalEntities = EntityManager.GetEntitiesWithComponents(
                typeof(EnvironmentalObjectComponent),
                typeof(TransformComponent)
            );
            
            Entity closestEntity = null;
            float closestDistance = GatheringRange;
            
            foreach (var entity in environmentalEntities)
            {
                var environmentalComponent = entity.GetComponent<EnvironmentalObjectComponent>();
                
                // Skip objects that can't be harvested
                if (!environmentalComponent.IsHarvestable)
                {
                    continue;
                }
                
                var entityTransform = entity.GetComponent<TransformComponent>();
                float distance = Vector2.Distance(playerPosition, entityTransform.Position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEntity = entity;
                }
            }
            
            return closestEntity;
        }

        /// <summary>
        /// Gathers resources from an environmental object.
        /// </summary>
        /// <param name="playerEntity">The player entity.</param>
        /// <param name="targetEntity">The target environmental object entity.</param>
        /// <param name="toolType">The type of tool being used.</param>
        /// <returns>True if gathering was successful, otherwise false.</returns>
        private bool GatherFromEnvironmentalObject(Entity playerEntity, Entity targetEntity, string toolType)
        {
            var environmentalComponent = targetEntity.GetComponent<EnvironmentalObjectComponent>();
            
            // Check if the object can be harvested with the given tool
            if (!environmentalComponent.CanHarvest(toolType))
            {
                return false;
            }
            
            // Calculate damage based on tool
            int damage = CalculateToolDamage(toolType, environmentalComponent.ObjectType);
            
            // Harvest the resource
            var harvested = environmentalComponent.Harvest(damage, toolType);
            
            if (harvested.Count > 0)
            {
                var inventory = playerEntity.GetComponent<InventoryComponent>().Inventory;
                int totalResourcesGathered = 0;
                
                // Add each resource to inventory
                foreach (var yield in harvested)
                {
                    // Skip if resource doesn't exist
                    if (!_resourceManager.TryGetResource(yield.ResourceId, out var resource))
                    {
                        continue;
                    }
                    
                    // Calculate actual amount with randomness
                    int amount = yield.GetAmount(_random);
                    totalResourcesGathered += amount;
                    
                    // Add to inventory
                    inventory.TryAddResource(resource, amount);
                    
                    // Raise event for feedback
                    OnResourceGathered(resource, amount, targetEntity);
                }
                
                // Use tool durability if applicable
                if (toolType != null && playerEntity.GetComponent<InventoryComponent>().GetSelectedItem() is Inventory.Tool tool)
                {
                    tool.UseTool();
                }
                
                return totalResourcesGathered > 0;
            }
            
            return false;
        }

        /// <summary>
        /// Calculates the damage amount based on the tool type and object type.
        /// </summary>
        /// <param name="toolType">The type of tool being used.</param>
        /// <param name="objectType">The type of environmental object.</param>
        /// <returns>The damage amount.</returns>
        private int CalculateToolDamage(string toolType, EnvironmentalObjectType objectType)
        {
            // Base damage for hand (no tool)
            int damage = 5;
            
            // Adjust damage based on tool type and object type
            if (toolType != null)
            {
                switch (objectType)
                {
                    case EnvironmentalObjectType.Tree:
                        damage = toolType == "axe" ? 25 : 5;
                        break;
                        
                    case EnvironmentalObjectType.Rock:
                        damage = toolType == "pickaxe" ? 20 : 1;
                        break;
                        
                    case EnvironmentalObjectType.Bush:
                        damage = 15; // Bushes are easy to gather from
                        break;
                        
                    default:
                        damage = 10;
                        break;
                }
            }
            
            return damage;
        }

        /// <summary>
        /// Raises the ResourceGathered event.
        /// </summary>
        /// <param name="resource">The resource that was gathered.</param>
        /// <param name="amount">The amount that was gathered.</param>
        /// <param name="sourceEntity">The entity the resource was gathered from.</param>
        protected virtual void OnResourceGathered(Resource resource, int amount, Entity sourceEntity)
        {
            ResourceGathered?.Invoke(this, new ResourceGatheredEventArgs(resource, amount, sourceEntity));
        }
    }

    /// <summary>
    /// Event arguments for when a resource is gathered.
    /// </summary>
    public class ResourceGatheredEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the resource that was gathered.
        /// </summary>
        public Resource Resource { get; }

        /// <summary>
        /// Gets the amount that was gathered.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// Gets the entity the resource was gathered from.
        /// </summary>
        public Entity SourceEntity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGatheredEventArgs"/> class.
        /// </summary>
        /// <param name="resource">The resource that was gathered.</param>
        /// <param name="amount">The amount that was gathered.</param>
        /// <param name="sourceEntity">The entity the resource was gathered from.</param>
        public ResourceGatheredEventArgs(Resource resource, int amount, Entity sourceEntity)
        {
            Resource = resource;
            Amount = amount;
            SourceEntity = sourceEntity;
        }
    }
}
