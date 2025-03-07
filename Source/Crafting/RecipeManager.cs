using System;
using System.Collections.Generic;
using System.Linq;
using MyIslandGame.Core.Resources;
using MyIslandGame.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.Crafting
{
    /// <summary>
    /// Manages recipe registration, lookup, and matching for the crafting system.
    /// </summary>
    public class RecipeManager
    {
        private readonly Dictionary<string, Recipe> _recipesById = new Dictionary<string, Recipe>();
        private readonly Dictionary<RecipeCategory, List<Recipe>> _recipesByCategory = new Dictionary<RecipeCategory, List<Recipe>>();
        private readonly Dictionary<CraftingStationType, List<Recipe>> _recipesByStation = new Dictionary<CraftingStationType, List<Recipe>>();
        
        private readonly ResourceManager _resourceManager;
        private readonly GraphicsDevice _graphicsDevice;
        
        // Recipe icon cache
        private readonly Dictionary<string, Texture2D> _recipeIcons = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Gets the collection of all registered recipes.
        /// </summary>
        public IEnumerable<Recipe> AllRecipes => _recipesById.Values;

        /// <summary>
        /// Event raised when a recipe is added.
        /// </summary>
        public event EventHandler<RecipeEventArgs> RecipeAdded;
        
        /// <summary>
        /// Event raised when a recipe is unlocked.
        /// </summary>
        public event EventHandler<RecipeEventArgs> RecipeUnlocked;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeManager"/> class.
        /// </summary>
        /// <param name="resourceManager">The resource manager for accessing resources.</param>
        /// <param name="graphicsDevice">The graphics device for creating textures.</param>
        public RecipeManager(ResourceManager resourceManager, GraphicsDevice graphicsDevice)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            
            // Initialize category dictionary
            foreach (RecipeCategory category in Enum.GetValues(typeof(RecipeCategory)))
            {
                _recipesByCategory[category] = new List<Recipe>();
            }
            
            // Initialize station dictionary
            foreach (CraftingStationType station in Enum.GetValues(typeof(CraftingStationType)))
            {
                _recipesByStation[station] = new List<Recipe>();
            }
        }

        /// <summary>
        /// Adds a recipe to the manager.
        /// </summary>
        /// <param name="recipe">The recipe to add.</param>
        /// <returns>True if the recipe was added successfully, false if it already exists.</returns>
        public bool AddRecipe(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));
            
            if (_recipesById.ContainsKey(recipe.Id))
                return false;
            
            _recipesById[recipe.Id] = recipe;
            _recipesByCategory[recipe.Category].Add(recipe);
            _recipesByStation[recipe.RequiredStation].Add(recipe);
            
            OnRecipeAdded(recipe);
            return true;
        }
        
        /// <summary>
        /// Adds multiple recipes to the manager.
        /// </summary>
        /// <param name="recipes">The recipes to add.</param>
        /// <returns>The number of recipes successfully added.</returns>
        public int AddRecipes(IEnumerable<Recipe> recipes)
        {
            if (recipes == null)
                throw new ArgumentNullException(nameof(recipes));
            
            int added = 0;
            foreach (var recipe in recipes)
            {
                if (AddRecipe(recipe))
                    added++;
            }
            
            return added;
        }

        /// <summary>
        /// Gets a recipe by its ID.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe to get.</param>
        /// <returns>The recipe, or null if not found.</returns>
        public Recipe GetRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return null;
            
            _recipesById.TryGetValue(recipeId, out var recipe);
            return recipe;
        }

        /// <summary>
        /// Gets recipes by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <param name="unlockedOnly">Whether to only include unlocked recipes.</param>
        /// <returns>A collection of matching recipes.</returns>
        public IEnumerable<Recipe> GetRecipesByCategory(RecipeCategory category, bool unlockedOnly = true)
        {
            if (!_recipesByCategory.TryGetValue(category, out var recipes))
                return Enumerable.Empty<Recipe>();
            
            return unlockedOnly 
                ? recipes.Where(r => r.IsUnlocked) 
                : recipes;
        }

        /// <summary>
        /// Gets recipes by required crafting station.
        /// </summary>
        /// <param name="stationType">The crafting station type to filter by.</param>
        /// <param name="unlockedOnly">Whether to only include unlocked recipes.</param>
        /// <returns>A collection of matching recipes.</returns>
        public IEnumerable<Recipe> GetRecipesByStation(CraftingStationType stationType, bool unlockedOnly = true)
        {
            if (!_recipesByStation.TryGetValue(stationType, out var recipes))
                return Enumerable.Empty<Recipe>();
            
            return unlockedOnly 
                ? recipes.Where(r => r.IsUnlocked) 
                : recipes;
        }
        
        /// <summary>
        /// Finds recipes that can be crafted with the given inventory items.
        /// </summary>
        /// <param name="availableItems">Dictionary of available item IDs and quantities.</param>
        /// <param name="stationType">The crafting station type to filter by (optional).</param>
        /// <returns>A collection of craftable recipes.</returns>
        public IEnumerable<Recipe> FindCraftableRecipes(
            Dictionary<string, int> availableItems, 
            CraftingStationType? stationType = null)
        {
            if (availableItems == null)
                throw new ArgumentNullException(nameof(availableItems));
            
            // Get all recipes or just those for a specific station
            var recipesToCheck = stationType.HasValue 
                ? GetRecipesByStation(stationType.Value, true)
                : _recipesById.Values.Where(r => r.IsUnlocked);
            
            // Filter to recipes that can be crafted
            return recipesToCheck.Where(r => r.CanCraft(availableItems));
        }

        /// <summary>
        /// Matches a recipe based on the provided crafting grid.
        /// </summary>
        /// <param name="grid">The crafting grid layout.</param>
        /// <param name="stationType">The crafting station being used.</param>
        /// <returns>The matching recipe, or null if no match found.</returns>
        public Recipe MatchRecipe(Item[,] grid, CraftingStationType stationType)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            
            var stationRecipes = GetRecipesByStation(stationType, true);
            
            // Convert grid to a dictionary for shapeless matching
            Dictionary<string, int> gridItems = new Dictionary<string, int>();
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    var item = grid[y, x];
                    if (item != null)
                    {
                        if (gridItems.ContainsKey(item.Id))
                            gridItems[item.Id]++;
                        else
                            gridItems[item.Id] = 1;
                    }
                }
            }
            
            // Try to match shaped recipes first
            foreach (var recipe in stationRecipes.Where(r => r.Type == RecipeType.Shaped))
            {
                if (recipe.MatchesShapedGrid(grid))
                    return recipe;
            }
            
            // Then try shapeless recipes
            foreach (var recipe in stationRecipes.Where(r => r.Type == RecipeType.Shapeless))
            {
                if (recipe.MatchesShapelessItems(gridItems))
                    return recipe;
            }
            
            // No match found
            return null;
        }

        /// <summary>
        /// Matches a smelting recipe based on the input item.
        /// </summary>
        /// <param name="inputItemId">The ID of the input item.</param>
        /// <param name="inputQuantity">The quantity of the input item.</param>
        /// <returns>The matching smelting recipe, or null if no match found.</returns>
        public Recipe MatchSmeltingRecipe(string inputItemId, int inputQuantity)
        {
            if (string.IsNullOrEmpty(inputItemId) || inputQuantity <= 0)
                return null;
            
            var smeltingRecipes = GetRecipesByStation(CraftingStationType.Furnace, true)
                .Where(r => r.Type == RecipeType.Smelting);
            
            foreach (var recipe in smeltingRecipes)
            {
                if (recipe.MatchesSmeltingInput(inputItemId, inputQuantity))
                    return recipe;
            }
            
            return null;
        }

        /// <summary>
        /// Gets an icon texture for a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to get an icon for.</param>
        /// <returns>A texture representing the recipe.</returns>
        public Texture2D GetRecipeIcon(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));
            
            // Return cached icon if available
            if (_recipeIcons.TryGetValue(recipe.Id, out var texture))
                return texture;
            
            // Otherwise, get the icon from the first result
            if (recipe.Results.Count > 0)
            {
                string resultItemId = recipe.Results[0].ItemId;
                
                // Try to get resource for the result
                if (_resourceManager.TryGetResource(resultItemId, out var resource) && resource.Icon != null)
                {
                    _recipeIcons[recipe.Id] = resource.Icon;
                    return resource.Icon;
                }
                
                // Create placeholder
                texture = CreateRecipeIconPlaceholder(recipe);
                _recipeIcons[recipe.Id] = texture;
                return texture;
            }
            
            // Fallback placeholder
            texture = CreateRecipeIconPlaceholder(recipe);
            _recipeIcons[recipe.Id] = texture;
            return texture;
        }

        /// <summary>
        /// Unlocks a recipe by ID.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe to unlock.</param>
        /// <returns>True if the recipe was found and unlocked, false otherwise.</returns>
        public bool UnlockRecipe(string recipeId)
        {
            Recipe recipe = GetRecipe(recipeId);
            if (recipe == null || recipe.IsUnlocked)
                return false;
            
            recipe.Unlock();
            OnRecipeUnlocked(recipe);
            return true;
        }

        /// <summary>
        /// Initializes the recipe manager with default recipes.
        /// </summary>
        public void InitializeDefaultRecipes()
        {
            // Add shapeless recipes
            AddWoodPlankRecipe();
            AddStickRecipe();
            
            // Add shaped recipes
            AddCraftingTableRecipe();
            AddWoodenAxeRecipe();
            AddWoodenPickaxeRecipe();
            AddWoodenSwordRecipe();
            AddStoneAxeRecipe();
            AddStonePickaxeRecipe();
            
            // Add smelting recipes
            // Will be implemented when metal resources are added
        }

        #region Default Recipe Definitions

        private void AddWoodPlankRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("wood_log", out var woodLog) ||
                !_resourceManager.TryGetResource("wood_plank", out var woodPlank))
                return;
            
            // Wood Log -> 4 Wood Planks
            var ingredient = RecipeIngredient.FromResource(woodLog);
            var result = new RecipeResult(woodPlank.Id, 4, woodPlank.Name);
            
            var recipe = new Recipe(
                "recipe_wood_planks",
                "Wood Planks",
                "Convert a log into wooden planks",
                RecipeCategory.Resources,
                CraftingStationType.None,
                new List<RecipeIngredient> { ingredient },
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddStickRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("wood_plank", out var woodPlank) ||
                !_resourceManager.TryGetResource("stick", out var stick))
                return;
            
            // 2 Wood Planks -> 4 Sticks
            var ingredient = new RecipeIngredient(woodPlank.Id, 2, false, woodPlank.Name);
            var result = new RecipeResult(stick.Id, 4, stick.Name);
            
            var recipe = new Recipe(
                "recipe_sticks",
                "Sticks",
                "Convert wooden planks into sticks",
                RecipeCategory.Resources,
                CraftingStationType.None,
                new List<RecipeIngredient> { ingredient },
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddCraftingTableRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("wood_plank", out var woodPlank) ||
                !_resourceManager.TryGetResource("crafting_table", out var craftingTable))
                return;
            
            // 4 Wood Planks in 2x2 grid -> Crafting Table
            var ingredient = new RecipeIngredient(woodPlank.Id, 1, false, woodPlank.Name);
            var result = new RecipeResult(craftingTable.Id, 1, craftingTable.Name);
            
            // Define pattern for a 2x2 grid filled with wood planks
            string[] pattern = new string[] {
                "##",
                "##"
            };
            
            var patternMap = new Dictionary<char, RecipeIngredient> {
                { '#', ingredient }
            };
            
            var recipe = new Recipe(
                "recipe_crafting_table",
                "Crafting Table",
                "Create a crafting table for advanced recipes",
                RecipeCategory.Furniture,
                CraftingStationType.None,
                pattern,
                patternMap,
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddWoodenAxeRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("wood_plank", out var woodPlank) ||
                !_resourceManager.TryGetResource("stick", out var stick) ||
                !_resourceManager.TryGetResource("wooden_axe", out var woodenAxe))
                return;
            
            var plankIngredient = new RecipeIngredient(woodPlank.Id, 1, false, woodPlank.Name);
            var stickIngredient = new RecipeIngredient(stick.Id, 1, false, stick.Name);
            var result = new RecipeResult(woodenAxe.Id, 1, woodenAxe.Name);
            
            // Define pattern: ## (planks)
            //                 #| (plank, stick)
            //                  | (stick)
            string[] pattern = new string[] {
                "##",
                "#|",
                " |"
            };
            
            var patternMap = new Dictionary<char, RecipeIngredient> {
                { '#', plankIngredient },
                { '|', stickIngredient }
            };
            
            var recipe = new Recipe(
                "recipe_wooden_axe",
                "Wooden Axe",
                "Create a wooden axe for chopping trees",
                RecipeCategory.Tools,
                CraftingStationType.CraftingTable,
                pattern,
                patternMap,
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddWoodenPickaxeRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("wood_plank", out var woodPlank) ||
                !_resourceManager.TryGetResource("stick", out var stick) ||
                !_resourceManager.TryGetResource("wooden_pickaxe", out var woodenPickaxe))
                return;
            
            var plankIngredient = new RecipeIngredient(woodPlank.Id, 1, false, woodPlank.Name);
            var stickIngredient = new RecipeIngredient(stick.Id, 1, false, stick.Name);
            var result = new RecipeResult(woodenPickaxe.Id, 1, woodenPickaxe.Name);
            
            // Define pattern: ### (planks)
            //                  |  (stick)
            //                  |  (stick)
            string[] pattern = new string[] {
                "###",
                " | ",
                " | "
            };
            
            var patternMap = new Dictionary<char, RecipeIngredient> {
                { '#', plankIngredient },
                { '|', stickIngredient }
            };
            
            var recipe = new Recipe(
                "recipe_wooden_pickaxe",
                "Wooden Pickaxe",
                "Create a wooden pickaxe for mining stone",
                RecipeCategory.Tools,
                CraftingStationType.CraftingTable,
                pattern,
                patternMap,
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddWoodenSwordRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("wood_plank", out var woodPlank) ||
                !_resourceManager.TryGetResource("stick", out var stick) ||
                !_resourceManager.TryGetResource("wooden_sword", out var woodenSword))
                return;
            
            var plankIngredient = new RecipeIngredient(woodPlank.Id, 1, false, woodPlank.Name);
            var stickIngredient = new RecipeIngredient(stick.Id, 1, false, stick.Name);
            var result = new RecipeResult(woodenSword.Id, 1, woodenSword.Name);
            
            // Define pattern: # (plank)
            //                 # (plank)
            //                 | (stick)
            string[] pattern = new string[] {
                "#",
                "#",
                "|"
            };
            
            var patternMap = new Dictionary<char, RecipeIngredient> {
                { '#', plankIngredient },
                { '|', stickIngredient }
            };
            
            var recipe = new Recipe(
                "recipe_wooden_sword",
                "Wooden Sword",
                "Create a wooden sword for defense",
                RecipeCategory.Weapons,
                CraftingStationType.CraftingTable,
                pattern,
                patternMap,
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddStoneAxeRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("stone", out var stone) ||
                !_resourceManager.TryGetResource("stick", out var stick) ||
                !_resourceManager.TryGetResource("stone_axe", out var stoneAxe))
                return;
            
            var stoneIngredient = new RecipeIngredient(stone.Id, 1, false, stone.Name);
            var stickIngredient = new RecipeIngredient(stick.Id, 1, false, stick.Name);
            var result = new RecipeResult(stoneAxe.Id, 1, stoneAxe.Name);
            
            // Define pattern: ## (stone)
            //                 #| (stone, stick)
            //                  | (stick)
            string[] pattern = new string[] {
                "##",
                "#|",
                " |"
            };
            
            var patternMap = new Dictionary<char, RecipeIngredient> {
                { '#', stoneIngredient },
                { '|', stickIngredient }
            };
            
            var recipe = new Recipe(
                "recipe_stone_axe",
                "Stone Axe",
                "Create a stone axe for efficient wood chopping",
                RecipeCategory.Tools,
                CraftingStationType.CraftingTable,
                pattern,
                patternMap,
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        private void AddStonePickaxeRecipe()
        {
            // Check if resources exist
            if (!_resourceManager.TryGetResource("stone", out var stone) ||
                !_resourceManager.TryGetResource("stick", out var stick) ||
                !_resourceManager.TryGetResource("stone_pickaxe", out var stonePickaxe))
                return;
            
            var stoneIngredient = new RecipeIngredient(stone.Id, 1, false, stone.Name);
            var stickIngredient = new RecipeIngredient(stick.Id, 1, false, stick.Name);
            var result = new RecipeResult(stonePickaxe.Id, 1, stonePickaxe.Name);
            
            // Define pattern: ### (stone)
            //                  |  (stick)
            //                  |  (stick)
            string[] pattern = new string[] {
                "###",
                " | ",
                " | "
            };
            
            var patternMap = new Dictionary<char, RecipeIngredient> {
                { '#', stoneIngredient },
                { '|', stickIngredient }
            };
            
            var recipe = new Recipe(
                "recipe_stone_pickaxe",
                "Stone Pickaxe",
                "Create a stone pickaxe for efficient mining",
                RecipeCategory.Tools,
                CraftingStationType.CraftingTable,
                pattern,
                patternMap,
                new List<RecipeResult> { result }
            );
            
            AddRecipe(recipe);
        }

        #endregion

        /// <summary>
        /// Creates a placeholder texture for a recipe icon.
        /// </summary>
        /// <param name="recipe">The recipe to create a placeholder for.</param>
        /// <returns>A simple colored texture based on the recipe category.</returns>
        private Texture2D CreateRecipeIconPlaceholder(Recipe recipe)
        {
            int size = 32;
            Texture2D texture = new Texture2D(_graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            // Choose color based on recipe category
            Color color = recipe.Category switch
            {
                RecipeCategory.Tools => Color.SandyBrown,
                RecipeCategory.Weapons => Color.Firebrick,
                RecipeCategory.Building => Color.SlateGray,
                RecipeCategory.Furniture => Color.SaddleBrown,
                RecipeCategory.Resources => Color.ForestGreen,
                RecipeCategory.Food => Color.Orange,
                RecipeCategory.Miscellaneous => Color.Purple,
                _ => Color.White
            };
            
            // Fill the texture
            for (int i = 0; i < data.Length; i++)
            {
                // Create a border
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

        /// <summary>
        /// Raises the RecipeAdded event.
        /// </summary>
        /// <param name="recipe">The recipe that was added.</param>
        protected virtual void OnRecipeAdded(Recipe recipe)
        {
            RecipeAdded?.Invoke(this, new RecipeEventArgs(recipe));
        }

        /// <summary>
        /// Raises the RecipeUnlocked event.
        /// </summary>
        /// <param name="recipe">The recipe that was unlocked.</param>
        protected virtual void OnRecipeUnlocked(Recipe recipe)
        {
            RecipeUnlocked?.Invoke(this, new RecipeEventArgs(recipe));
        }
    }

    /// <summary>
    /// Event arguments for recipe-related events.
    /// </summary>
    public class RecipeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the recipe associated with the event.
        /// </summary>
        public Recipe Recipe { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeEventArgs"/> class.
        /// </summary>
        /// <param name="recipe">The recipe associated with the event.</param>
        public RecipeEventArgs(Recipe recipe)
        {
            Recipe = recipe;
        }
    }
}
