using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.Crafting;
using MyIslandGame.Core.Resources;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.Input;
using MyIslandGame.Inventory;

namespace MyIslandGame.UI
{
    /// <summary>
    /// Implements a modern CraftingUI that follows the IUIElement architecture.
    /// This implementation properly handles rendering, input, and state management
    /// for the crafting interface.
    /// </summary>
    public class ModernCraftingUI : BaseUIElement
    {
        private readonly CraftingSystem _craftingSystem;
        private readonly InputManager _inputManager;
        private readonly EntityManager _entityManager;
        private readonly ResourceManager _resourceManager;
        
        // UI elements
        private Rectangle _craftingGridBounds;
        private Dictionary<Point, Rectangle> _slotBounds;
        private Rectangle _resultSlotBounds;
        
        // Textures and fonts
        private Texture2D _slotTexture;
        private Texture2D _slotHighlightTexture;
        private Texture2D _backgroundTexture;
        private Texture2D _arrowTexture;
        private SpriteFont _font;
        
        // Dragging state
        private bool _isDragging;
        private Item _draggedItem;
        private int _draggedQuantity;
        private Vector2 _dragPosition;
        private Point _dragSourcePosition;
        private bool _dragSourceIsInventory;
        
        // Interaction state
        private Point? _hoveredSlot;
        private bool _isResultHovered;
        
        // Cached recipe result
        private Recipe _currentRecipe;
        private Item _resultItem;
        private int _resultQuantity;
        
        // Debug/logging info
        private bool _isInitialized;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernCraftingUI"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="craftingSystem">The crafting system.</param>
        /// <param name="inputManager">The input manager.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="resourceManager">The resource manager.</param>
        /// <param name="font">The font to use for text rendering.</param>
        public ModernCraftingUI(
            GraphicsDevice graphicsDevice, 
            CraftingSystem craftingSystem,
            InputManager inputManager,
            EntityManager entityManager,
            ResourceManager resourceManager,
            SpriteFont font)
            : base(graphicsDevice)
        {
            _craftingSystem = craftingSystem ?? throw new ArgumentNullException(nameof(craftingSystem));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            _font = font;
            
            // Set to foreground layer for proper rendering order
            Layer = UILayer.Foreground;
            
            // Initialize slot bounds dictionary
            _slotBounds = new Dictionary<Point, Rectangle>();
            
            // Initially not visible - will be synced with crafting system
            IsVisible = false;
            IsActive = false;
            
            // Subscribe to crafting events
            _craftingSystem.CraftingResultChanged += OnCraftingResultChanged;
            _craftingSystem.ItemCrafted += OnItemCrafted;
            _craftingSystem.CraftingStateChanged += OnCraftingStateChanged;
            
            Console.WriteLine("ModernCraftingUI: Constructor completed");
        }
        
        /// <summary>
        /// Initializes this UI element by creating required textures and setting up initial state.
        /// </summary>
        public override void Initialize()
        {
            Console.WriteLine("ModernCraftingUI: Initializing...");
            
            // Create textures only once during initialization
            _slotTexture = CreateColorTexture(1, 1, new Color(60, 60, 60, 200));
            _slotHighlightTexture = CreateColorTexture(1, 1, new Color(200, 200, 100, 128));
            _backgroundTexture = CreateColorTexture(1, 1, new Color(30, 30, 30, 230));
            
            // Create an arrow texture
            _arrowTexture = CreateArrowTexture();
            
            // Reset drag state
            ResetDragState();
            
            // Update layout
            UpdateLayout();
            
            _isInitialized = true;
            Console.WriteLine("ModernCraftingUI: Initialization complete");
        }
        
        /// <summary>
        /// Updates this UI element.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            bool wasVisible = IsVisible;
            bool wasActive = IsActive;
            
            // Update visibility and activity based on crafting system state
            IsVisible = _craftingSystem.IsCraftingActive;
            IsActive = _craftingSystem.IsCraftingActive;
            
            // Log state changes for debugging
            if (wasVisible != IsVisible || wasActive != IsActive)
            {
                Console.WriteLine($"ModernCraftingUI: Visibility={IsVisible}, Active={IsActive}, CraftingSystem.IsCraftingActive={_craftingSystem.IsCraftingActive}");
            }
            
            if (!IsVisible || !IsActive)
                return;
            
            // Update layout based on current crafting state
            UpdateLayout();
            
            // Get mouse state
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            
            // Update hover state
            UpdateHoverState(mousePosition);
            
            // Handle dragging or interaction
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
        /// Draws this UI element.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null)
            {
                Console.WriteLine("ModernCraftingUI.Draw: SpriteBatch is null!");
                return;
            }
            
            if (!IsVisible)
            {
                Console.WriteLine("ModernCraftingUI.Draw: Not visible, skipping drawing");
                return;
            }
            
            if (!_isInitialized)
            {
                Console.WriteLine("ModernCraftingUI.Draw: Not initialized, skipping drawing");
                return;
            }
            
            Console.WriteLine("ModernCraftingUI.Draw: Drawing UI!");
            
            // Draw background
            spriteBatch.Draw(_backgroundTexture, _craftingGridBounds, Color.White);
            
            // Draw title based on station type
            if (_font != null)
            {
                string title = _craftingSystem.CurrentStation == CraftingStationType.None ? 
                    "Basic Crafting (2x2)" : 
                    $"{_craftingSystem.CurrentStation} (3x3)";
                
                Vector2 textSize = _font.MeasureString(title);
                Vector2 position = new Vector2(
                    _craftingGridBounds.X + (_craftingGridBounds.Width - textSize.X) / 2,
                    _craftingGridBounds.Y - textSize.Y - 5
                );
                
                // Draw text with shadow
                DrawTextWithShadow(spriteBatch, _font, title, position, Color.White);
            }
            
            // Draw crafting grid slots
            DrawCraftingGrid(spriteBatch);
            
            // Draw arrow pointing to result
            int arrowWidth = 30;
            int arrowHeight = 20;
            Rectangle arrowBounds = new Rectangle(
                _resultSlotBounds.X - arrowWidth - 10,
                _resultSlotBounds.Y + (_resultSlotBounds.Height / 2) - (arrowHeight / 2),
                arrowWidth,
                arrowHeight
            );
            
            spriteBatch.Draw(_arrowTexture, arrowBounds, Color.White);
            
            // Draw result slot
            DrawResultSlot(spriteBatch);
            
            // Draw dragged item
            if (_isDragging && _draggedItem != null)
            {
                DrawDraggedItem(spriteBatch);
            }
        }
        
        /// <summary>
        /// Handles input for this UI element.
        /// </summary>
        /// <param name="inputManager">The input manager.</param>
        /// <returns>True if input was handled, otherwise false.</returns>
        public override bool HandleInput(InputManager inputManager)
        {
            if (!IsActive || !IsVisible)
                return false;
                
            // Check if mouse is over our UI bounds
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            
            // If mouse is within the crafting UI, consider input handled
            // even if we don't specifically use it
            if (_craftingGridBounds.Contains(mousePosition) || 
                (_isDragging && _draggedItem != null))
            {
                return true;
            }
            
            // Also handle clicks on any slot
            foreach (var slotBounds in _slotBounds.Values)
            {
                if (slotBounds.Contains(mousePosition))
                {
                    return true;
                }
            }
            
            // And handle clicks on the result slot
            if (_resultSlotBounds.Contains(mousePosition))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Updates hover state for slots and result.
        /// </summary>
        /// <param name="mousePosition">Current mouse position.</param>
        private void UpdateHoverState(Point mousePosition)
        {
            // Check slots
            _hoveredSlot = null;
            foreach (var kvp in _slotBounds)
            {
                if (kvp.Value.Contains(mousePosition))
                {
                    _hoveredSlot = kvp.Key;
                    break;
                }
            }
            
            // Check result slot
            _isResultHovered = _resultSlotBounds.Contains(mousePosition);
        }
        
        /// <summary>
        /// Draws the crafting grid and its contents.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
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
                    
                    // Determine if this slot is hovered
                    bool isHovered = _hoveredSlot.HasValue && _hoveredSlot.Value == position;
                    
                    // Draw slot background (use highlight color if hovered)
                    Color slotColor = isHovered ? Color.Gold : Color.White;
                    spriteBatch.Draw(_slotTexture, bounds, slotColor);
                    
                    if (isHovered)
                    {
                        // Draw slot highlight
                        Rectangle highlightBounds = new Rectangle(
                            bounds.X - 2, bounds.Y - 2, 
                            bounds.Width + 4, bounds.Height + 4);
                        spriteBatch.Draw(_slotHighlightTexture, highlightBounds, Color.White * 0.5f);
                    }
                    
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
                            
                            // Draw quantity if more than 1 and font is available
                            if (slot.Quantity > 1 && _font != null)
                            {
                                DrawItemQuantity(spriteBatch, slot.Quantity, bounds);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws the result slot and its contents.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        private void DrawResultSlot(SpriteBatch spriteBatch)
        {
            // Determine color based on hover state and if result is available
            Color slotColor = Color.White;
            if (_isResultHovered)
            {
                slotColor = Color.Gold;
            }
            else if (_resultItem != null)
            {
                slotColor = new Color(220, 220, 150);
            }
            
            // Draw result slot background
            spriteBatch.Draw(_slotTexture, _resultSlotBounds, slotColor);
            
            // Draw highlight if hovered
            if (_isResultHovered)
            {
                Rectangle highlightBounds = new Rectangle(
                    _resultSlotBounds.X - 2, _resultSlotBounds.Y - 2,
                    _resultSlotBounds.Width + 4, _resultSlotBounds.Height + 4);
                spriteBatch.Draw(_slotHighlightTexture, highlightBounds, Color.White * 0.5f);
            }
            
            // Draw result item if available
            if (_resultItem != null && _resultItem.Icon != null)
            {
                // Calculate bounds with padding
                Rectangle iconBounds = new Rectangle(
                    _resultSlotBounds.X + 4,
                    _resultSlotBounds.Y + 4,
                    _resultSlotBounds.Width - 8,
                    _resultSlotBounds.Height - 8
                );
                
                // Draw the item icon
                spriteBatch.Draw(_resultItem.Icon, iconBounds, Color.White);
                
                // Draw quantity if more than 1 and font is available
                if (_resultQuantity > 1 && _font != null)
                {
                    DrawItemQuantity(spriteBatch, _resultQuantity, _resultSlotBounds);
                }
            }
        }
        
        /// <summary>
        /// Draws the item quantity text in the bottom-right corner of a slot.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="quantity">The quantity to display.</param>
        /// <param name="slotBounds">The bounds of the slot.</param>
        private void DrawItemQuantity(SpriteBatch spriteBatch, int quantity, Rectangle slotBounds)
        {
            string quantityText = quantity.ToString();
            Vector2 textSize = _font.MeasureString(quantityText);
            
            // Position in bottom-right of the slot
            Vector2 textPosition = new Vector2(
                slotBounds.Right - textSize.X - 4,
                slotBounds.Bottom - textSize.Y - 2
            );
            
            // Draw text with shadow
            DrawTextWithShadow(spriteBatch, _font, quantityText, textPosition, Color.White);
        }
        
        /// <summary>
        /// Draws the item being dragged.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
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
            
            // Draw the item with slight transparency
            spriteBatch.Draw(_draggedItem.Icon, iconBounds, Color.White * 0.8f);
            
            // Draw quantity if more than 1 and font is available
            if (_draggedQuantity > 1 && _font != null)
            {
                DrawItemQuantity(spriteBatch, _draggedQuantity, iconBounds);
            }
        }
        
        /// <summary>
        /// Updates the layout of the crafting UI based on the current crafting grid size.
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
            
            // Update the overall bounds property
            Bounds = _craftingGridBounds;
            
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
        
        /// <summary>
        /// Handles mouse interaction when not dragging.
        /// </summary>
        /// <param name="mousePosition">The current mouse position.</param>
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
        /// <param name="mousePosition">The current mouse position.</param>
        private void HandleInventorySlotInteraction(Point mousePosition)
        {
            // In a full implementation, this would check against inventory slot positions
            // and start dragging items from inventory to crafting grid
            
            // For now, this is a placeholder for the inventory interaction logic
        }
        
        /// <summary>
        /// Handles dragging interaction.
        /// </summary>
        /// <param name="mousePosition">The current mouse position.</param>
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
        /// <param name="item">The item to drag.</param>
        /// <param name="quantity">The quantity of items to drag.</param>
        /// <param name="sourcePosition">The source position in the grid.</param>
        /// <param name="fromInventory">Whether the item is from the inventory.</param>
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
        /// <param name="mousePosition">The current mouse position.</param>
        /// <returns>True if the item was placed, otherwise false.</returns>
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
        
        /// <summary>
        /// Gets the player entity.
        /// </summary>
        /// <returns>The player entity.</returns>
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
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemCrafted(object sender, CraftingEventArgs e)
        {
            // Update crafting result
            OnCraftingResultChanged(sender, new CraftingResultChangedEventArgs(null));
        }
        
        /// <summary>
        /// Handles changes to the crafting system state.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCraftingStateChanged(object sender, CraftingStateChangedEventArgs e)
        {
            Console.WriteLine($"ModernCraftingUI: CraftingStateChanged event received - IsActive={e.IsActive}, StationType={e.StationType}");
            
            // Update visibility based on crafting state
            IsVisible = e.IsActive;
            IsActive = e.IsActive;
            
            // If closing crafting, reset drag state
            if (!e.IsActive && _isDragging)
            {
                ReturnDraggedItem();
                ResetDragState();
            }
            
            // Force a layout update immediately
            UpdateLayout();
        }
        
        /// <summary>
        /// Creates a texture for a right-pointing arrow.
        /// </summary>
        /// <returns>The created arrow texture.</returns>
        private Texture2D CreateArrowTexture()
        {
            // Create a simple right-pointing arrow texture
            int width = 30;
            int height = 20;
            Texture2D texture = new Texture2D(_graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Fill with transparent
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            // Draw arrow shape (simple right-pointing triangle)
            for (int y = 0; y < height; y++)
            {
                // Calculate width of arrow at this height
                // Creates a triangle shape pointing right
                int halfHeight = height / 2;
                int distFromCenter = Math.Abs(y - halfHeight);
                int lineWidth = width - (distFromCenter * 2);
                
                for (int x = 0; x < lineWidth; x++)
                {
                    int index = y * width + x;
                    data[index] = Color.White;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
    }
}
