using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.Core.Resources
{
    /// <summary>
    /// Represents a game resource that can be collected, stored, and used in crafting.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Gets the unique identifier for this resource.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the display name of the resource.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the resource.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the category of the resource.
        /// </summary>
        public ResourceCategory Category { get; }

        /// <summary>
        /// Gets the maximum number of resources that can be stacked in a single inventory slot.
        /// </summary>
        public int MaxStackSize { get; }

        /// <summary>
        /// Gets or sets the texture used to represent this resource in the UI and world.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        /// <param name="id">The unique resource identifier.</param>
        /// <param name="name">The display name of the resource.</param>
        /// <param name="description">The description of the resource.</param>
        /// <param name="category">The resource category.</param>
        /// <param name="maxStackSize">The maximum stack size.</param>
        public Resource(string id, string name, string description, ResourceCategory category, int maxStackSize)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Category = category;
            MaxStackSize = Math.Max(1, maxStackSize);
        }

        /// <summary>
        /// Determines whether this resource can stack with another resource.
        /// </summary>
        /// <param name="other">The other resource to check.</param>
        /// <returns>True if the resources can stack, otherwise false.</returns>
        public bool CanStackWith(Resource other)
        {
            return other != null && Id == other.Id;
        }

        /// <summary>
        /// Creates a placeholder texture for this resource.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used to create the texture.</param>
        /// <returns>A placeholder texture.</returns>
        public Texture2D CreatePlaceholderTexture(GraphicsDevice graphicsDevice)
        {
            // Create a simple colored square based on resource category
            int size = 32;
            Texture2D texture = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];

            // Choose color based on resource category
            Color color = Category switch
            {
                ResourceCategory.Organic => new Color(139, 69, 19),    // Brown
                ResourceCategory.Mineral => new Color(169, 169, 169),  // Gray
                ResourceCategory.Environmental => new Color(65, 105, 225),  // Royal Blue
                _ => Color.White
            };

            // Fill the texture with the category color
            for (int i = 0; i < data.Length; i++)
            {
                // Create a border of darker pixels
                int x = i % size;
                int y = i / size;
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                {
                    data[i] = new Color(color.R / 2, color.G / 2, color.B / 2);
                }
                else
                {
                    data[i] = color;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }

    /// <summary>
    /// Enumeration of resource categories.
    /// </summary>
    public enum ResourceCategory
    {
        /// <summary>
        /// Organic resources like wood, leaves, and fruits.
        /// </summary>
        Organic,

        /// <summary>
        /// Mineral resources like stone, sand, and ores.
        /// </summary>
        Mineral,

        /// <summary>
        /// Environmental resources like water, soil, and snow.
        /// </summary>
        Environmental
    }
}
