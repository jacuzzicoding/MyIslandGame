using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyIslandGame.Crafting;
using MyIslandGame.ECS.Components;
using MyIslandGame.Inventory;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A UI component for displaying and interacting with the crafting system.
    /// </summary>
    public class UICraftingGrid : UIContainer
    {
        private const int SlotSize = 40;
        private const int SlotPadding = 5;
        
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteFont _font;
        private readonly CraftingSystem _craftingSystem;
        private readonly InventoryComponent _inventoryComponent;
        private readonly MyIslandGame.Core.Resources.ResourceManager _resourceManager;
        
        private readonly List<UISlot> _inputSlots = new List<UISlot>();
        private UISlot _outputSlot;
        private UIButton _craftButton;
        private UILabel _recipeNameLabel;
        
        private Recipe _currentRecipe;
        private Item[] _craftingGrid;
        
        /// <summary>
        /// Gets the number of input slots.
        /// </summary>
        public int InputSlotCount => _inputSlots.Count;
        
        /// <summary>
        /// Gets the current recipe.
        /// </summary>
        public Recipe CurrentRecipe => _currentRecipe;
        
        /// <summary>
        /// Occurs when an item is crafted.
        /// </summary>
        public event EventHandler<CraftingEventArgs> ItemCrafted;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UICraftingGrid"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="bounds">The bounds of the crafting grid.</param>
        /// <param name="font">The font to use for text.</param>
        /// <param name="craftingSystem">The crafting system.</param>
        /// <param name="inventoryComponent">The inventory component.</param>
        public UICraftingGrid(
            GraphicsDevice graphicsDevice,
            Rectangle bounds,
            SpriteFont font,
            CraftingSystem craftingSystem,
            InventoryComponent inventoryComponent,
            MyIslandGame.Core.Resources.ResourceManager resourceManager)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
                
            if (craftingSystem == null)
                throw new ArgumentNullException(nameof(craftingSystem));
                
            if (inventoryComponent == null)
                throw new ArgumentNullException(nameof(inventoryComponent));
                
            Bounds = bounds;
            _graphicsDevice = graphicsDevice;
            _font = font;
            _craftingSystem = craftingSystem;
            _inventoryComponent = inventoryComponent;
            _resourceManager = resourceManager;
            
            // Initialize crafting grid array (3x3 grid)
            _craftingGrid = new Item[9];
            
            // Create UI elements
            CreateCraftingUI();
        }
        
        /// <summary>
        /// Initializes the crafting grid and its children.
        /// </summary>
        public override void Initialize()
        {
            // Don't call abstract base.Initialize()
            
            // Initialize child elements
            foreach (var child in Children)
            {
                child.Initialize();
            }
        }
        
        /// <summary>
        /// Updates the crafting grid and its children.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Check for recipe match based on current inputs
            UpdateRecipeMatch();
        }
        
        /// <summary>
        /// Creates the crafting UI elements.
        /// </summary>
        private void CreateCraftingUI()
        {
            // Create panel background
            var panel = new UIPanel(
                _graphicsDevice,
                Bounds,
                new Color(50, 50, 50, 200),
                Color.Gray,
                2);
                
            AddChild(panel);
            
            // Create title label
            var titleLabel = new UILabel(
                _font,
                "Crafting",
                new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + 10),
                Color.White,
                true);
                
            titleLabel.SetPosition(new Vector2(
                Bounds.X + (Bounds.Width - titleLabel.Bounds.Width) / 2,
                Bounds.Y + 10));
                
            AddChild(titleLabel);
            
            // Create recipe name label
            _recipeNameLabel = new UILabel(
                _font,
                "",
                new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height - 20),
                Color.White,
                true);
                
            _recipeNameLabel.SetPosition(new Vector2(
                Bounds.X + (Bounds.Width - _recipeNameLabel.Bounds.Width) / 2,
                Bounds.Y + Bounds.Height - 20));
                
            AddChild(_recipeNameLabel);
            
            // Create crafting grid (3x3)
            int gridStartX = Bounds.X + 20;
            int gridStartY = Bounds.Y + 40;
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int index = row * 3 + col;
                    int x = gridStartX + col * (SlotSize + SlotPadding);
                    int y = gridStartY + row * (SlotSize + SlotPadding);
                    
                    var slot = new UISlot(
                        _graphicsDevice,
                        new Rectangle(x, y, SlotSize, SlotSize),
                        _font)
                    {
                        SlotIndex = index
                    };
                    
                    // Set up event handlers
                    slot.Click += OnInputSlotClick;
                    
                    _inputSlots.Add(slot);
                    AddChild(slot);
                }
            }
            
            // Create output slot
            int outputX = gridStartX + 4 * (SlotSize + SlotPadding);
            int outputY = gridStartY + SlotSize;
            
            _outputSlot = new UISlot(
                _graphicsDevice,
                new Rectangle(outputX, outputY, SlotSize, SlotSize),
                _font);
                
            _outputSlot.Click += OnOutputSlotClick;
            AddChild(_outputSlot);
            
            // Create craft button
            int buttonX = gridStartX + 3 * (SlotSize + SlotPadding);
            int buttonY = gridStartY + SlotSize;
            int buttonWidth = SlotSize;
            int buttonHeight = SlotSize;
            
            _craftButton = new UIButton(
                _graphicsDevice,
                new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight),
                new Color(0, 100, 0),
                new Color(0, 150, 0),
                new Color(0, 200, 0),
                _font,
                "→",
                Color.White);
                
            _craftButton.Click += OnCraftButtonClick;
            AddChild(_craftButton);
        }
        
        /// <summary>
        /// Checks for a recipe match based on the current crafting grid.
        /// </summary>
        private void UpdateRecipeMatch()
        {
            // Convert UI slots to recipe pattern
            string[] pattern = new string[9];
            
            for (int i = 0; i < 9; i++)
            {
                var slot = _inputSlots[i];
                if (slot.InventorySlot != null && !slot.InventorySlot.IsEmpty)
                {
                    pattern[i] = slot.InventorySlot.Item.Id;
                    _craftingGrid[i] = slot.InventorySlot.Item;
                }
                else
                {
                    pattern[i] = null;
                    _craftingGrid[i] = null;
                }
            }
            
            // Convert pattern to grid items format for the crafting system
            Item[,] gridItems = new Item[3, 3];
            for (int row = 0; row < 3; row++) {
                for (int col = 0; col < 3; col++) {
                    int index = row * 3 + col;
                    gridItems[row, col] = _craftingGrid[index];
                }
            }
            
            // Find matching recipe using the crafting system
            var recipeManager = _craftingSystem.GetRecipeManager();
            if (recipeManager != null) {
                _currentRecipe = recipeManager.MatchRecipe(gridItems, _craftingSystem.CurrentStation);
            }
            
            // Update output slot
            if (_currentRecipe != null)
            {
                // Create a temporary inventory slot with the recipe result
                var resultSlot = new InventorySlot();
                // Get first result from recipe
                if (_currentRecipe.Results.Count > 0) {
                    if (_resourceManager.TryGetResource(_currentRecipe.Results[0].ItemId, out var resource)) {
                        resultSlot.AddItem(Item.FromResource(resource), _currentRecipe.Results[0].Quantity);
                    }
                }
                _outputSlot.InventorySlot = resultSlot;
                
                // Update recipe name
                _recipeNameLabel.Text = _currentRecipe.Name;
                
                // Enable craft button
                _craftButton.IsActive = true;
            }
            else
            {
                // No matching recipe
                _outputSlot.InventorySlot = null;
                _recipeNameLabel.Text = "No recipe match";
                
                // Disable craft button
                _craftButton.IsActive = false;
            }
        }
        
        /// <summary>
        /// Handles input slot click events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInputSlotClick(object sender, UIEventArgs e)
        {
            if (sender is UISlot slot)
            {
                // Get currently selected inventory item
                int selectedHotbarIndex = _inventoryComponent.SelectedHotbarIndex;
                var selectedSlot = _inventoryComponent.Inventory.GetSlot(selectedHotbarIndex);
                
                if (!selectedSlot.IsEmpty)
                {
                    // Place one item in the crafting grid
                    if (slot.InventorySlot == null)
                    {
                        // Create a new inventory slot for this crafting slot
                        slot.InventorySlot = new InventorySlot();
                    }
                    
                    // Add one item to the crafting slot
                    if (slot.InventorySlot.IsEmpty)
                    {
                        slot.InventorySlot.AddItem(selectedSlot.Item, 1);
                        selectedSlot.RemoveItem(1);
                    }
                    else if (slot.InventorySlot.CanStack(selectedSlot.Item))
                    {
                        slot.InventorySlot.AddItem(selectedSlot.Item, 1);
                        selectedSlot.RemoveItem(1);
                    }
                }
                else if (slot.InventorySlot != null && !slot.InventorySlot.IsEmpty)
                {
                    // Take all items from crafting grid back to inventory
                    _inventoryComponent.Inventory.TryAddItem(
                        slot.InventorySlot.Item, 
                        slot.InventorySlot.Quantity);
                        
                    slot.InventorySlot.Clear();
                }
                
                // Update recipe match after changes
                UpdateRecipeMatch();
            }
        }
        
        /// <summary>
        /// Handles output slot click events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnOutputSlotClick(object sender, UIEventArgs e)
        {
            // Take crafted item
            if (_currentRecipe != null && _outputSlot.InventorySlot != null && !_outputSlot.InventorySlot.IsEmpty)
            {
                // Add crafted item to inventory
                if (_inventoryComponent.Inventory.TryAddItem(
                    _outputSlot.InventorySlot.Item, 
                    _outputSlot.InventorySlot.Quantity))
                {
                    // Consume crafting ingredients
                    ConsumeIngredients();
                    
                    // Raise event
                    OnItemCrafted(_currentRecipe);
                    
                    // Update recipe match after crafting
                    UpdateRecipeMatch();
                }
            }
        }
        
        /// <summary>
        /// Handles craft button click events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCraftButtonClick(object sender, UIEventArgs e)
        {
            if (_currentRecipe != null)
            {
                // Get the first result from the recipe
                if (_currentRecipe.Results.Count == 0) return;
                
                // Create crafted item from the first result
                string resultItemId = _currentRecipe.Results[0].ItemId;
                int resultQuantity = _currentRecipe.Results[0].Quantity;
                
                // Try to get resource for the result
                if (!_resourceManager.TryGetResource(resultItemId, out var resource)) return;
                Item craftedItem = Item.FromResource(resource);
                
                // Add crafted item to inventory
                if (_inventoryComponent.Inventory.TryAddItem(craftedItem, resultQuantity))
                {
                    // Consume crafting ingredients
                    ConsumeIngredients();
                    
                    // Raise event
                    OnItemCrafted(_currentRecipe);
                    
                    // Update recipe match after crafting
                    UpdateRecipeMatch();
                }
            }
        }
        
        /// <summary>
        /// Consumes the ingredients for the current recipe.
        /// </summary>
        private void ConsumeIngredients()
        {
            if (_currentRecipe == null)
                return;
                
            // Consume items from crafting grid
            for (int i = 0; i < _inputSlots.Count; i++)
            {
                var slot = _inputSlots[i];
                
                if (slot.InventorySlot != null && !slot.InventorySlot.IsEmpty)
                {
                    slot.InventorySlot.RemoveItem(1);
                }
            }
        }
        
        /// <summary>
        /// Clears all ingredients from the crafting grid.
        /// </summary>
        public void ClearGrid()
        {
            foreach (var slot in _inputSlots)
            {
                if (slot.InventorySlot != null && !slot.InventorySlot.IsEmpty)
                {
                    // Return items to inventory
                    _inventoryComponent.Inventory.TryAddItem(
                        slot.InventorySlot.Item, 
                        slot.InventorySlot.Quantity);
                        
                    slot.InventorySlot.Clear();
                }
            }
            
            // Update recipe match
            UpdateRecipeMatch();
        }
        
        /// <summary>
        /// Raises the ItemCrafted event.
        /// </summary>
        /// <param name="recipe">The recipe that was crafted.</param>
        protected virtual void OnItemCrafted(Recipe recipe)
        {
            ItemCrafted?.Invoke(this, new CraftingEventArgs(recipe));
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
}