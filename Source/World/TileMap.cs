using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.World
{
    /// <summary>
    /// Manages a grid of tiles that make up the game world.
    /// </summary>
    public class TileMap
    {
        private Tile[,] _tiles;
        private int _width;
        private int _height;
        private int _tileSize;
        
        /// <summary>
        /// Gets the width of the tile map in tiles.
        /// </summary>
        public int Width => _width;
        
        /// <summary>
        /// Gets the height of the tile map in tiles.
        /// </summary>
        public int Height => _height;
        
        /// <summary>
        /// Gets the size of each tile in pixels.
        /// </summary>
        public int TileSize => _tileSize;
        
        /// <summary>
        /// Gets the width of the tile map in pixels.
        /// </summary>
        public int PixelWidth => _width * _tileSize;
        
        /// <summary>
        /// Gets the height of the tile map in pixels.
        /// </summary>
        public int PixelHeight => _height * _tileSize;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TileMap"/> class.
        /// </summary>
        /// <param name="width">The width of the map in tiles.</param>
        /// <param name="height">The height of the map in tiles.</param>
        /// <param name="tileSize">The size of each tile in pixels.</param>
        public TileMap(int width, int height, int tileSize)
        {
            _width = width;
            _height = height;
            _tileSize = tileSize;
            _tiles = new Tile[width, height];
            
            // Initialize with empty tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = null;
                }
            }
        }
        
        /// <summary>
        /// Gets a tile at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The tile at the specified coordinates, or null if out of bounds.</returns>
        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return null;
            }
            
            return _tiles[x, y];
        }
        
        /// <summary>
        /// Sets a tile at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="tile">The tile to set.</param>
        /// <returns>True if successful, false if out of bounds.</returns>
        public bool SetTile(int x, int y, Tile tile)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return false;
            }
            
            _tiles[x, y] = tile;
            return true;
        }
        
        /// <summary>
        /// Converts a world position to tile coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns>The tile coordinates.</returns>
        public Point WorldToTile(Vector2 position)
        {
            return new Point(
                (int)Math.Floor(position.X / _tileSize),
                (int)Math.Floor(position.Y / _tileSize));
        }
        
        /// <summary>
        /// Converts tile coordinates to a world position (top-left of tile).
        /// </summary>
        /// <param name="tileX">The tile x-coordinate.</param>
        /// <param name="tileY">The tile y-coordinate.</param>
        /// <returns>The world position.</returns>
        public Vector2 TileToWorld(int tileX, int tileY)
        {
            return new Vector2(tileX * _tileSize, tileY * _tileSize);
        }
        
        /// <summary>
        /// Converts tile coordinates to the center position of the tile in world space.
        /// </summary>
        /// <param name="tileX">The tile x-coordinate.</param>
        /// <param name="tileY">The tile y-coordinate.</param>
        /// <returns>The world position at the center of the tile.</returns>
        public Vector2 TileToWorldCenter(int tileX, int tileY)
        {
            return new Vector2(
                tileX * _tileSize + _tileSize / 2f,
                tileY * _tileSize + _tileSize / 2f);
        }
        
        /// <summary>
        /// Gets the world bounds of the entire tile map.
        /// </summary>
        /// <returns>A rectangle representing the world bounds.</returns>
        public Rectangle GetWorldBounds()
        {
            return new Rectangle(0, 0, PixelWidth, PixelHeight);
        }
        
        /// <summary>
        /// Gets the world bounds of a specific tile.
        /// </summary>
        /// <param name="tileX">The tile x-coordinate.</param>
        /// <param name="tileY">The tile y-coordinate.</param>
        /// <returns>A rectangle representing the tile bounds in world space.</returns>
        public Rectangle GetTileBounds(int tileX, int tileY)
        {
            return new Rectangle(
                tileX * _tileSize,
                tileY * _tileSize,
                _tileSize,
                _tileSize);
        }
        
        /// <summary>
        /// Checks if a tile at the specified coordinates is passable.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True if the tile is passable or out of bounds, otherwise false.</returns>
        public bool IsTilePassable(int x, int y)
        {
            Tile tile = GetTile(x, y);
            return tile == null || tile.IsPassable;
        }
        
        /// <summary>
        /// Checks if a world position is on a passable tile.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns>True if the position is on a passable tile, otherwise false.</returns>
        public bool IsPositionPassable(Vector2 position)
        {
            Point tilePos = WorldToTile(position);
            return IsTilePassable(tilePos.X, tilePos.Y);
        }
        
        /// <summary>
        /// Draws the tile map using the provided sprite batch.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="viewRect">The view rectangle in world coordinates.</param>
        public void Draw(SpriteBatch spriteBatch, Rectangle viewRect)
        {
            // Calculate the visible tile range
            int startX = Math.Max(0, viewRect.Left / _tileSize);
            int startY = Math.Max(0, viewRect.Top / _tileSize);
            int endX = Math.Min(_width - 1, (viewRect.Right + _tileSize - 1) / _tileSize);
            int endY = Math.Min(_height - 1, (viewRect.Bottom + _tileSize - 1) / _tileSize);
            
            // Draw visible tiles
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Tile tile = _tiles[x, y];
                    
                    if (tile != null && tile.Texture != null)
                    {
                        Vector2 position = new Vector2(x * _tileSize, y * _tileSize);
                        
                        spriteBatch.Draw(
                            tile.Texture,
                            position,
                            tile.SourceRectangle,
                            Color.White);
                    }
                }
            }
        }
    }
}
