using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.Crafting;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.Input;
using MyIslandGame.Inventory;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Handles the UI for crafting interfaces.
    /// </summary>
    public class CraftingUI
    {
        private readonly CraftingSystem _craftingSystem;
        private readonly InputManager _inputManager;
        private readonly EntityManager _entityManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly UIManager _uiManager;
        private readonly ResourceManager _resourceManager;
        
        // UI elements
        private Rectangle _craftingGridBounds;
        private Dictionary<Point, Rectangle> _slotBounds;
        private Rectangle _resultSlotBounds;
        
        // Dragging state
        private bool _isDragging;
        private Item _draggedItem;
        private int _draggedQuantity;
        private Vector2 _dragPosition;
        private Point _dragSourcePosition;
        private bool _dragSourceIsInventory;
        
        // Textures and styles
        private Texture2D _slotTexture;
        private Texture2D _highlightTexture;
        private Texture2D _backgroundTexture;
        private SpriteFont _font;
        
        // Cached recipe result
        private Recipe _currentRecipe;
        private Item _resultItem;
        private int _resultQuantity;
        
        /// <summary>
        /// Gets a value indicating whether the crafting UI is active.
        /// </summary>
        public bool IsActive => _craftingSystem.IsCraftingActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingUI"/> class.
        /// </summary>
        public CraftingUI(
            CraftingSystem craftingSystem,
            InputManager inputManager,
            EntityManager entityManager,
            UIManager uiManager,
            GraphicsDevice graphicsDevice,
            ResourceManager resourceManager)
        {
            _craftingSystem = craftingSystem ?? throw new ArgumentNullException(nameof(craftingSystem));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            
            // Subscribe to crafting events
            _craftingSystem.CraftingResultChanged += OnCraftingResultChanged;
            _craftingSystem.ItemCrafted += OnItemCrafted;
            
            // Initialize ui elements
            _slotBounds = new Dictionary<Point, Rectangle>();
            
            // Create textures
            CreateUITextures();
            
            // Initialize dragging state
            ResetDragState();
        }

        /// <summary>
        /// Loads content for the crafting UI.
        /// </summary>
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _font = content.Load<SpriteFont>("Fonts/DefaultFont");
        }

        /// <summary>
        /// Updates the crafting UI.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;
            
            // Update UI layout based on current crafting state
            UpdateLayout();
            
            // Get mouse state
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            
            if (_isDragging)
            {
                HandleDragging(mousePosition);
            }
            else
            {
                HandleMouseInteraction(mousePosition);
            }
        }

        /// <summary>
        /// Draws the crafting UI.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive)
                return;
            
            // Draw background
            spriteBatch.Draw(_backgroundTexture, _craftingGridBounds, Color.White);
            
            // Draw crafting grid slots
            DrawCraftingGrid(spriteBatch);
            
            // Draw result slot
            DrawResultSlot(spriteBatch);
            
            // Draw dragged item
            if (_isDragging && _draggedItem != null)
            {
                DrawDraggedItem(spriteBatch);
            }
        }

        #region UI Setup and Layout

        /// <summary>
        /// Creates the UI textures.
        /// </summary>
        private void CreateUITextures()
        {
            _slotTexture = CreateSlotTexture();
            _highlightTexture = CreateHighlightTexture();
            _backgroundTexture = CreateBackgroundTexture();
        }

        /// <summary>
        /// Creates a texture for inventory slots.
        /// </summary>
        private Texture2D CreateSlotTexture()
        {
            int size = 1;
            Texture2D texture = new Texture2D(_graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            data[0] = new Color(60, 60, 60, 200);
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a texture for slot highlighting.
        /// </summary>
        private Texture2D CreateHighlightTexture()
        {
            int size = 1;
            Texture2D texture = new Texture2D(_graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            data[0] = new Color(200, 200, 100, 128);
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Creates a texture for the crafting background.
        /// </summary>
        private Texture2D CreateBackgroundTexture()
        {
            int size = 1;
            Texture2D texture = new Texture2D(_graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            data[0] = new Color(30, 30, 30, 230);
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Updates the UI layout based on the current crafting state.
        /// </summary>
        private void UpdateLayout()
        {
            // Get viewport dimensions
            var viewport = _graphicsDevice.Viewport;
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;
            
            // Calculate crafting grid dimensions
            int gridWidth = _craftingSystem.CraftingGrid.Width;
            int gridHeight = _craftingSystem.CraftingGrid.Height;
            
            // Size constants
            const int SlotSize = 64;
            const int SlotMargin = 8;
            const int ResultMargin = 48;
            
            // Calculate total grid size
            int gridTotalWidth = (gridWidth * SlotSize) + ((gridWidth - 1) * SlotMargin);
            int gridTotalHeight = (gridHeight * SlotSize) + ((gridHeight - 1) * SlotMargin);
            
            // Add space for result slot
            int totalWidth = gridTotalWidth + ResultMargin + SlotSize;
            int totalHeight = Math.Max(gridTotalHeight, SlotSize);
            
            // Center in screen
            int startX = (screenWidth - totalWidth) / 2;
            int startY = (screenHeight - totalHeight) / 2;
            
            // Set crafting grid bounds
            _craftingGridBounds = new Rectangle(
                startX - 20,
                startY - 20,
                totalWidth + 40,
                totalHeight + 40
            );
            
            // Create slot bounds
            _slotBounds.Clear();
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int slotX = startX + (x * (SlotSize + SlotMargin));
                    int slotY = startY + (y * (SlotSize + SlotMargin));
                    
                    _slotBounds[new Point(x, y)] = new Rectangle(slotX, slotY, SlotSize, SlotSize);
                }
            }
            
            // Set result slot bounds
            int resultX = startX + gridTotalWidth + ResultMargin;
            int resultY = startY + ((totalHeight - SlotSize) / 2);
            _resultSlotBounds = new Rectangle(resultX, resultY, SlotSize, SlotSize);
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draws the crafting grid slots and their contents.
        /// </summary>
        private void DrawCraftingGrid(SpriteBatch spriteBatch)
        {
            // Draw each slot in the grid
            for (int y = 0; y < _craftingSystem.CraftingGrid.Height; y++)
            {
                for (int x = 0; x < _craftingSystem.CraftingGrid.Width; x++)
                {
                    Point position = new Point(x, y);
                    if (!_slotBounds.TryGetValue(position, out Rectangle bounds))
                        continue;
                    
                    // Draw slot background
                    spriteBatch.Draw(_slotTexture, bounds, Color.White);
                    
                    // Draw slot contents if not empty
                    var slot = _craftingSystem.CraftingGrid.GetSlot(x, y);
                    if (!slot.IsEmpty)
                    {
                        // Draw item icon
                        Rectangle iconBounds = new Rectangle(
                            bounds.X + 4,
                            bounds.Y + 4,
                            bounds.Width - 8,
                            bounds.Height - 8
                        );
                        
                        if (slot.Item.Icon != null)
                        {
                            spriteBatch.Draw(slot.Item.Icon, iconBounds, Color.White);
                            
                            // Draw quantity if more than 1
                            if (slot.Quantity > 1)
                            {
                                string quantityText = slot.Quantity.ToString();
                                Vector2 textSize = _font.MeasureString(quantityText);
                                
                                // Position in bottom-right of the slot
                                Vector2 textPosition = new Vector2(
                                    bounds.Right - textSize.X - 4,
                                    bounds.Bottom - textSize.Y - 2
                                );
                                
                                // Draw text with shadow
                                spriteBatch.DrawString(_font, quantityText, textPosition + new Vector2(1, 1), Color.Black);
                                spriteBatch.DrawString(_font, quantityText, textPosition, Color.White);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the result slot and item.
        /// </summary>
        private void DrawResultSlot(SpriteBatch spriteBatch)
        {
            // Draw result slot background with highlight if result available
            Color slotColor = _resultItem != null ? Color.Gold : Color.White;
            spriteBatch.Draw(_slotTexture, _resultSlotBounds, slotColor);
            
            // Draw arrow pointing to result
            Rectangle arrowBounds = new Rectangle(
                _resultSlotBounds.X - 40,
                _resultSlotBounds.Y + (_resultSlotBounds.Height / 2) - 10,
                30,
                20
            );
            
            // Simple arrow using lines (in a full implementation, use an arrow texture)
            // This is just a placeholder
            
            // Draw result item if available
            if (_resultItem != null && _resultItem.Icon != null)
            {
                // Draw item icon
                Rectangle iconBounds = new Rectangle(
                    _resultSlotBounds.X + 4,
                    _resultSlotBounds.Y + 4,
                    _resultSlotBounds.Width - 8,
                    _resultSlotBounds.Height - 8
                );
                
                spriteBatch.Draw(_resultItem.Icon, iconBounds, Color.White);
                
                // Draw quantity if more than 1
                if (_resultQuantity > 1)
                {
                    string quantityText = _resultQuantity.ToString();
                    Vector2 textSize = _font.MeasureString(quantityText);
                    
                    // Position in bottom-right of the slot
                    Vector2 textPosition = new Vector2(
                        _resultSlotBounds.Right - textSize.X - 4,
                        _resultSlotBounds.Bottom - textSize.Y - 2
                    );
                    
                    // Draw text with shadow
                    spriteBatch.DrawString(_font, quantityText, textPosition + new Vector2(1, 1), Color.Black);
                    spriteBatch.DrawString(_font, quantityText, textPosition, Color.White);
                }
            }
        }

        /// <summary>
        /// Draws the item being dragged.
        /// </summary>
        private void DrawDraggedItem(SpriteBatch spriteBatch)
        {
            if (_draggedItem?.Icon == null)
                return;
            
            // Draw item centered on cursor
            int size = 48;
            Rectangle iconBounds = new Rectangle(
                (int)_dragPosition.X - (size / 2),
                (int)_dragPosition.Y - (size / 2),
                size,
                size
            );
            
            spriteBatch.Draw(_draggedItem.Icon, iconBounds, Color.White * 0.8f);
            
            // Draw quantity if more than 1
            if (_draggedQuantity > 1)
            {
                string quantityText = _draggedQuantity.ToString();
                Vector2 textSize = _font.MeasureString(quantityText);
                
                // Position in bottom-right of the icon
                Vector2 textPosition = new Vector2(
                    iconBounds.Right - textSize.X,
                    iconBounds.Bottom - textSize.Y
                );
                
                // Draw text with shadow
                spriteBatch.DrawString(_font, quantityText, textPosition + new Vector2(1, 1), Color.Black);
                spriteBatch.DrawString(_font, quantityText, textPosition, Color.White);
            }
        }

        #endregion

        #region Mouse Interaction

        /// <summary>
        /// Handles mouse interaction when not dragging.
        /// </summary>
        private void HandleMouseInteraction(Point mousePosition)
        {
            // Check for interaction start (mouse press)
            if (_inputManager.IsActionTriggered("Craft"))
            {
                // Check if clicking on a crafting grid slot
                foreach (var kvp in _slotBounds)
                {
                    if (kvp.Value.Contains(mousePosition))
                    {
                        Point gridPosition = kvp.Key;
                        var slot = _craftingSystem.CraftingGrid.GetSlot(gridPosition.X, gridPosition.Y);
                        
                        if (!slot.IsEmpty)
                        {
                            // Start dragging item from crafting grid
                            StartDragging(slot.Item, slot.Quantity, gridPosition, false);
                            
                            // Remove item from grid
                            slot.Clear();
                            return;
                        }
                    }
                }
                
                // Check if clicking on result slot
                if (_resultSlotBounds.Contains(mousePosition) && _resultItem != null)
                {
                    // Try to craft and take the result
                    if (_craftingSystem.TryCraft())
                    {
                        Console.WriteLine("Item crafted successfully");
                    }
                    return;
                }
                
                // Check if clicking on inventory slot (this would be handled in a more complete implementation)
                HandleInventorySlotInteraction(mousePosition);
            }
        }

        /// <summary>
        /// Handles inventory slot interaction (simplified implementation).
        /// </summary>
        private void HandleInventorySlotInteraction(Point mousePosition)
        {
            // In a full implementation, this would check against inventory slot positions
            // and start dragging items from inventory to crafting grid
            
            // For now, this is a placeholder for the inventory interaction logic
        }

        /// <summary>
        /// Handles dragging interaction.
        /// </summary>
        private void HandleDragging(Point mousePosition)
        {
            // Update drag position
            _dragPosition = new Vector2(mousePosition.X, mousePosition.Y);
            
            // Check for drag end (mouse release)
            if (_inputManager.IsActionReleased("Craft"))
            {
                // Try to place the dragged item
                bool placed = TryPlaceDraggedItem(mousePosition);
                
                if (!placed)
                {
                    // Return item to source or player inventory
                    ReturnDraggedItem();
                }
                
                // Reset drag state
                ResetDragState();
            }
        }

        /// <summary>
        /// Starts dragging an item.
        /// </summary>
        private void StartDragging(Item item, int quantity, Point sourcePosition, bool fromInventory)
        {
            _isDragging = true;
            _draggedItem = item;
            _draggedQuantity = quantity;
            _dragPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _dragSourcePosition = sourcePosition;
            _dragSourceIsInventory = fromInventory;
        }

        /// <summary>
        /// Attempts to place the dragged item at the mouse position.
        /// </summary>
        private bool TryPlaceDraggedItem(Point mousePosition)
        {
            // Check if dropping on a crafting grid slot
            foreach (var kvp in _slotBounds)
            {
                if (kvp.Value.Contains(mousePosition))
                {
                    Point gridPosition = kvp.Key;
                    
                    // Try to place in crafting grid
                    bool placed = _craftingSystem.PlaceItemInGrid(
                        _draggedItem,
                        _draggedQuantity,
                        gridPosition.X,
                        gridPosition.Y
                    );
                    
                    return placed;
                }
            }
            
            // Not placed on a valid target
            return false;
        }

        /// <summary>
        /// Returns the dragged item to the player's inventory or original source.
        /// </summary>
        private void ReturnDraggedItem()
        {
            if (_draggedItem == null || _draggedQuantity <= 0)
                return;
            
            var playerEntity = GetPlayerEntity();
            if (playerEntity == null)
                return;
            
            var inventoryComponent = playerEntity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null)
                return;
            
            // Add item back to player inventory
            inventoryComponent.Inventory.TryAddItem(_draggedItem, _draggedQuantity);
        }

        /// <summary>
        /// Resets the dragging state.
        /// </summary>
        private void ResetDragState()
        {
            _isDragging = false;
            _draggedItem = null;
            _draggedQuantity = 0;
            _dragSourcePosition = Point.Zero;
            _dragSourceIsInventory = false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the player entity.
        /// </summary>
        private Entity GetPlayerEntity()
        {
            var playerEntities = _entityManager.GetEntitiesWithComponents(typeof(PlayerComponent), typeof(InventoryComponent));
            var enumerator = playerEntities.GetEnumerator();
            
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            
            return null;
        }

        /// <summary>
        /// Handles the crafting result changed event.
        /// </summary>
        private void OnCraftingResultChanged(object sender, CraftingResultChangedEventArgs e)
        {
            _currentRecipe = e.Recipe;
            
            if (_currentRecipe != null && _currentRecipe.Results.Count > 0)
            {
                string resultItemId = _currentRecipe.Results[0].ItemId;
                
                // Create result item
                if (_resourceManager.TryGetResource(resultItemId, out var resource))
                {
                    _resultItem = Item.FromResource(resource);
                    _resultQuantity = _currentRecipe.Results[0].Quantity;
                }
                else
                {
                    _resultItem = null;
                    _resultQuantity = 0;
                }
            }
            else
            {
                _resultItem = null;
                _resultQuantity = 0;
            }
        }

        /// <summary>
        /// Handles the item crafted event.
        /// </summary>
        private void OnItemCrafted(object sender, CraftingEventArgs e)
        {
            // Update crafting result
            OnCraftingResultChanged(sender, new CraftingResultChangedEventArgs(null));
        }

        #endregion
    }
}
