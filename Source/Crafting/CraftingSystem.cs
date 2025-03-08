using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyIslandGame.Core.Resources;
using MyIslandGame.ECS;
using MyIslandGame.ECS.Components;
using MyIslandGame.Inventory;
using MyIslandGame.Input;

namespace MyIslandGame.Crafting
{
    /// <summary>
    /// System for handling crafting interactions.
    /// </summary>
    public class CraftingSystem : ECS.System
    {
        private readonly RecipeManager _recipeManager;
        private readonly InputManager _inputManager;
        private readonly ResourceManager _resourceManager;
        
        // Current crafting state
        private CraftingGrid _craftingGrid;
        private CraftingStationType _currentStation;
        private bool _isCraftingActive;
        
        /// <summary>
        /// Gets the current crafting grid.
        /// </summary>
        public CraftingGrid CraftingGrid => _craftingGrid;
        
        /// <summary>
        /// Gets the current crafting station type.
        /// </summary>
        public CraftingStationType CurrentStation => _currentStation;
        
        /// <summary>
        /// Gets a value indicating whether crafting is currently active.
        /// </summary>
        public bool IsCraftingActive => _isCraftingActive;
        
        /// <summary>
        /// Event raised when an item is crafted.
        /// </summary>
        public event EventHandler<CraftingEventArgs> ItemCrafted;
        
        /// <summary>
        /// Event raised when a crafting result changes.
        /// </summary>
        public event EventHandler<CraftingResultChangedEventArgs> CraftingResultChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingSystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="recipeManager">The recipe manager.</param>
        /// <param name="inputManager">The input manager.</param>
        /// <param name="resourceManager">The resource manager.</param>
        public CraftingSystem(EntityManager entityManager, RecipeManager recipeManager, InputManager inputManager, ResourceManager resourceManager)
            : base(entityManager)
        {
            _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            
            // Create default 2x2 crafting grid
            _craftingGrid = new CraftingGrid(2, 2);
            _currentStation = CraftingStationType.None;
            _isCraftingActive = false;
            
            // Subscribe to grid changes
            _craftingGrid.GridChanged += OnCraftingGridChanged;
        }

        /// <summary>
        /// Initializes this system.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            // Register input actions
            _inputManager.RegisterAction("OpenCrafting", new InputAction().MapKey(Microsoft.Xna.Framework.Input.Keys.C));
            _inputManager.RegisterAction("CloseCrafting", new InputAction().MapKey(Microsoft.Xna.Framework.Input.Keys.Escape));
            _inputManager.RegisterAction("Craft", new InputAction().MapMouseButton(MouseButton.Left));
            
            Console.WriteLine("CraftingSystem initialized");
        }

        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Toggle crafting mode
            if (_inputManager.IsActionTriggered("OpenCrafting") && !_isCraftingActive)
            {
                OpenCrafting(CraftingStationType.None);
            }
            else if (_inputManager.IsActionTriggered("CloseCrafting") && _isCraftingActive)
            {
                CloseCrafting();
            }
            
            // Process crafting interaction if active
            if (_isCraftingActive)
            {
                // Check for crafting action
                if (_inputManager.IsActionTriggered("Craft"))
                {
                    // Check if mouse is over result slot
                    // UI handling would provide this information in actual implementation
                    bool isOverResultSlot = true; // Placeholder
                    
                    if (isOverResultSlot)
                    {
                        TryCraft();
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this system is interested in the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the system is interested, otherwise false.</returns>
        public override bool IsInterestedIn(Entity entity)
        {
            // This system doesn't operate on specific entities directly
            return false;
        }

        /// <summary>
        /// Opens the crafting interface with the specified station type.
        /// </summary>
        /// <param name="stationType">The crafting station type.</param>
        public void OpenCrafting(CraftingStationType stationType)
        {
            // Change grid size based on station type
            if (stationType == CraftingStationType.None)
            {
                _craftingGrid = new CraftingGrid(2, 2);
            }
            else if (stationType == CraftingStationType.CraftingTable)
            {
                _craftingGrid = new CraftingGrid(3, 3);
            }
            else if (stationType == CraftingStationType.Furnace)
            {
                _craftingGrid = new CraftingGrid(1, 1); // Smelting only needs one input
            }
            
            _currentStation = stationType;
            _isCraftingActive = true;
            
            // Subscribe to grid changes for the new grid
            _craftingGrid.GridChanged += OnCraftingGridChanged;
            
            Console.WriteLine($"Crafting opened with station: {stationType}");
        }

        /// <summary>
        /// Closes the crafting interface and returns items to the player's inventory.
        /// </summary>
        public void CloseCrafting()
        {
            // First, find player inventory
            var playerEntities = EntityManager.GetEntitiesWithComponents(typeof(PlayerComponent), typeof(InventoryComponent));
            var enumerator = playerEntities.GetEnumerator();
            
            if (enumerator.MoveNext())
            {
                var inventoryComponent = enumerator.Current.GetComponent<InventoryComponent>();
                
                // Return all items in the grid to the player's inventory
                foreach (var slot in _craftingGrid.GetAllSlots())
                {
                    if (slot.Item != null)
                    {
                        inventoryComponent.Inventory.TryAddItem(slot.Item, slot.Quantity);
                        slot.Clear();
                    }
                }
            }
            
            // Unsubscribe from grid changes
            _craftingGrid.GridChanged -= OnCraftingGridChanged;
            
            _isCraftingActive = false;
            Console.WriteLine("Crafting closed");
        }

        /// <summary>
        /// Places an item in the crafting grid.
        /// </summary>
        /// <param name="item">The item to place.</param>
        /// <param name="quantity">The quantity to place.</param>
        /// <param name="x">The x-coordinate in the grid.</param>
        /// <param name="y">The y-coordinate in the grid.</param>
        /// <returns>True if the item was placed successfully, otherwise false.</returns>
        public bool PlaceItemInGrid(Item item, int quantity, int x, int y)
        {
            if (!_isCraftingActive || item == null || quantity <= 0 || 
                x < 0 || x >= _craftingGrid.Width || y < 0 || y >= _craftingGrid.Height)
            {
                return false;
            }
            
            return _craftingGrid.AddItem(item, quantity, x, y);
        }

        /// <summary>
        /// Removes an item from the crafting grid.
        /// </summary>
        /// <param name="x">The x-coordinate in the grid.</param>
        /// <param name="y">The y-coordinate in the grid.</param>
        /// <returns>The removed item, or null if no item was removed.</returns>
        public (Item Item, int Quantity) RemoveItemFromGrid(int x, int y)
        {
            if (!_isCraftingActive || x < 0 || x >= _craftingGrid.Width || y < 0 || y >= _craftingGrid.Height)
            {
                return (null, 0);
            }
            
            var slot = _craftingGrid.GetSlot(x, y);
            if (slot.Item == null)
            {
                return (null, 0);
            }
            
            var item = slot.Item;
            var quantity = slot.Quantity;
            slot.Clear();
            
            // Update crafting result
            UpdateCraftingResult();
            
            return (item, quantity);
        }

        /// <summary>
        /// Attempts to craft the current recipe.
        /// </summary>
        /// <returns>True if crafting was successful, otherwise false.</returns>
        public bool TryCraft()
        {
            if (!_isCraftingActive)
            {
                return false;
            }
            
            // Get current recipe match
            Item[,] gridItems = _craftingGrid.GetItemGrid();
            Recipe recipe = _recipeManager.MatchRecipe(gridItems, _currentStation);
            
            if (recipe == null)
            {
                return false;
            }
            
            // Find player inventory
            var playerEntities = EntityManager.GetEntitiesWithComponents(typeof(PlayerComponent), typeof(InventoryComponent));
            var enumerator = playerEntities.GetEnumerator();
            
            if (!enumerator.MoveNext())
            {
                return false;
            }
            
            var inventoryComponent = enumerator.Current.GetComponent<InventoryComponent>();
            var inventory = inventoryComponent.Inventory;
            
            // Add result items to inventory
            bool allResultsAdded = true;
            List<(string ItemId, int Quantity)> addedItems = new List<(string, int)>();
            
            foreach (var result in recipe.Results)
            {
                // Create item from resource
                if (_resourceManager.TryGetResource(result.ItemId, out var resource))
                {
                    Item item = Item.FromResource(resource);
                    if (!inventory.TryAddItem(item, result.Quantity))
                    {
                        allResultsAdded = false;
                        break;
                    }
                    
                    addedItems.Add((result.ItemId, result.Quantity));
                }
                else
                {
                    allResultsAdded = false;
                    break;
                }
            }
            
            // If we couldn't add all results, roll back
            if (!allResultsAdded)
            {
                foreach (var (itemId, quantity) in addedItems)
                {
                    inventory.RemoveItem(itemId, quantity);
                }
                
                return false;
            }
            
            // Consume grid items
            _craftingGrid.Clear();
            
            // Raise event
            OnItemCrafted(recipe);
            
            // Update crafting result
            UpdateCraftingResult();
            
            return true;
        }

        /// <summary>
        /// Updates the crafting result based on the current grid contents.
        /// </summary>
        private void UpdateCraftingResult()
        {
            if (!_isCraftingActive)
            {
                return;
            }
            
            // Get current recipe match
            Item[,] gridItems = _craftingGrid.GetItemGrid();
            Recipe recipe = _recipeManager.MatchRecipe(gridItems, _currentStation);
            
            // Raise event with result
            OnCraftingResultChanged(recipe);
        }

        /// <summary>
        /// Handles changes to the crafting grid.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCraftingGridChanged(object sender, EventArgs e)
        {
            UpdateCraftingResult();
        }

        /// <summary>
        /// Raises the ItemCrafted event.
        /// </summary>
        /// <param name="recipe">The crafted recipe.</param>
        protected virtual void OnItemCrafted(Recipe recipe)
        {
            ItemCrafted?.Invoke(this, new CraftingEventArgs(recipe));
        }

        /// <summary>
        /// Raises the CraftingResultChanged event.
        /// </summary>
        /// <param name="recipe">The current recipe match.</param>
        protected virtual void OnCraftingResultChanged(Recipe recipe)
        {
            CraftingResultChanged?.Invoke(this, new CraftingResultChangedEventArgs(recipe));
        }

        public void ToggleCrafting()
        {
            if (_isCraftingActive)
                CloseCrafting();
            else
                OpenCrafting(CraftingStationType.None);
        }
    }

    /// <summary>
    /// Represents a grid for item placement in crafting.
    /// </summary>
    public class CraftingGrid
    {
        private readonly CraftingSlot[,] _slots;
        
        /// <summary>
        /// Gets the width of the grid.
        /// </summary>
        public int Width => _slots.GetLength(1);
        
        /// <summary>
        /// Gets the height of the grid.
        /// </summary>
        public int Height => _slots.GetLength(0);
        
        /// <summary>
        /// Event raised when the grid contents change.
        /// </summary>
        public event EventHandler GridChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingGrid"/> class.
        /// </summary>
        /// <param name="width">The width of the grid.</param>
        /// <param name="height">The height of the grid.</param>
        public CraftingGrid(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Grid dimensions must be positive");
            
            _slots = new CraftingSlot[height, width];
            
            // Initialize slots
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _slots[y, x] = new CraftingSlot();
                    
                    // Subscribe to slot changes
                    int slotX = x;
                    int slotY = y;
                    _slots[y, x].SlotChanged += (sender, e) => OnSlotChanged(slotX, slotY);
                }
            }
        }

        /// <summary>
        /// Gets a slot at the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The crafting slot.</returns>
        public CraftingSlot GetSlot(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Position is outside the grid");
            
            return _slots[y, x];
        }

        /// <summary>
        /// Gets all slots in the grid.
        /// </summary>
        /// <returns>All crafting slots.</returns>
        public IEnumerable<CraftingSlot> GetAllSlots()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return _slots[y, x];
                }
            }
        }

        /// <summary>
        /// Gets a 2D array of items in the grid.
        /// </summary>
        /// <returns>2D array of items (null for empty slots).</returns>
        public Item[,] GetItemGrid()
        {
            Item[,] items = new Item[Height, Width];
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    items[y, x] = _slots[y, x].Item;
                }
            }
            
            return items;
        }

        /// <summary>
        /// Adds an item to a specific position in the grid.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True if the item was added successfully, otherwise false.</returns>
        public bool AddItem(Item item, int quantity, int x, int y)
        {
            if (item == null || quantity <= 0 || x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            
            var slot = _slots[y, x];
            
            // If slot is empty or has same item
            if (slot.Item == null || (slot.Item.CanStack(item) && !slot.IsFull))
            {
                int added = slot.AddItem(item, quantity);
                return added > 0;
            }
            
            return false;
        }

        /// <summary>
        /// Clears all items from the grid.
        /// </summary>
        public void Clear()
        {
            foreach (var slot in GetAllSlots())
            {
                slot.Clear();
            }
            
            OnGridChanged();
        }

        /// <summary>
        /// Handles changes to individual slots.
        /// </summary>
        /// <param name="x">The x-coordinate of the changed slot.</param>
        /// <param name="y">The y-coordinate of the changed slot.</param>
        private void OnSlotChanged(int x, int y)
        {
            OnGridChanged();
        }

        /// <summary>
        /// Raises the GridChanged event.
        /// </summary>
        protected virtual void OnGridChanged()
        {
            GridChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Represents a slot in the crafting grid.
    /// </summary>
    public class CraftingSlot
    {
        /// <summary>
        /// Gets the item in this slot.
        /// </summary>
        public Item Item { get; private set; }
        
        /// <summary>
        /// Gets the quantity of the item in this slot.
        /// </summary>
        public int Quantity { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether this slot is empty.
        /// </summary>
        public bool IsEmpty => Item == null || Quantity <= 0;
        
        /// <summary>
        /// Gets a value indicating whether this slot is full.
        /// </summary>
        public bool IsFull => !IsEmpty && Quantity >= Item.MaxStackSize;
        
        /// <summary>
        /// Event raised when the slot contents change.
        /// </summary>
        public event EventHandler SlotChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingSlot"/> class.
        /// </summary>
        public CraftingSlot()
        {
            Item = null;
            Quantity = 0;
        }

        /// <summary>
        /// Adds an item to this slot.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <returns>The quantity that was actually added.</returns>
        public int AddItem(Item item, int quantity)
        {
            if (item == null || quantity <= 0)
                return 0;
            
            // If slot is empty, set the item
            if (IsEmpty)
            {
                Item = item;
                Quantity = Math.Min(quantity, item.MaxStackSize);
                OnSlotChanged();
                return Quantity;
            }
            
            // If slot has same item type, add quantity
            if (Item.CanStack(item))
            {
                int spaceLeft = Item.MaxStackSize - Quantity;
                int amountToAdd = Math.Min(spaceLeft, quantity);
                
                Quantity += amountToAdd;
                OnSlotChanged();
                return amountToAdd;
            }
            
            // Cannot add to this slot
            return 0;
        }

        /// <summary>
        /// Removes a quantity of the item from this slot.
        /// </summary>
        /// <param name="quantity">The quantity to remove.</param>
        /// <returns>The quantity that was actually removed.</returns>
        public int RemoveItem(int quantity)
        {
            if (IsEmpty || quantity <= 0)
                return 0;
            
            int amountToRemove = Math.Min(Quantity, quantity);
            Quantity -= amountToRemove;
            
            if (Quantity <= 0)
            {
                Clear();
            }
            
            OnSlotChanged();
            return amountToRemove;
        }

        /// <summary>
        /// Clears this slot.
        /// </summary>
        public void Clear()
        {
            if (!IsEmpty)
            {
                Item = null;
                Quantity = 0;
                OnSlotChanged();
            }
        }

        /// <summary>
        /// Raises the SlotChanged event.
        /// </summary>
        protected virtual void OnSlotChanged()
        {
            SlotChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Event arguments for crafting events.
    /// </summary>
    public class CraftingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the recipe that was crafted.
        /// </summary>
        public Recipe Recipe { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingEventArgs"/> class.
        /// </summary>
        /// <param name="recipe">The recipe that was crafted.</param>
        public CraftingEventArgs(Recipe recipe)
        {
            Recipe = recipe;
        }
    }

    /// <summary>
    /// Event arguments for crafting result changed events.
    /// </summary>
    public class CraftingResultChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current recipe match.
        /// </summary>
        public Recipe Recipe { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingResultChangedEventArgs"/> class.
        /// </summary>
        /// <param name="recipe">The current recipe match (can be null if no match).</param>
        public CraftingResultChangedEventArgs(Recipe recipe)
        {
            Recipe = recipe;
        }
    }
}
