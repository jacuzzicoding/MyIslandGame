using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.World
{
    /// <summary>
    /// Represents a single tile in the game world.
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// Gets or sets the type of this tile.
        /// </summary>
        public TileType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the texture of this tile.
        /// </summary>
        public Texture2D Texture { get; set; }
        
        /// <summary>
        /// Gets or sets whether this tile is passable by the player.
        /// </summary>
        public bool IsPassable { get; set; }
        
        /// <summary>
        /// Gets or sets whether this tile is water.
        /// </summary>
        public bool IsWater { get; set; }
        
        /// <summary>
        /// Gets or sets the source rectangle for drawing this tile.
        /// </summary>
        public Rectangle? SourceRectangle { get; set; }
        
        /// <summary>
        /// Gets or sets additional data for this tile.
        /// </summary>
        public int TileData { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="type">The tile type.</param>
        /// <param name="texture">The tile texture.</param>
        /// <param name="isPassable">Whether the tile is passable.</param>
        /// <param name="isWater">Whether the tile is water.</param>
        /// <param name="sourceRectangle">Optional source rectangle for drawing.</param>
        public Tile(TileType type, Texture2D texture, bool isPassable, bool isWater, Rectangle? sourceRectangle = null)
        {
            Type = type;
            Texture = texture;
            IsPassable = isPassable;
            IsWater = isWater;
            SourceRectangle = sourceRectangle;
            TileData = 0;
        }
    }
    
    /// <summary>
    /// Defines the different types of tiles in the game.
    /// </summary>
    public enum TileType
    {
        /// <summary>Empty tile.</summary>
        Empty,
        
        /// <summary>Grass tile.</summary>
        Grass,
        
        /// <summary>Dirt tile.</summary>
        Dirt,
        
        /// <summary>Sand tile.</summary>
        Sand,
        
        /// <summary>Water tile.</summary>
        Water,
        
        /// <summary>Stone tile.</summary>
        Stone,
        
        /// <summary>Wood tile.</summary>
        Wood,
        
        /// <summary>Snow tile.</summary>
        Snow
    }
}
