using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Core.Resources;
using MyIslandGame.ECS; // Add this namespace for Entity

namespace MyIslandGame.Inventory
{
    /// <summary>
    /// Base class for all inventory items.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets the unique identifier for this item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the display name of the item.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the item.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the maximum stack size for this item.
        /// </summary>
        public virtual int MaxStackSize => 1;

        /// <summary>
        /// Gets or sets the icon used to represent this item in the UI.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class.
        /// </summary>
        /// <param name="id">The unique item identifier.</param>
        /// <param name="name">The display name of the item.</param>
        /// <param name="description">The description of the item.</param>
        public Item(string id, string name, string description)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <summary>
        /// Determines whether this item can stack with another item.
        /// </summary>
        /// <param name="other">The other item to check.</param>
        /// <returns>True if the items can stack, otherwise false.</returns>
        public virtual bool CanStack(Item other)
        {
            return other != null && Id == other.Id;
        }

        /// <summary>
        /// Called when the item is used.
        /// </summary>
        /// <param name="entity">The entity using the item.</param>
        /// <returns>True if the item was used successfully, otherwise false.</returns>
        public virtual bool Use(Entity entity)
        {
            // Base implementation does nothing
            return false;
        }

        /// <summary>
        /// Creates an item from a resource.
        /// </summary>
        /// <param name="resource">The resource to create an item from.</param>
        /// <returns>A new item representing the resource.</returns>
        public static Item FromResource(Resource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            var item = new ResourceItem(resource);
            return item;
        }
    }

    /// <summary>
    /// Represents a resource that can be held in an inventory.
    /// </summary>
    public class ResourceItem : Item
    {
        /// <summary>
        /// Gets the resource this item represents.
        /// </summary>
        public Resource Resource { get; }

        /// <summary>
        /// Gets the maximum stack size for this resource item.
        /// </summary>
        public override int MaxStackSize => Resource.MaxStackSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceItem"/> class.
        /// </summary>
        /// <param name="resource">The resource this item represents.</param>
        public ResourceItem(Resource resource)
            : base(resource.Id, resource.Name, resource.Description)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Icon = resource.Icon;
        }
    }

    /// <summary>
    /// Represents a tool that can be used for gathering resources.
    /// </summary>
    public class Tool : Item
    {
        /// <summary>
        /// Gets the tool type.
        /// </summary>
        public string ToolType { get; }

        /// <summary>
        /// Gets the efficiency level of the tool.
        /// </summary>
        public int Efficiency { get; }

        /// <summary>
        /// Gets or sets the current durability of the tool.
        /// </summary>
        public int Durability { get; private set; }

        /// <summary>
        /// Gets the maximum durability of the tool.
        /// </summary>
        public int MaxDurability { get; }

        /// <summary>
        /// Gets a value indicating whether the tool is broken.
        /// </summary>
        public bool IsBroken => Durability <= 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool"/> class.
        /// </summary>
        /// <param name="id">The unique tool identifier.</param>
        /// <param name="name">The display name of the tool.</param>
        /// <param name="description">The description of the tool.</param>
        /// <param name="toolType">The type of tool.</param>
        /// <param name="efficiency">The efficiency level of the tool.</param>
        /// <param name="durability">The maximum durability of the tool.</param>
        public Tool(string id, string name, string description, string toolType, int efficiency, int durability)
            : base(id, name, description)
        {
            ToolType = toolType ?? throw new ArgumentNullException(nameof(toolType));
            Efficiency = Math.Max(1, efficiency);
            MaxDurability = Math.Max(1, durability);
            Durability = MaxDurability;
        }

        /// <summary>
        /// Uses the tool, reducing its durability.
        /// </summary>
        /// <returns>True if the tool was used successfully, false if it's broken.</returns>
        public bool UseTool()
        {
            if (IsBroken)
            {
                return false;
            }

            Durability--;
            return true;
        }

        /// <summary>
        /// Repairs the tool to its maximum durability.
        /// </summary>
        public void Repair()
        {
            Durability = MaxDurability;
        }

        /// <summary>
        /// Creates a primitive axe tool.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used to create the icon.</param>
        /// <returns>A new axe tool.</returns>
        public static Tool CreateAxe(GraphicsDevice graphicsDevice)
        {
            var axe = new Tool("axe", "Axe", "A tool for chopping wood.", "axe", 2, 50);
            axe.Icon = CreateAxeIcon(graphicsDevice);
            return axe;
        }

        /// <summary>
        /// Creates a primitive pickaxe tool.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used to create the icon.</param>
        /// <returns>A new pickaxe tool.</returns>
        public static Tool CreatePickaxe(GraphicsDevice graphicsDevice)
        {
            var pickaxe = new Tool("pickaxe", "Pickaxe", "A tool for mining stone.", "pickaxe", 2, 40);
            pickaxe.Icon = CreatePickaxeIcon(graphicsDevice);
            return pickaxe;
        }

        #region Icon Creation Methods

        /// <summary>
        /// Creates a simple axe icon.
        /// </summary>
        private static Texture2D CreateAxeIcon(GraphicsDevice graphicsDevice)
        {
            int size = 32;
            Texture2D texture = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];

            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }

            // Draw axe handle (brown)
            Color handleColor = new Color(139, 69, 19); // Brown
            for (int y = 10; y < size - 5; y++)
            {
                for (int x = 15; x < 19; x++)
                {
                    data[y * size + x] = handleColor;
                }
            }

            // Draw axe head (gray)
            Color bladeColor = new Color(192, 192, 192); // Silver
            for (int y = 5; y < 15; y++)
            {
                for (int x = 8; x < 24; x++)
                {
                    // Create axe head shape
                    if ((y - 10) * (y - 10) + (x - 16) * (x - 16) / 4 < 16)
                    {
                        data[y * size + x] = bladeColor;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a simple pickaxe icon.
        /// </summary>
        private static Texture2D CreatePickaxeIcon(GraphicsDevice graphicsDevice)
        {
            int size = 32;
            Texture2D texture = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];

            // Fill with transparent initially
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }

            // Draw pickaxe handle (brown)
            Color handleColor = new Color(139, 69, 19); // Brown
            for (int y = 10; y < size - 5; y++)
            {
                for (int x = 15; x < 19; x++)
                {
                    data[y * size + x] = handleColor;
                }
            }

            // Draw pickaxe head (gray)
            Color bladeColor = new Color(169, 169, 169); // Dark Gray
            
            // Left point
            for (int y = 5; y < 13; y++)
            {
                for (int x = 8; x < 16; x++)
                {
                    if (x >= 16 - (y - 5))
                    {
                        data[y * size + x] = bladeColor;
                    }
                }
            }
            
            // Right point
            for (int y = 5; y < 13; y++)
            {
                for (int x = 17; x < 25; x++)
                {
                    if (x <= 17 + (y - 5))
                    {
                        data[y * size + x] = bladeColor;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        #endregion
    }
}
