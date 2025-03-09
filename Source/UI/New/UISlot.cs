using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Inventory;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A UI element for displaying and interacting with inventory slots.
    /// </summary>
    public class UISlot : InteractableElement
    {
        private Texture2D _normalTexture;
        private Texture2D _hoverTexture;
        private Texture2D _selectedTexture;
        private Texture2D _disabledTexture;
        private SpriteFont _font;
        private bool _isSelected;
        
        /// <summary>
        /// Gets or sets a value indicating whether this slot is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }
        
        /// <summary>
        /// Gets or sets the inventory slot associated with this UI slot.
        /// </summary>
        public InventorySlot InventorySlot { get; set; }
        
        /// <summary>
        /// Gets or sets user data associated with this slot.
        /// </summary>
        public object Tag { get; set; }
        
        /// <summary>
        /// Gets or sets the index of this slot within its container.
        /// </summary>
        public int SlotIndex { get; set; }
        
        /// <summary>
        /// Occurs when an item is dragged from this slot.
        /// </summary>
        public event EventHandler<UISlotEventArgs> ItemDragStart;
        
        /// <summary>
        /// Occurs when an item is dropped onto this slot.
        /// </summary>
        public event EventHandler<UISlotEventArgs> ItemDrop;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UISlot"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the slot.</param>
        /// <param name="font">The font to use for text.</param>
        public UISlot(GraphicsDevice graphicsDevice, Rectangle bounds, SpriteFont font)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            Bounds = bounds;
            _font = font;
            
            // Create textures for different slot states
            _normalTexture = CreateSlotTexture(graphicsDevice, bounds.Width, bounds.Height, 
                                              Color.Gray, Color.DarkGray);
                                              
            _hoverTexture = CreateSlotTexture(graphicsDevice, bounds.Width, bounds.Height, 
                                             Color.LightGray, Color.Gray);
                                             
            _selectedTexture = CreateSlotTexture(graphicsDevice, bounds.Width, bounds.Height, 
                                                Color.Gold, Color.Goldenrod);
                                                
            _disabledTexture = CreateSlotTexture(graphicsDevice, bounds.Width, bounds.Height, 
                                                Color.DarkGray, Color.DimGray, 128);
        }
        
        /// <summary>
        /// Initializes the slot.
        /// </summary>
        public override void Initialize()
        {
            // No additional initialization needed
        }
        
        /// <summary>
        /// Updates the slot.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // No additional update logic needed
        }
        
        /// <summary>
        /// Draws the slot and its contents.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Choose appropriate texture based on state
            Texture2D texture;
            
            if (!IsActive)
            {
                texture = _disabledTexture;
            }
            else if (_isSelected)
            {
                texture = _selectedTexture;
            }
            else if (IsHovered)
            {
                texture = _hoverTexture;
            }
            else
            {
                texture = _normalTexture;
            }
            
            // Draw slot background
            Vector2 position = GetAbsolutePosition();
            spriteBatch.Draw(texture, position, Color.White);
            
            // Draw item if exists
            if (InventorySlot != null && !InventorySlot.IsEmpty)
            {
                var item = InventorySlot.Item;
                
                if (item.Icon != null)
                {
                    // Draw item icon (centered with padding)
                    int iconSize = Bounds.Width - 10;
                    Vector2 iconPos = position + new Vector2(5, 5);
                    
                    Rectangle iconRect = new Rectangle(
                        (int)iconPos.X, 
                        (int)iconPos.Y, 
                        iconSize, 
                        iconSize);
                        
                    spriteBatch.Draw(item.Icon, iconRect, Color.White);
                    
                    // Draw quantity if more than 1
                    if (InventorySlot.Quantity > 1 && _font != null)
                    {
                        string quantityText = InventorySlot.Quantity.ToString();
                        Vector2 textSize = _font.MeasureString(quantityText);
                        Vector2 textPos = position + new Vector2(
                            Bounds.Width - textSize.X - 2,
                            Bounds.Height - textSize.Y - 2);
                            
                        // Draw with shadow for better readability
                        spriteBatch.DrawString(_font, quantityText, textPos + new Vector2(1, 1), Color.Black);
                        spriteBatch.DrawString(_font, quantityText, textPos, Color.White);
                    }
                }
                else
                {
                    // Draw item name if no icon is available
                    if (_font != null)
                    {
                        string itemName = item.Name;
                        Vector2 textSize = _font.MeasureString(itemName);
                        
                        // Scale text to fit if needed
                        float scale = 1.0f;
                        if (textSize.X > Bounds.Width - 6)
                        {
                            scale = (Bounds.Width - 6) / textSize.X;
                        }
                        
                        Vector2 textPos = position + new Vector2(
                            (Bounds.Width - textSize.X * scale) / 2,
                            (Bounds.Height - textSize.Y * scale) / 2);
                            
                        spriteBatch.DrawString(_font, itemName, textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a texture for the slot.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="borderColor">The border color.</param>
        /// <param name="fillColor">The fill color.</param>
        /// <param name="alpha">The alpha value of the colors.</param>
        /// <returns>A new texture for the slot.</returns>
        private Texture2D CreateSlotTexture(GraphicsDevice graphicsDevice, int width, int height, 
                                          Color borderColor, Color fillColor, byte alpha = 255)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Apply alpha to colors
            borderColor = new Color(borderColor.R, borderColor.G, borderColor.B, alpha);
            fillColor = new Color(fillColor.R, fillColor.G, fillColor.B, alpha);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    
                    // Create border (2 pixels thick for better visibility)
                    if (x < 2 || x >= width - 2 || y < 2 || y >= height - 2)
                    {
                        data[index] = borderColor;
                    }
                    else
                    {
                        data[index] = fillColor;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Raises the ItemDragStart event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public virtual void OnItemDragStart(UISlotEventArgs args)
        {
            ItemDragStart?.Invoke(this, args);
        }
        
        /// <summary>
        /// Raises the ItemDrop event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public virtual void OnItemDrop(UISlotEventArgs args)
        {
            ItemDrop?.Invoke(this, args);
        }
    }
    
    /// <summary>
    /// Event arguments for slot-related events.
    /// </summary>
    public class UISlotEventArgs : UIEventArgs
    {
        /// <summary>
        /// Gets the source slot.
        /// </summary>
        public UISlot SourceSlot { get; }
        
        /// <summary>
        /// Gets the target slot.
        /// </summary>
        public UISlot TargetSlot { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UISlotEventArgs"/> class.
        /// </summary>
        /// <param name="sourceSlot">The source slot.</param>
        /// <param name="mousePosition">The mouse position.</param>
        public UISlotEventArgs(UISlot sourceSlot, Point mousePosition)
            : base(sourceSlot, mousePosition)
        {
            SourceSlot = sourceSlot;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UISlotEventArgs"/> class.
        /// </summary>
        /// <param name="sourceSlot">The source slot.</param>
        /// <param name="targetSlot">The target slot.</param>
        /// <param name="mousePosition">The mouse position.</param>
        public UISlotEventArgs(UISlot sourceSlot, UISlot targetSlot, Point mousePosition)
            : base(sourceSlot, mousePosition)
        {
            SourceSlot = sourceSlot;
            TargetSlot = targetSlot;
        }
    }
}