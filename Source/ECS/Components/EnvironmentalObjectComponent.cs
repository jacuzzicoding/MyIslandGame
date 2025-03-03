using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Core;
using MyIslandGame.Core.Resources;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component for interactive environmental objects that can be sources of resources.
    /// </summary>
    public class EnvironmentalObjectComponent : Component
    {
        /// <summary>
        /// Gets the type of environmental object.
        /// </summary>
        public EnvironmentalObjectType ObjectType { get; }

        /// <summary>
        /// Gets or sets the growth level of the object (0-100).
        /// </summary>
        public int Growth { get; set; }

        /// <summary>
        /// Gets or sets the health/integrity of the object (0-100).
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Gets a value indicating whether this object can be harvested.
        /// </summary>
        public bool IsHarvestable => Growth >= 100 && Health > 0;

        /// <summary>
        /// Gets the possible resource yields from this object.
        /// </summary>
        public List<ResourceYield> PossibleYields { get; }

        /// <summary>
        /// Gets the time required for this object to regrow after being harvested.
        /// </summary>
        public TimeSpan RegrowthTime { get; }

        /// <summary>
        /// Gets or sets the last time this object was harvested.
        /// </summary>
        public TimeSpan? LastHarvestTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this object is regenerating.
        /// </summary>
        public bool IsRegenerating { get; set; }

        /// <summary>
        /// Gets or sets the original texture used when the object is fully grown and healthy.
        /// </summary>
        public Texture2D FullTexture { get; set; }

        /// <summary>
        /// Gets or sets the texture used when the object is partially depleted.
        /// </summary>
        public Texture2D PartialTexture { get; set; }

        /// <summary>
        /// Gets or sets the texture used when the object is fully depleted/harvested.
        /// </summary>
        public Texture2D DepletedTexture { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentalObjectComponent"/> class.
        /// </summary>
        /// <param name="objectType">The type of environmental object.</param>
        /// <param name="initialGrowth">The initial growth level (0-100).</param>
        /// <param name="health">The initial health of the object.</param>
        /// <param name="yields">The possible resource yields.</param>
        /// <param name="regrowthTime">The time required for regrowth after harvesting.</param>
        public EnvironmentalObjectComponent(
            EnvironmentalObjectType objectType,
            int initialGrowth,
            int health,
            List<ResourceYield> yields,
            TimeSpan regrowthTime)
        {
            ObjectType = objectType;
            Growth = Math.Clamp(initialGrowth, 0, 100);
            Health = Math.Clamp(health, 0, 100);
            PossibleYields = yields ?? new List<ResourceYield>();
            RegrowthTime = regrowthTime;
        }

        /// <summary>
        /// Gets the required tool type for harvesting this object.
        /// </summary>
        public string RequiredToolType
        {
            get
            {
                return ObjectType switch
                {
                    EnvironmentalObjectType.Tree => "axe",
                    EnvironmentalObjectType.Rock => "pickaxe",
                    EnvironmentalObjectType.Bush => null, // No tool required
                    EnvironmentalObjectType.Flower => null, // No tool required
                    _ => null
                };
            }
        }

        /// <summary>
        /// Checks if the object can be harvested with the given tool.
        /// </summary>
        /// <param name="toolType">The type of tool being used (null for bare hands).</param>
        /// <returns>True if the object can be harvested, otherwise false.</returns>
        public bool CanHarvest(string toolType = null)
        {
            if (!IsHarvestable)
                return false;

            // Check if a specific tool is required
            if (RequiredToolType != null && toolType != RequiredToolType)
                return false;

            return true;
        }

        /// <summary>
        /// Harvests resources from this object.
        /// </summary>
        /// <param name="damage">The amount of damage to apply to the object.</param>
        /// <param name="toolType">The type of tool being used (null for bare hands).</param>
        /// <returns>A list of harvested resources.</returns>
        public List<ResourceYield> Harvest(int damage, string toolType = null)
        {
            if (!CanHarvest(toolType))
                return new List<ResourceYield>();

            // Reduce health based on damage
            Health = Math.Max(0, Health - damage);

            // Calculate harvest results
            var results = new List<ResourceYield>();
            
            // Calculate harvest percentage based on damage relative to health
            float harvestPercentage = (float)damage / 100f;
            harvestPercentage = Math.Min(1.0f, harvestPercentage);

            // If completely harvested, return full yields and start regeneration
            if (Health <= 0)
            {
                IsRegenerating = true;
                LastHarvestTime = TimeSpan.FromTicks(DateTime.Now.Ticks);
                results.AddRange(PossibleYields);
            }
            // Otherwise return partial yields based on damage percentage
            else
            {
                foreach (var yield in PossibleYields)
                {
                    int amount = Math.Max(1, (int)(yield.BaseAmount * harvestPercentage));
                    results.Add(new ResourceYield(yield.ResourceId, amount, yield.RandomVariance));
                }
            }

            // Update the sprite component if this entity has one
            UpdateVisuals();

            return results;
        }

        /// <summary>
        /// Updates the growth and regeneration state of this object.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        /// <param name="timeManager">The game's time manager.</param>
        public void Update(GameTime gameTime, TimeManager timeManager)
        {
            // Update growth for growing objects
            if (Growth < 100)
            {
                // Base growth rate (units per real-time second)
                float baseGrowthRate = 5.0f;
                
                // Adjust growth rate based on time of day - faster during day
                float timeMultiplier = timeManager.CurrentTimeOfDay == TimeManager.TimeOfDay.Day ? 1.5f : 0.5f;
                
                // Apply growth
                float growthDelta = baseGrowthRate * timeMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Growth = Math.Min(100, Growth + (int)growthDelta);
                
                // Update visuals as growth changes
                UpdateVisuals();
            }

            // Update regeneration for harvested objects
            if (IsRegenerating && LastHarvestTime.HasValue)
            {
                TimeSpan currentTime = TimeSpan.FromTicks(DateTime.Now.Ticks);
                TimeSpan timeSinceHarvest = currentTime - LastHarvestTime.Value;

                if (timeSinceHarvest >= RegrowthTime)
                {
                    // Reset object to full health and stop regeneration
                    Health = 100;
                    IsRegenerating = false;
                    LastHarvestTime = null;
                    
                    // Update visuals for regenerated state
                    UpdateVisuals();
                }
            }
        }

        /// <summary>
        /// Updates the visual representation of this object based on its state.
        /// </summary>
        private void UpdateVisuals()
        {
            if (Owner != null && Owner.HasComponent<SpriteComponent>())
            {
                var spriteComponent = Owner.GetComponent<SpriteComponent>();
                
                // Depleted state
                if (Health <= 0 && IsRegenerating)
                {
                    if (DepletedTexture != null)
                    {
                        spriteComponent.Texture = DepletedTexture;
                    }
                }
                // Partial health state
                else if (Health < 50 && Health > 0)
                {
                    if (PartialTexture != null)
                    {
                        spriteComponent.Texture = PartialTexture;
                    }
                }
                // Full health state
                else if (Health > 0 && Growth >= 100)
                {
                    if (FullTexture != null)
                    {
                        spriteComponent.Texture = FullTexture;
                    }
                }
                
                // Update opacity based on growth for growing objects
                if (Growth < 100)
                {
                    float opacityFactor = Math.Max(0.3f, Growth / 100.0f);
                    spriteComponent.Color = new Color(spriteComponent.Color, opacityFactor);
                }
                else
                {
                    spriteComponent.Color = new Color(spriteComponent.Color, 1.0f);
                }
            }
        }

        /// <summary>
        /// Called when the component is attached to an entity.
        /// </summary>
        public override void OnAttached()
        {
            base.OnAttached();
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Enumeration of environmental object types.
    /// </summary>
    public enum EnvironmentalObjectType
    {
        /// <summary>Trees that provide wood and leaves.</summary>
        Tree,
        
        /// <summary>Rocks that provide stone and minerals.</summary>
        Rock,
        
        /// <summary>Bushes that may provide berries and fibers.</summary>
        Bush,
        
        /// <summary>Flowers that may provide seeds and berries.</summary>
        Flower,
        
        /// <summary>Various types of grass.</summary>
        Grass,
        
        /// <summary>Fallen logs that provide wood.</summary>
        FallenLog,
        
        /// <summary>Stumps from harvested trees.</summary>
        Stump,
        
        /// <summary>Large boulders that provide stone.</summary>
        Boulder,
        
        /// <summary>Ore deposits for mining minerals.</summary>
        OreDeposit,
        
        /// <summary>Shells found on beaches.</summary>
        Shell,
        
        /// <summary>Driftwood found near water.</summary>
        Driftwood
    }

    /// <summary>
    /// Represents a resource yield from an environmental object.
    /// </summary>
    public class ResourceYield
    {
        /// <summary>
        /// Gets the identifier of the resource.
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Gets the base amount of the resource provided.
        /// </summary>
        public int BaseAmount { get; }

        /// <summary>
        /// Gets the random variance in yield amount (0.0 to 1.0).
        /// </summary>
        public float RandomVariance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceYield"/> class.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="baseAmount">The base amount yielded.</param>
        /// <param name="randomVariance">The random variance (0.0 to 1.0).</param>
        public ResourceYield(string resourceId, int baseAmount, float randomVariance = 0.2f)
        {
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            BaseAmount = Math.Max(1, baseAmount);
            RandomVariance = Math.Clamp(randomVariance, 0f, 1f);
        }

        /// <summary>
        /// Gets the actual yield amount with randomization.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <returns>The actual yield amount.</returns>
        public int GetAmount(Random random)
        {
            if (random == null)
            {
                return BaseAmount;
            }

            // Apply random variance (-variance to +variance)
            float variance = (random.NextSingle() * 2 - 1) * RandomVariance;
            return Math.Max(1, (int)(BaseAmount * (1 + variance)));
        }
    }
}
