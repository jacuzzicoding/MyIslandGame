using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.ECS.Components;
using MyIslandGame.Inventory;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A UI component for displaying and interacting with an inventory grid.
    /// </summary>
    public class UIInventoryGrid : UIContainer
    {
        private const int SlotSize = 40;
        private const int SlotPadding = 5;
        
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteFont _font;
        private readonly InventoryComponent _inventoryComponent;
        private readonly List<UISlot> _slots = new List<UISlot>();
        private int _selectedSlotIndex = -1;
        private bool _isHotbar;
        private UISlot _dragSource;
        private Texture2D _dragTexture;
        private Point _dragOffset;
        private bool _isDragging;
        
        /// <summary>
        /// Gets or sets a value indicating whether this is a hotbar inventory grid.
        /// </summary>
        public bool IsHotbar
        {
            get => _isHotbar;
            set => _isHotbar = value;
        }
        
        /// <summary>
        /// Gets the list of slots in this inventory grid.
        /// </summary>
        public IReadOnlyList<UISlot> Slots => _slots.AsReadOnly();
        
        /// <summary>
        /// Gets or sets the selected slot index.
        /// </summary>
        public int SelectedSlotIndex
        {
            get => _selectedSlotIndex;
            set
            {
                if (_selectedSlotIndex != value)
                {
                    // Deselect previous slot
                    if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _slots.Count)
                    {
                        _slots[_selectedSlotIndex].IsSelected = false;
                    }
                    
                    _selectedSlotIndex = value;
                    
                    // Select new slot
                    if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _slots.Count)
                    {
                        _slots[_selectedSlotIndex].IsSelected = true;
                    }
                    
                    OnSelectionChanged();
                }
            }
        }
        
        /// <summary>
        /// Occurs when the selection changes.
        /// </summary>
        public event EventHandler<UISlotEventArgs> SelectionChanged;
        
        /// <summary>
        /// Occurs when an item is used.
        /// </summary>
        public event EventHandler<UISlotEventArgs> ItemUsed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIInventoryGrid"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the inventory grid.</param>
        /// <param name="font">The font to use for text.</param>
        /// <param name="inventoryComponent">The inventory component.</param>
        /// <param name="isHotbar">Whether this is a hotbar inventory grid.</param>
        public UIInventoryGrid(
            GraphicsDevice graphicsDevice,
            Rectangle bounds,
            SpriteFont font,
            InventoryComponent inventoryComponent,
            bool isHotbar = false)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            if (inventoryComponent == null)
                throw new ArgumentNullException(nameof(inventoryComponent));
                
            Bounds = bounds;
            _graphicsDevice = graphicsDevice;
            _font = font;
            _inventoryComponent = inventoryComponent;
            _isHotbar = isHotbar;
            
            // Create UI slots
            CreateSlots();
            
            // Subscribe to inventory changes
            _inventoryComponent.Inventory.InventoryChanged += OnInventoryChanged;
            
            if (isHotbar)
            {
                // Subscribe to hotbar selection changes
                _inventoryComponent.HotbarSelectionChanged += OnHotbarSelectionChanged;
                
                // Initialize selection
                SelectedSlotIndex = _inventoryComponent.SelectedHotbarIndex;
            }
        }
        
        /// <summary>
        /// Initializes the inventory grid and its children.
        /// </summary>
        public override void Initialize()
        {
            // Don't call abstract base.Initialize()
            
            // Initialize child elements
            foreach (var child in Children)
            {
                child.Initialize();
            }
            
            // Update slots with inventory data
            UpdateSlots();
        }
        
        /// <summary>
        /// Updates the inventory grid and its children.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Update drag and drop visual
            if (_isDragging && _dragSource != null && _dragTexture != null)
            {
                // Custom update logic for dragging
            }
        }
        
        /// <summary>
        /// Draws the inventory grid and its children.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background panel
            // (Optional: Create and draw a background texture)
            
            // Draw children (slots)
            base.Draw(spriteBatch);
            
            // Draw drag texture if dragging
            if (_isDragging && _dragSource != null && _dragTexture != null)
            {
                // Get the current mouse position from Mouse.GetState() instead
                Point mousePos = Microsoft.Xna.Framework.Input.Mouse.GetState().Position;
                
                // Draw the drag texture at the mouse position with offset
                Vector2 dragPos = new Vector2(mousePos.X - _dragOffset.X, mousePos.Y - _dragOffset.Y);
                spriteBatch.Draw(_dragTexture, dragPos, Color.White * 0.8f);
            }
        }
        
        /// <summary>
        /// Creates the inventory slots.
        /// </summary>
        private void CreateSlots()
        {
            int startSlotIndex = _isHotbar ? 0 : _inventoryComponent.Inventory.HotbarSize;
            int slotCount = _isHotbar 
                ? _inventoryComponent.Inventory.HotbarSize 
                : _inventoryComponent.Inventory.Size - _inventoryComponent.Inventory.HotbarSize;
            
            int cols = _isHotbar ? slotCount : 9; // Standard inventory width
            int rows = (int)Math.Ceiling(slotCount / (float)cols);
            
            for (int i = 0; i < slotCount; i++)
            {
                int slotIndex = startSlotIndex + i;
                int row = i / cols;
                int col = i % cols;
                
                int x = Bounds.X + col * (SlotSize + SlotPadding);
                int y = Bounds.Y + row * (SlotSize + SlotPadding);
                
                var slot = new UISlot(
                    _graphicsDevice,
                    new Rectangle(x, y, SlotSize, SlotSize),
                    _font)
                {
                    InventorySlot = _inventoryComponent.Inventory.GetSlot(slotIndex),
                    SlotIndex = slotIndex
                };
                
                // Set up event handlers
                slot.Click += OnSlotClick;
                slot.ItemDragStart += OnItemDragStart;
                slot.ItemDrop += OnItemDrop;
                
                _slots.Add(slot);
                AddChild(slot);
            }
        }
        
        /// <summary>
        /// Updates all slots with current inventory data.
        /// </summary>
        private void UpdateSlots()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                int slotIndex = _slots[i].SlotIndex;
                _slots[i].InventorySlot = _inventoryComponent.Inventory.GetSlot(slotIndex);
            }
        }
        
        /// <summary>
        /// Handles inventory change events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // Update all slots with their current inventory data
            UpdateSlots();
        }
        
        /// <summary>
        /// Handles hotbar selection change events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnHotbarSelectionChanged(object sender, EventArgs e)
        {
            if (_isHotbar)
            {
                SelectedSlotIndex = _inventoryComponent.SelectedHotbarIndex;
            }
        }
        
        /// <summary>
        /// Handles slot click events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSlotClick(object sender, UIEventArgs e)
        {
            if (sender is UISlot slot)
            {
                if (_isHotbar)
                {
                    // For hotbar, update selected index
                    _inventoryComponent.SelectHotbarSlot(slot.SlotIndex);
                    SelectedSlotIndex = slot.SlotIndex;
                }
                else
                {
                    // For main inventory, handle selection and item swap
                    if (_selectedSlotIndex == -1)
                    {
                        // First slot selection
                        SelectedSlotIndex = slot.SlotIndex;
                    }
                    else if (_selectedSlotIndex == slot.SlotIndex)
                    {
                        // Deselect
                        SelectedSlotIndex = -1;
                    }
                    else
                    {
                        // Swap items between slots
                        _inventoryComponent.Inventory.SwapSlots(_selectedSlotIndex, slot.SlotIndex);
                        
                        // Deselect
                        SelectedSlotIndex = -1;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles item drag start events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemDragStart(object sender, UISlotEventArgs e)
        {
            if (sender is UISlot slot && slot.InventorySlot != null && !slot.InventorySlot.IsEmpty)
            {
                _dragSource = slot;
                
                // Create drag texture (could be a copy of the item icon)
                // This is a simplified example - in a real implementation, you'd
                // create a texture based on the actual item being dragged
                _dragTexture = new Texture2D(_graphicsDevice, SlotSize, SlotSize);
                Color[] data = new Color[SlotSize * SlotSize];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Color.White;
                }
                _dragTexture.SetData(data);
                
                // Set drag offset to center of texture
                _dragOffset = new Point(SlotSize / 2, SlotSize / 2);
                
                _isDragging = true;
            }
        }
        
        /// <summary>
        /// Handles item drop events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemDrop(object sender, UISlotEventArgs e)
        {
            if (_isDragging && _dragSource != null && sender is UISlot targetSlot)
            {
                // Swap items between source and target slots
                _inventoryComponent.Inventory.SwapSlots(_dragSource.SlotIndex, targetSlot.SlotIndex);
                
                // End dragging
                EndDragging();
            }
        }
        
        /// <summary>
        /// Ends the dragging operation.
        /// </summary>
        private void EndDragging()
        {
            _isDragging = false;
            _dragSource = null;
            
            if (_dragTexture != null)
            {
                _dragTexture.Dispose();
                _dragTexture = null;
            }
        }
        
        /// <summary>
        /// Raises the SelectionChanged event.
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _slots.Count)
            {
                UISlot selectedSlot = _slots[_selectedSlotIndex];
                SelectionChanged?.Invoke(this, new UISlotEventArgs(selectedSlot, Point.Zero));
            }
        }
        
        /// <summary>
        /// Raises the ItemUsed event.
        /// </summary>
        /// <param name="slot">The slot containing the item being used.</param>
        protected virtual void OnItemUsed(UISlot slot)
        {
            ItemUsed?.Invoke(this, new UISlotEventArgs(slot, Point.Zero));
        }
    }
}