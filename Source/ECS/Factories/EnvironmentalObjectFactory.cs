using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.ECS.Components;

namespace MyIslandGame.ECS.Factories
{
    /// <summary>
    /// Factory for creating environmental object entities.
    /// </summary>
    public static class EnvironmentalObjectFactory
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Creates a tree entity.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="position">The position of the tree.</param>
        /// <param name="growthStage">The initial growth stage (0-100).</param>
        /// <param name="health">The initial health of the tree.</param>
        /// <returns>The created tree entity.</returns>
        public static Entity CreateTree(EntityManager entityManager, GraphicsDevice graphicsDevice, Vector2 position, int growthStage = 100, int health = 100)
        {
            Entity entity = entityManager.CreateEntity();

            // Calculate scale based on growth stage
            float scaleFactor = Math.Max(0.5f, growthStage / 100f);

            // Create tree textures for different states
            Texture2D fullTexture = CreateTreeTexture(graphicsDevice, 100);
            Texture2D partialTexture = CreateTreeTexture(graphicsDevice, 50);
            Texture2D depletedTexture = CreateStumpTexture(graphicsDevice);

            // Add transform component
            entity.AddComponent(new TransformComponent(position, 0f, new Vector2(scaleFactor)));

            // Add sprite component with appropriate texture based on growth and health
            Texture2D initialTexture;
            
            if (health <= 0)
            {
                initialTexture = depletedTexture;
            }
            else if (health < 50)
            {
                initialTexture = partialTexture;
            }
            else
            {
                initialTexture = fullTexture;
            }

            var spriteComponent = new SpriteComponent(initialTexture)
            {
                // Set origin to bottom center for proper placement
                Origin = new Vector2(initialTexture.Width / 2f, initialTexture.Height)
            };
            entity.AddComponent(spriteComponent);

            // Add collider for physical interactions
            float colliderWidth = 32 * scaleFactor;
            float colliderHeight = 16 * scaleFactor; // Lower height for just the trunk
            
            entity.AddComponent(new ColliderComponent(
                new Vector2(colliderWidth, colliderHeight),
                new Vector2(0, -colliderHeight / 2) // Offset the collider to the base of the tree
            ));

            // Add environmental object component
            var resourceYields = new List<ResourceYield>
            {
                new ResourceYield("wood", 5 + (int)(5 * scaleFactor)),
                new ResourceYield("leaves", 3 + (int)(7 * scaleFactor)),
                new ResourceYield("seeds", 1)  // Small chance of seeds
            };

            var envComponent = new EnvironmentalObjectComponent(
                EnvironmentalObjectType.Tree,
                growthStage,
                health,
                resourceYields,
                TimeSpan.FromHours(48)  // In-game hours to regrow
            )
            {
                FullTexture = fullTexture,
                PartialTexture = partialTexture,
                DepletedTexture = depletedTexture
            };
            
            entity.AddComponent(envComponent);

            return entity;
        }

        /// <summary>
        /// Creates a rock entity.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="position">The position of the rock.</param>
        /// <param name="size">The size of the rock (0-100).</param>
        /// <param name="health">The initial health of the rock.</param>
        /// <returns>The created rock entity.</returns>
        public static Entity CreateRock(EntityManager entityManager, GraphicsDevice graphicsDevice, Vector2 position, int size = 100, int health = 100)
        {
            Entity entity = entityManager.CreateEntity();

            // Calculate scale based on size
            float scaleFactor = Math.Max(0.5f, size / 100f);

            // Create rock textures for different states
            Texture2D fullTexture = CreateRockTexture(graphicsDevice, 100);
            Texture2D partialTexture = CreateRockTexture(graphicsDevice, 50);
            Texture2D depletedTexture = CreateRubbleTexture(graphicsDevice);

            // Add transform component
            entity.AddComponent(new TransformComponent(position, 0f, new Vector2(scaleFactor)));

            // Add sprite component with appropriate texture based on health
            Texture2D initialTexture;
            
            if (health <= 0)
            {
                initialTexture = depletedTexture;
            }
            else if (health < 50)
            {
                initialTexture = partialTexture;
            }
            else
            {
                initialTexture = fullTexture;
            }

            var spriteComponent = new SpriteComponent(initialTexture)
            {
                // Set origin to center for proper placement
                Origin = new Vector2(initialTexture.Width / 2f, initialTexture.Height / 2f)
            };
            entity.AddComponent(spriteComponent);

            // Add collider for physical interactions
            float colliderSize = 28 * scaleFactor;
            entity.AddComponent(new ColliderComponent(
                new Vector2(colliderSize, colliderSize),
                Vector2.Zero  // Centered collider
            ));

            // Add environmental object component
            var resourceYields = new List<ResourceYield>
            {
                new ResourceYield("stone", 3 + (int)(5 * scaleFactor)),
                // Rare chance of finding flint
                new ResourceYield("flint", 1, 0.5f)
            };

            var envComponent = new EnvironmentalObjectComponent(
                EnvironmentalObjectType.Rock,
                100,  // Rocks are always fully grown
                health,
                resourceYields,
                TimeSpan.FromHours(72)  // In-game hours to regrow
            )
            {
                FullTexture = fullTexture,
                PartialTexture = partialTexture,
                DepletedTexture = depletedTexture
            };
            
            entity.AddComponent(envComponent);

            return entity;
        }

        /// <summary>
        /// Creates a bush entity.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="position">The position of the bush.</param>
        /// <param name="growthStage">The initial growth stage (0-100).</param>
        /// <param name="health">The initial health of the bush.</param>
        /// <returns>The created bush entity.</returns>
        public static Entity CreateBush(EntityManager entityManager, GraphicsDevice graphicsDevice, Vector2 position, int growthStage = 100, int health = 100)
        {
            Entity entity = entityManager.CreateEntity();

            // Calculate scale based on growth stage
            float scaleFactor = Math.Max(0.3f, growthStage / 100f);

            // Create bush textures for different states
            Texture2D fullTexture = CreateBushTexture(graphicsDevice, true);
            Texture2D partialTexture = CreateBushTexture(graphicsDevice, false);
            Texture2D depletedTexture = CreateDepletedBushTexture(graphicsDevice);

            // Add transform component
            entity.AddComponent(new TransformComponent(position, 0f, new Vector2(scaleFactor)));

            // Add sprite component with appropriate texture based on growth and health
            Texture2D initialTexture;
            
            if (health <= 0)
            {
                initialTexture = depletedTexture;
            }
            else if (health < 50 || growthStage < 50)
            {
                initialTexture = partialTexture;
            }
            else
            {
                initialTexture = fullTexture;
            }

            var spriteComponent = new SpriteComponent(initialTexture)
            {
                // Set origin to bottom center for proper placement
                Origin = new Vector2(initialTexture.Width / 2f, initialTexture.Height)
            };
            entity.AddComponent(spriteComponent);

            // Add collider for physical interactions
            float colliderSize = 24 * scaleFactor;
            entity.AddComponent(new ColliderComponent(
                new Vector2(colliderSize, colliderSize / 2),
                new Vector2(0, -colliderSize / 4)  // Offset the collider to the base of the bush
            ));

            // Add environmental object component
            var resourceYields = new List<ResourceYield>
            {
                new ResourceYield("berries", 2 + (int)(3 * scaleFactor)),
                new ResourceYield("fibers", 1 + (int)(2 * scaleFactor)),
                new ResourceYield("seeds", 1)  // Small chance of seeds
            };

            var envComponent = new EnvironmentalObjectComponent(
                EnvironmentalObjectType.Bush,
                growthStage,
                health,
                resourceYields,
                TimeSpan.FromHours(24)  // In-game hours to regrow
            )
            {
                FullTexture = fullTexture,
                PartialTexture = partialTexture,
                DepletedTexture = depletedTexture
            };
            
            entity.AddComponent(envComponent);

            return entity;
        }

        #region Texture Creation Methods

        /// <summary>
        /// Creates a tree texture for the given growth stage.
        /// </summary>
        private static Texture2D CreateTreeTexture(GraphicsDevice graphicsDevice, int growth)
        {
            // Create a simple tree texture: brown trunk with green top
            int width = 64;
            int height = 96;
            
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            // Draw trunk
            int trunkWidth = 16;
            int trunkHeight = 32;
            Color trunkColor = new Color(139, 69, 19); // Brown
            
            for (int y = height - trunkHeight; y < height; y++)
            {
                for (int x = width / 2 - trunkWidth / 2; x < width / 2 + trunkWidth / 2; x++)
                {
                    data[y * width + x] = trunkColor;
                }
            }
            
            // Draw leaves (based on growth)
            int leavesSize = (int)(48 * (growth / 100f));
            int leavesStartY = height - trunkHeight - leavesSize;
            Color leavesColor = new Color(34, 139, 34); // Forest Green
            
            if (growth >= 30) // Only draw leaves if growth is at least 30%
            {
                for (int y = Math.Max(0, leavesStartY); y < height - trunkHeight; y++)
                {
                    // Calculate width of leaves at this height (oval shape)
                    float normalizedY = (float)(y - leavesStartY) / leavesSize;
                    int leavesWidth = (int)(leavesSize * (1 - Math.Pow(normalizedY - 0.5, 2) * 2));
                    
                    for (int x = width / 2 - leavesWidth / 2; x < width / 2 + leavesWidth / 2; x++)
                    {
                        if (x >= 0 && x < width && y >= 0 && y < height)
                        {
                            data[y * width + x] = leavesColor;
                        }
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a rock texture for the given size.
        /// </summary>
        private static Texture2D CreateRockTexture(GraphicsDevice graphicsDevice, int size)
        {
            // Create a simple rock texture: gray irregular shape
            int dimension = (int)(32 * (size / 100f));
            dimension = Math.Max(16, dimension);
            Texture2D texture = new Texture2D(graphicsDevice, dimension, dimension);
            Color[] data = new Color[dimension * dimension];
            
            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            Color rockColor = new Color(105, 105, 105); // Dim Gray
            
            // Draw an irregular rock shape
            Vector2 center = new Vector2(dimension / 2f, dimension / 2f);
            float radius = dimension / 2f * 0.9f;
            
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    // Add some irregularity to the shape
                    float angle = (float)Math.Atan2(y - center.Y, x - center.X);
                    float noise = (float)(_random.NextDouble() * 0.2 + 0.8);
                    float adjustedRadius = radius * noise;
                    
                    if (distance <= adjustedRadius)
                    {
                        data[y * dimension + x] = rockColor;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a stump texture for a harvested tree.
        /// </summary>
        private static Texture2D CreateStumpTexture(GraphicsDevice graphicsDevice)
        {
            // Create a simple stump texture: just a brown circle
            int width = 24;
            int height = 12;
            
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            // Draw a brown oval stump
            Color stumpColor = new Color(101, 67, 33); // Dark brown
            Vector2 center = new Vector2(width / 2f, height / 2f);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float normalizedX = (x - center.X) / (width / 2f);
                    float normalizedY = (y - center.Y) / (height / 2f);
                    
                    if (normalizedX * normalizedX + normalizedY * normalizedY <= 1)
                    {
                        data[y * width + x] = stumpColor;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a rubble texture for a harvested rock.
        /// </summary>
        private static Texture2D CreateRubbleTexture(GraphicsDevice graphicsDevice)
        {
            // Create a simple rubble texture: small gray pebbles
            int dimension = 16;
            Texture2D texture = new Texture2D(graphicsDevice, dimension, dimension);
            Color[] data = new Color[dimension * dimension];
            
            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            Color rockColor = new Color(150, 150, 150); // Light gray
            
            // Draw a few pebbles
            for (int i = 0; i < 5; i++)
            {
                int x = _random.Next(dimension);
                int y = _random.Next(dimension);
                int size = _random.Next(2, 5);
                
                for (int dy = -size / 2; dy <= size / 2; dy++)
                {
                    for (int dx = -size / 2; dx <= size / 2; dx++)
                    {
                        int posX = x + dx;
                        int posY = y + dy;
                        
                        if (posX >= 0 && posX < dimension && posY >= 0 && posY < dimension)
                        {
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                            if (distance <= size / 2f)
                            {
                                data[posY * dimension + posX] = rockColor;
                            }
                        }
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a bush texture with or without berries.
        /// </summary>
        private static Texture2D CreateBushTexture(GraphicsDevice graphicsDevice, bool withBerries)
        {
            // Create a simple bush texture: green circular shrub
            int width = 32;
            int height = 24;
            
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            // Draw a green bush shape
            Color bushColor = new Color(40, 120, 40); // Dark green
            Color berryColor = new Color(180, 20, 20); // Red berries
            Vector2 center = new Vector2(width / 2f, height / 2f);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - center.X) / (width / 2f);
                    float dy = (y - center.Y) / (height / 2f);
                    float distanceSquared = dx * dx + dy * dy;
                    
                    if (distanceSquared <= 1.0f)
                    {
                        // Add some variation to the bush edge
                        float noise = (float)(_random.NextDouble() * 0.2 - 0.1);
                        if (distanceSquared <= 0.8f + noise)
                        {
                            data[y * width + x] = bushColor;
                            
                            // Add berries if specified
                            if (withBerries && _random.NextDouble() < 0.15 && distanceSquared > 0.3f)
                            {
                                data[y * width + x] = berryColor;
                            }
                        }
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a texture for a depleted bush.
        /// </summary>
        private static Texture2D CreateDepletedBushTexture(GraphicsDevice graphicsDevice)
        {
            // Create a simple depleted bush texture: brown/dead looking shrub
            int width = 32;
            int height = 24;
            
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            // Draw a brown/dead bush shape
            Color deadBushColor = new Color(139, 115, 85); // Tan/brown
            Vector2 center = new Vector2(width / 2f, height / 2f);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - center.X) / (width / 2f);
                    float dy = (y - center.Y) / (height / 2f);
                    float distanceSquared = dx * dx + dy * dy;
                    
                    if (distanceSquared <= 1.0f)
                    {
                        // Make the bush look more sparse and dead
                        float noise = (float)(_random.NextDouble() * 0.3 - 0.15);
                        if (distanceSquared <= 0.7f + noise && _random.NextDouble() < 0.7)
                        {
                            data[y * width + x] = deadBushColor;
                        }
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        #endregion
    }
}