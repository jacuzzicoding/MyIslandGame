using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.World
{
    /// <summary>
    /// Generates procedural tile-based worlds.
    /// </summary>
    public class WorldGenerator
    {
        private Random _random;
        private GraphicsDevice _graphicsDevice;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldGenerator"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="seed">The random seed for generation.</param>
        public WorldGenerator(GraphicsDevice graphicsDevice, int seed = 0)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            
            // Use the provided seed or generate a random one
            _random = seed == 0 
                ? new Random() 
                : new Random(seed);
        }
        
        /// <summary>
        /// Generates a simple test map with grass and some random features.
        /// </summary>
        /// <param name="width">The width of the map in tiles.</param>
        /// <param name="height">The height of the map in tiles.</param>
        /// <param name="tileSize">The size of each tile in pixels.</param>
        /// <returns>The generated tile map.</returns>
        public TileMap GenerateTestMap(int width, int height, int tileSize)
        {
            TileMap map = new TileMap(width, height, tileSize);
            
            // For now, use simple colored textures
            Texture2D grassTexture = CreateColoredTexture(tileSize, tileSize, Color.Green);
            Texture2D waterTexture = CreateColoredTexture(tileSize, tileSize, Color.Blue);
            Texture2D sandTexture = CreateColoredTexture(tileSize, tileSize, Color.SandyBrown);
            Texture2D stoneTexture = CreateColoredTexture(tileSize, tileSize, Color.Gray);
            
            // Fill with grass by default
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile grassTile = new Tile(TileType.Grass, grassTexture, true, false);
                    map.SetTile(x, y, grassTile);
                }
            }
            
            // Add some water features (simple ponds)
            int numPonds = _random.Next(3, 8);
            for (int i = 0; i < numPonds; i++)
            {
                int pondX = _random.Next(width);
                int pondY = _random.Next(height);
                int pondSize = _random.Next(3, 8);
                
                AddCircularFeature(map, pondX, pondY, pondSize, TileType.Water, waterTexture, true, true);
                
                // Add sand around the water
                AddCircularFeature(map, pondX, pondY, pondSize + 1, TileType.Sand, sandTexture, true, false, true);
            }
            
            // Add some stone clusters
            int numStoneClusters = _random.Next(5, 12);
            for (int i = 0; i < numStoneClusters; i++)
            {
                int stoneX = _random.Next(width);
                int stoneY = _random.Next(height);
                int stoneSize = _random.Next(1, 4);
                
                AddCircularFeature(map, stoneX, stoneY, stoneSize, TileType.Stone, stoneTexture, false, false);
            }
            
            return map;
        }
        
        /// <summary>
        /// Creates a circular feature of tiles centered at the specified coordinates.
        /// </summary>
        /// <param name="map">The map to modify.</param>
        /// <param name="centerX">The center x-coordinate.</param>
        /// <param name="centerY">The center y-coordinate.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="tileType">The type of tile to place.</param>
        /// <param name="texture">The texture for the tiles.</param>
        /// <param name="isPassable">Whether the tiles are passable.</param>
        /// <param name="isWater">Whether the tiles are water.</param>
        /// <param name="onlyOverwrite">If true, only overwrite specific tile types.</param>
        private void AddCircularFeature(
            TileMap map, 
            int centerX, 
            int centerY, 
            int radius, 
            TileType tileType, 
            Texture2D texture, 
            bool isPassable, 
            bool isWater,
            bool onlyOverwrite = false)
        {
            int radiusSquared = radius * radius;
            
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    {
                        int deltaX = x - centerX;
                        int deltaY = y - centerY;
                        
                        // Use distance formula to create a circle
                        if (deltaX * deltaX + deltaY * deltaY <= radiusSquared)
                        {
                            // For features like sand beaches, only place if the current tile isn't already a feature
                            if (!onlyOverwrite || 
                                (map.GetTile(x, y) != null && map.GetTile(x, y).Type == TileType.Grass))
                            {
                                Tile newTile = new Tile(tileType, texture, isPassable, isWater);
                                map.SetTile(x, y, newTile);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a texture filled with a solid color.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        /// <returns>The created texture.</returns>
        private Texture2D CreateColoredTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(_graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = color;
            }
            
            texture.SetData(colorData);
            return texture;
        }
    }
}
