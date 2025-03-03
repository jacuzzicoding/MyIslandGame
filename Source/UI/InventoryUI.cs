using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.ECS.Systems;
using MyIslandGame.Inventory;

namespace MyIslandGame.UI
{
    /// <summary>
    /// User interface for displaying the player's inventory and hotbar.
    /// </summary>
    public class InventoryUI
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly InventorySystem _inventorySystem;
        private readonly EntityManager _entityManager;
        
        private Entity _playerEntity;
        private InventoryComponent _playerInventory;
        
        // UI assets
        private Texture2D _slotTexture;
        private Texture2D _selectedSlotTexture;
        private Texture2D _inventoryPanelTexture;
        private SpriteFont _font;
        
        // UI dimensions
        private const int SlotSize = 40;
        private const int SlotPadding = 5;
        private const int HotbarY = 20;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryUI"/> class.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for rendering.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="inventorySystem">The inventory system.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="font">The font to use for text rendering.</param>
        public InventoryUI(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, InventorySystem inventorySystem, EntityManager entityManager, SpriteFont font)
        {
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _inventorySystem = inventorySystem ?? throw new ArgumentNullException(nameof(inventorySystem));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _font = font ?? throw new ArgumentNullException(nameof(font));
            
            _inventorySystem.InventoryVisibilityChanged += OnInventoryVisibilityChanged;
            
            CreateUITextures();
            InitializePlayerReference();
        }

        /// <summary>
        /// Updates the inventory UI.
        /// </summary>
        public void Update()
        {
            // Ensure we have a reference to the player
            if (_playerEntity == null || _playerInventory == null)
            {
                InitializePlayerReference();
                
                if (_playerEntity == null || _playerInventory == null)
                {
                    return; // Still no player, skip update
                }
            }
            
            // Handle mouse interaction with inventory slots when inventory is open
            if (_inventorySystem.InventoryVisible)
            {
                HandleInventoryInteraction();
            }
        }

        /// <summary>
        /// Draws the inventory UI.
        /// </summary>
        public void Draw()
        {
            if (_playerInventory == null)
            {
                return;
            }
            
            // Always draw hotbar
            DrawHotbar();
            
            // Draw full inventory if visible
            if (_inventorySystem.InventoryVisible)
            {
                DrawInventory();
            }
        }

        /// <summary>
        /// Creates the UI textures for inventory elements.
        /// </summary>
        private void CreateUITextures()
        {
            // Create slot texture
            _slotTexture = new Texture2D(_graphicsDevice, SlotSize, SlotSize);
            Color[] slotData = new Color[SlotSize * SlotSize];
            
            for (int i = 0; i < slotData.Length; i++)
            {
                int x = i % SlotSize;
                int y = i / SlotSize;
                
                // Create a border
                if (x == 0 || x == SlotSize - 1 || y == 0 || y == SlotSize - 1)
                {
                    slotData[i] = new Color(80, 80, 80); // Dark gray border
                }
                else
                {
                    slotData[i] = new Color(40, 40, 40, 180); // Semi-transparent gray background
                }
            }
            
            _slotTexture.SetData(slotData);
            
            // Create selected slot texture
            _selectedSlotTexture = new Texture2D(_graphicsDevice, SlotSize, SlotSize);
            Color[] selectedSlotData = new Color[SlotSize * SlotSize];
            
            for (int i = 0; i < selectedSlotData.Length; i++)
            {
                int x = i % SlotSize;
                int y = i / SlotSize;
                
                // Create a yellow border
                if (x == 0 || x == SlotSize - 1 || y == 0 || y == SlotSize - 1)
                {
                    selectedSlotData[i] = new Color(255, 215, 0); // Gold border
                }
                else if (x == 1 || x == SlotSize - 2 || y == 1 || y == SlotSize - 2)
                {
                    selectedSlotData[i] = new Color(255, 215, 0, 180); // Semi-transparent gold inner border
                }
                else
                {
                    selectedSlotData[i] = new Color(40, 40, 40, 180); // Semi-transparent gray background
                }
            }
            
            _selectedSlotTexture.SetData(selectedSlotData);
            
            // Create inventory panel texture
            int panelWidth = 280;
            int panelHeight = 200;
            _inventoryPanelTexture = new Texture2D(_graphicsDevice, panelWidth, panelHeight);
            Color[] panelData = new Color[panelWidth * panelHeight];
            
            for (int i = 0; i < panelData.Length; i++)
            {
                int x = i % panelWidth;
                int y = i / panelWidth;
                
                // Create a border
                if (x == 0 || x == panelWidth - 1 || y == 0 || y == panelHeight - 1)
                {
                    panelData[i] = new Color(80, 80, 80); // Dark gray border
                }
                else
                {
                    panelData[i] = new Color(40, 40, 40, 220); // Semi-transparent gray background
                }
            }
            
            _inventoryPanelTexture.SetData(panelData);
        }

        /// <summary>
        /// Initializes the reference to the player entity and inventory.
        /// </summary>
        private void InitializePlayerReference()
        {
            // Find player entity (assuming single player game)
            var playerEntities = _entityManager.GetEntitiesWithComponents(typeof(PlayerComponent), typeof(InventoryComponent));
            var playerEnumerator = playerEntities.GetEnumerator();
            
            if (playerEnumerator.MoveNext())
            {
                _playerEntity = playerEnumerator.Current;
                _playerInventory = _playerEntity.GetComponent<InventoryComponent>();
            }
        }

        /// <summary>
        /// Draws the hotbar at the bottom of the screen.
        /// </summary>
        private void DrawHotbar()
        {
            int hotbarSize = _playerInventory.Inventory.HotbarSize;
            int totalWidth = hotbarSize * (SlotSize + SlotPadding) - SlotPadding;
            int startX = (_graphicsDevice.Viewport.Width - totalWidth) / 2;
            int startY = _graphicsDevice.Viewport.Height - SlotSize - HotbarY;
            
            for (int i = 0; i < hotbarSize; i++)
            {
                int x = startX + i * (SlotSize + SlotPadding);
                
                // Draw slot background
                Texture2D slotTexture = (i == _playerInventory.SelectedHotbarIndex) ? _selectedSlotTexture : _slotTexture;
                _spriteBatch.Draw(slotTexture, new Rectangle(x, startY, SlotSize, SlotSize), Color.White);
                
                // Draw item if exists
                var slot = _playerInventory.Inventory.GetSlot(i);
                if (!slot.IsEmpty)
                {
                    var item = slot.Item;
                    
                    if (item.Icon != null)
                    {
                        // Center item in slot
                        int iconSize = SlotSize - 10;
                        int iconX = x + (SlotSize - iconSize) / 2;
                        int iconY = startY + (SlotSize - iconSize) / 2;
                        
                        _spriteBatch.Draw(item.Icon, new Rectangle(iconX, iconY, iconSize, iconSize), Color.White);
                        
                        // Draw quantity if more than 1
                        if (slot.Quantity > 1)
                        {
                            string quantityText = slot.Quantity.ToString();
                            Vector2 textSize = _font.MeasureString(quantityText);
                            _spriteBatch.DrawString(_font, quantityText, new Vector2(x + SlotSize - textSize.X - 2, startY + SlotSize - textSize.Y - 2), Color.White);
                        }
                    }
                }
                
                // Draw slot number
                string slotNumber = (i + 1).ToString();
                Vector2 numberSize = _font.MeasureString(slotNumber);
                _spriteBatch.DrawString(_font, slotNumber, new Vector2(x + 3, startY + 2), Color.White);
            }
        }

        /// <summary>
        /// Draws the full inventory when visible.
        /// </summary>
        private void DrawInventory()
        {
            int inventoryRows = 3;
            int inventoryCols = 9;
            int totalWidth = inventoryCols * (SlotSize + SlotPadding) - SlotPadding;
            int totalHeight = inventoryRows * (SlotSize + SlotPadding) - SlotPadding;
            int startX = (_graphicsDevice.Viewport.Width - totalWidth) / 2;
            int startY = (_graphicsDevice.Viewport.Height - totalHeight) / 2 - 20;
            
            // Draw inventory panel background
            _spriteBatch.Draw(_inventoryPanelTexture, new Rectangle(startX - 10, startY - 30, totalWidth + 20, totalHeight + 60), Color.White);
            
            // Draw inventory title
            string title = "Inventory";
            Vector2 titleSize = _font.MeasureString(title);
            _spriteBatch.DrawString(_font, title, new Vector2(startX + (totalWidth - titleSize.X) / 2, startY - 25), Color.White);
            
            // Draw inventory slots (skipping hotbar slots)
            int hotbarSize = _playerInventory.Inventory.HotbarSize;
            
            for (int row = 0; row < inventoryRows; row++)
            {
                for (int col = 0; col < inventoryCols; col++)
                {
                    int slotIndex = hotbarSize + row * inventoryCols + col;
                    
                    // Skip if beyond inventory size
                    if (slotIndex >= _playerInventory.Inventory.Size)
                    {
                        continue;
                    }
                    
                    int x = startX + col * (SlotSize + SlotPadding);
                    int y = startY + row * (SlotSize + SlotPadding);
                    
                    // Draw slot background
                    _spriteBatch.Draw(_slotTexture, new Rectangle(x, y, SlotSize, SlotSize), Color.White);
                    
                    // Draw item if exists
                    var slot = _playerInventory.Inventory.GetSlot(slotIndex);
                    if (!slot.IsEmpty)
                    {
                        var item = slot.Item;
                        
                        if (item.Icon != null)
                        {
                            // Center item in slot
                            int iconSize = SlotSize - 10;
                            int iconX = x + (SlotSize - iconSize) / 2;
                            int iconY = y + (SlotSize - iconSize) / 2;
                            
                            _spriteBatch.Draw(item.Icon, new Rectangle(iconX, iconY, iconSize, iconSize), Color.White);
                            
                            // Draw quantity if more than 1
                            if (slot.Quantity > 1)
                            {
                                string quantityText = slot.Quantity.ToString();
                                Vector2 textSize = _font.MeasureString(quantityText);
                                _spriteBatch.DrawString(_font, quantityText, new Vector2(x + SlotSize - textSize.X - 2, y + SlotSize - textSize.Y - 2), Color.White);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles mouse interaction with inventory slots.
        /// </summary>
        private void HandleInventoryInteraction()
        {
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;
            
            // Check if mouse is hovering over a slot
            int hoveredSlot = GetSlotUnderMouse(mouseX, mouseY);
            
            // Handle mouse clicks
            if (mouseState.LeftButton == ButtonState.Pressed && hoveredSlot != -1)
            {
                // Get currently selected hotbar slot
                int hotbarSlot = _playerInventory.SelectedHotbarIndex;
                
                // Swap the slots
                _playerInventory.Inventory.SwapSlots(hoveredSlot, hotbarSlot);
            }
        }

        /// <summary>
        /// Gets the inventory slot index under the mouse cursor.
        /// </summary>
        /// <param name="mouseX">The mouse X position.</param>
        /// <param name="mouseY">The mouse Y position.</param>
        /// <returns>The slot index, or -1 if no slot is under the cursor.</returns>
        private int GetSlotUnderMouse(int mouseX, int mouseY)
        {
            // Check hotbar slots
            int hotbarSize = _playerInventory.Inventory.HotbarSize;
            int totalHotbarWidth = hotbarSize * (SlotSize + SlotPadding) - SlotPadding;
            int hotbarStartX = (_graphicsDevice.Viewport.Width - totalHotbarWidth) / 2;
            int hotbarStartY = _graphicsDevice.Viewport.Height - SlotSize - HotbarY;
            
            for (int i = 0; i < hotbarSize; i++)
            {
                int x = hotbarStartX + i * (SlotSize + SlotPadding);
                
                if (mouseX >= x && mouseX < x + SlotSize && mouseY >= hotbarStartY && mouseY < hotbarStartY + SlotSize)
                {
                    return i;
                }
            }
            
            // Check inventory slots
            if (_inventorySystem.InventoryVisible)
            {
                int inventoryRows = 3;
                int inventoryCols = 9;
                int totalWidth = inventoryCols * (SlotSize + SlotPadding) - SlotPadding;
                int totalHeight = inventoryRows * (SlotSize + SlotPadding) - SlotPadding;
                int startX = (_graphicsDevice.Viewport.Width - totalWidth) / 2;
                int startY = (_graphicsDevice.Viewport.Height - totalHeight) / 2 - 20;
                
                for (int row = 0; row < inventoryRows; row++)
                {
                    for (int col = 0; col < inventoryCols; col++)
                    {
                        int slotIndex = hotbarSize + row * inventoryCols + col;
                        
                        // Skip if beyond inventory size
                        if (slotIndex >= _playerInventory.Inventory.Size)
                        {
                            continue;
                        }
                        
                        int x = startX + col * (SlotSize + SlotPadding);
                        int y = startY + row * (SlotSize + SlotPadding);
                        
                        if (mouseX >= x && mouseX < x + SlotSize && mouseY >= y && mouseY < y + SlotSize)
                        {
                            return slotIndex;
                        }
                    }
                }
            }
            
            return -1;
        }

        /// <summary>
        /// Handles inventory visibility changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="visible">True if the inventory is now visible, otherwise false.</param>
        private void OnInventoryVisibilityChanged(object sender, bool visible)
        {
            // Handle visibility change if needed
        }
    }
}
