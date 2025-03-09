using System;
using System.Collections.Generic;
using System.Linq;
using MyIslandGame.Core.Resources;
using MyIslandGame.Inventory;

namespace MyIslandGame.Crafting
{
    /// <summary>
    /// Defines the type of crafting recipe.
    /// </summary>
    public enum RecipeType
    {
        /// <summary>
        /// Shapeless recipe where only the ingredients matter, not their positions.
        /// </summary>
        Shapeless,
        
        /// <summary>
        /// Shaped recipe where both ingredients and their positions in the grid matter.
        /// </summary>
        Shaped,
        
        /// <summary>
        /// Smelting recipe performed in a furnace.
        /// </summary>
        Smelting
    }

    /// <summary>
    /// Defines the required crafting station for a recipe.
    /// </summary>
    public enum CraftingStationType
    {
        /// <summary>
        /// No specific station required, can craft in 2x2 inventory grid.
        /// </summary>
        None,
        
        /// <summary>
        /// Requires a crafting table with 3x3 grid.
        /// </summary>
        CraftingTable,
        
        /// <summary>
        /// Requires a furnace for smelting recipes.
        /// </summary>
        Furnace,
        
        /// <summary>
        /// Requires an anvil for tool repair and enhancement.
        /// </summary>
        Anvil,
        
        /// <summary>
        /// Requires a specialized cooking station.
        /// </summary>
        Cooking
    }

    /// <summary>
    /// Defines a category for recipe classification.
    /// </summary>
    public enum RecipeCategory
    {
        /// <summary>Tools like axes, pickaxes, etc.</summary>
        Tools,
        
        /// <summary>Weapons like swords, bows, etc.</summary>
        Weapons,
        
        /// <summary>Building materials like walls, foundations, etc.</summary>
        Building,
        
        /// <summary>Furniture items like storage, crafting stations, etc.</summary>
        Furniture,
        
        /// <summary>Processed resources like ingots, planks, etc.</summary>
        Resources,
        
        /// <summary>Food items for consumption.</summary>
        Food,
        
        /// <summary>Miscellaneous items that don't fit other categories.</summary>
        Miscellaneous
    }

    /// <summary>
    /// Represents a crafting recipe that defines how to create items.
    /// </summary>
    public class Recipe
    {
        /// <summary>
        /// Gets the unique identifier for this recipe.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the display name of the recipe.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the description of the recipe.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Gets the type of recipe (shaped, shapeless, smelting).
        /// </summary>
        public RecipeType Type { get; }
        
        /// <summary>
        /// Gets the category of the recipe for UI organization.
        /// </summary>
        public RecipeCategory Category { get; }
        
        /// <summary>
        /// Gets the required crafting station for this recipe.
        /// </summary>
        public CraftingStationType RequiredStation { get; }
        
        /// <summary>
        /// Gets the ingredients required for this recipe.
        /// </summary>
        public List<RecipeIngredient> Ingredients { get; }
        
        /// <summary>
        /// Gets the result items produced by this recipe.
        /// </summary>
        public List<RecipeResult> Results { get; }
        
        /// <summary>
        /// Gets the pattern for shaped recipes. Null for shapeless recipes.
        /// Each string represents a row in the crafting grid with characters mapping to ingredients.
        /// </summary>
        public string[] Pattern { get; }
        
        /// <summary>
        /// Gets the dictionary mapping pattern characters to recipe ingredients.
        /// Only used for shaped recipes.
        /// </summary>
        public Dictionary<char, RecipeIngredient> PatternMap { get; }
        
        /// <summary>
        /// Gets a value indicating whether this recipe has been unlocked by the player.
        /// </summary>
        public bool IsUnlocked { get; private set; }
        
        /// <summary>
        /// Gets the width of the crafting grid required for this recipe.
        /// </summary>
        public int GridWidth => Type == RecipeType.Shaped && Pattern != null && Pattern.Length > 0 
            ? Pattern.Max(row => row.Length) 
            : RequiredStation == CraftingStationType.None ? 2 : 3;
        
        /// <summary>
        /// Gets the height of the crafting grid required for this recipe.
        /// </summary>
        public int GridHeight => Type == RecipeType.Shaped && Pattern != null 
            ? Pattern.Length 
            : RequiredStation == CraftingStationType.None ? 2 : 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="Recipe"/> class for a shapeless recipe.
        /// </summary>
        /// <param name="id">The unique identifier for this recipe.</param>
        /// <param name="name">The display name of the recipe.</param>
        /// <param name="description">The description of the recipe.</param>
        /// <param name="category">The category of the recipe.</param>
        /// <param name="requiredStation">The required crafting station.</param>
        /// <param name="ingredients">The ingredients required for this recipe.</param>
        /// <param name="results">The result items produced by this recipe.</param>
        /// <param name="isUnlocked">Whether this recipe is initially unlocked.</param>
        public Recipe(
            string id,
            string name,
            string description,
            RecipeCategory category,
            CraftingStationType requiredStation,
            List<RecipeIngredient> ingredients,
            List<RecipeResult> results,
            bool isUnlocked = true)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Recipe ID cannot be null or empty", nameof(id));
            
            if (ingredients == null || ingredients.Count == 0)
                throw new ArgumentException("Recipe must have at least one ingredient", nameof(ingredients));
            
            if (results == null || results.Count == 0)
                throw new ArgumentException("Recipe must have at least one result", nameof(results));
            
            Id = id;
            Name = name ?? id;
            Description = description ?? string.Empty;
            Type = RecipeType.Shapeless;
            Category = category;
            RequiredStation = requiredStation;
            Ingredients = ingredients;
            Results = results;
            IsUnlocked = isUnlocked;
            
            // For shapeless recipes, pattern is null
            Pattern = null;
            PatternMap = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Recipe"/> class for a shaped recipe.
        /// </summary>
        /// <param name="id">The unique identifier for this recipe.</param>
        /// <param name="name">The display name of the recipe.</param>
        /// <param name="description">The description of the recipe.</param>
        /// <param name="category">The category of the recipe.</param>
        /// <param name="requiredStation">The required crafting station.</param>
        /// <param name="pattern">The pattern for the shaped recipe.</param>
        /// <param name="patternMap">The mapping of characters to ingredients.</param>
        /// <param name="results">The result items produced by this recipe.</param>
        /// <param name="isUnlocked">Whether this recipe is initially unlocked.</param>
        public Recipe(
            string id,
            string name,
            string description,
            RecipeCategory category,
            CraftingStationType requiredStation,
            string[] pattern,
            Dictionary<char, RecipeIngredient> patternMap,
            List<RecipeResult> results,
            bool isUnlocked = true)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Recipe ID cannot be null or empty", nameof(id));
            
            if (pattern == null || pattern.Length == 0)
                throw new ArgumentException("Shaped recipe must have a pattern", nameof(pattern));
            
            if (patternMap == null || patternMap.Count == 0)
                throw new ArgumentException("Shaped recipe must have a pattern map", nameof(patternMap));
            
            if (results == null || results.Count == 0)
                throw new ArgumentException("Recipe must have at least one result", nameof(results));
            
            // Validate that all characters in the pattern are in the map or are spaces
            foreach (string row in pattern)
            {
                foreach (char c in row)
                {
                    if (c != ' ' && !patternMap.ContainsKey(c))
                    {
                        throw new ArgumentException($"Pattern contains character '{c}' that is not defined in pattern map", nameof(pattern));
                    }
                }
            }
            
            Id = id;
            Name = name ?? id;
            Description = description ?? string.Empty;
            Type = RecipeType.Shaped;
            Category = category;
            RequiredStation = requiredStation;
            Pattern = pattern;
            PatternMap = patternMap;
            Results = results;
            IsUnlocked = isUnlocked;
            
            // For shaped recipes, extract ingredients from pattern map
            Ingredients = patternMap.Values.Distinct().ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Recipe"/> class for a smelting recipe.
        /// </summary>
        /// <param name="id">The unique identifier for this recipe.</param>
        /// <param name="name">The display name of the recipe.</param>
        /// <param name="description">The description of the recipe.</param>
        /// <param name="category">The category of the recipe.</param>
        /// <param name="input">The input ingredient for smelting.</param>
        /// <param name="result">The result item produced by smelting.</param>
        /// <param name="isUnlocked">Whether this recipe is initially unlocked.</param>
        public Recipe(
            string id,
            string name,
            string description,
            RecipeCategory category,
            RecipeIngredient input,
            RecipeResult result,
            bool isUnlocked = true)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Recipe ID cannot be null or empty", nameof(id));
            
            if (input == null)
                throw new ArgumentException("Smelting recipe must have an input ingredient", nameof(input));
            
            if (result == null)
                throw new ArgumentException("Smelting recipe must have a result", nameof(result));
            
            Id = id;
            Name = name ?? id;
            Description = description ?? string.Empty;
            Type = RecipeType.Smelting;
            Category = category;
            RequiredStation = CraftingStationType.Furnace;
            Ingredients = new List<RecipeIngredient> { input };
            Results = new List<RecipeResult> { result };
            IsUnlocked = isUnlocked;
            
            // Smelting recipes don't use patterns
            Pattern = null;
            PatternMap = null;
        }

        /// <summary>
        /// Unlocks this recipe for crafting.
        /// </summary>
        public void Unlock()
        {
            IsUnlocked = true;
        }

        /// <summary>
        /// Checks if the recipe can be crafted with the given ingredients.
        /// </summary>
        /// <param name="availableItems">Available items to craft with.</param>
        /// <returns>True if the recipe can be crafted, false otherwise.</returns>
        public bool CanCraft(Dictionary<string, int> availableItems)
        {
            if (!IsUnlocked)
                return false;
            
            // For each ingredient, check if we have enough
            foreach (var ingredient in Ingredients)
            {
                // Skip optional ingredients
                if (ingredient.IsOptional)
                    continue;
                
                string itemId = ingredient.ItemId;
                
                // Check if we have the item
                if (!availableItems.TryGetValue(itemId, out int available))
                    return false;
                
                // Check if we have enough of the item
                if (available < ingredient.Quantity)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Checks if the shaped recipe matches the given crafting grid.
        /// </summary>
        /// <param name="grid">The crafting grid to check against.</param>
        /// <returns>True if the recipe matches the grid, false otherwise.</returns>
        public bool MatchesShapedGrid(Item[,] grid)
        {
            if (Type != RecipeType.Shaped || Pattern == null || grid == null)
                return false;
            
            int gridWidth = grid.GetLength(1);
            int gridHeight = grid.GetLength(0);
            
            // Check if the grid is big enough for the pattern
            if (gridWidth < GridWidth || gridHeight < GridHeight)
                return false;
            
            // Try to match the pattern at different positions in the grid
            for (int startY = 0; startY <= gridHeight - GridHeight; startY++)
            {
                for (int startX = 0; startX <= gridWidth - GridWidth; startX++)
                {
                    if (MatchesAtPosition(grid, startX, startY))
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if the shapeless recipe matches the given crafting items.
        /// </summary>
        /// <param name="items">The items in the crafting grid.</param>
        /// <returns>True if the recipe matches the items, false otherwise.</returns>
        public bool MatchesShapelessItems(Dictionary<string, int> items)
        {
            if (Type != RecipeType.Shapeless)
                return false;
            
            // Create a copy of the items dictionary to modify
            var availableItems = new Dictionary<string, int>(items);
            
            // Check each required ingredient
            foreach (var ingredient in Ingredients)
            {
                string itemId = ingredient.ItemId;
                
                // Skip optional ingredients
                if (ingredient.IsOptional)
                    continue;
                
                // Check if we have the item
                if (!availableItems.TryGetValue(itemId, out int available))
                    return false;
                
                // Check if we have enough of the item
                if (available < ingredient.Quantity)
                    return false;
                
                // Use up the items
                availableItems[itemId] = available - ingredient.Quantity;
                if (availableItems[itemId] <= 0)
                    availableItems.Remove(itemId);
            }
            
            // All required ingredients were matched
            return true;
        }

        /// <summary>
        /// Checks if the smelting recipe matches the given input item.
        /// </summary>
        /// <param name="inputItem">The input item for smelting.</param>
        /// <param name="inputQuantity">The quantity of the input item.</param>
        /// <returns>True if the recipe matches the input, false otherwise.</returns>
        public bool MatchesSmeltingInput(string inputItemId, int inputQuantity)
        {
            if (Type != RecipeType.Smelting || Ingredients.Count == 0)
                return false;
            
            var input = Ingredients[0];
            return inputItemId == input.ItemId && inputQuantity >= input.Quantity;
        }

        /// <summary>
        /// Helper method to check if the shaped pattern matches at a specific position in the grid.
        /// </summary>
        /// <param name="grid">The crafting grid.</param>
        /// <param name="startX">The starting X position.</param>
        /// <param name="startY">The starting Y position.</param>
        /// <returns>True if the pattern matches at the given position, false otherwise.</returns>
        private bool MatchesAtPosition(Item[,] grid, int startX, int startY)
        {
            // For each row in the pattern
            for (int y = 0; y < Pattern.Length; y++)
            {
                string row = Pattern[y];
                
                // For each character in the row
                for (int x = 0; x < row.Length; x++)
                {
                    char c = row[x];
                    Item item = grid[startY + y, startX + x];
                    
                    // If the pattern has a space, the grid cell should be empty
                    if (c == ' ')
                    {
                        if (item != null)
                            return false;
                        continue;
                    }
                    
                    // Get the ingredient for this pattern character
                    if (!PatternMap.TryGetValue(c, out RecipeIngredient ingredient))
                        return false;
                    
                    // If the ingredient is optional and the grid cell is empty, continue
                    if (ingredient.IsOptional && item == null)
                        continue;
                    
                    // Check if the item matches the ingredient
                    if (item == null || item.Id != ingredient.ItemId)
                        return false;
                }
            }
            
            // All pattern cells matched
            return true;
        }
    }

    /// <summary>
    /// Represents an ingredient required for a recipe.
    /// </summary>
    public class RecipeIngredient
    {
        /// <summary>
        /// Gets the ID of the required item.
        /// </summary>
        public string ItemId { get; }
        
        /// <summary>
        /// Gets the quantity of the item required.
        /// </summary>
        public int Quantity { get; }
        
        /// <summary>
        /// Gets a value indicating whether this ingredient is optional.
        /// </summary>
        public bool IsOptional { get; }
        
        /// <summary>
        /// Gets or sets the display name of the ingredient (optional).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeIngredient"/> class.
        /// </summary>
        /// <param name="itemId">The ID of the required item.</param>
        /// <param name="quantity">The quantity of the item required.</param>
        /// <param name="isOptional">Whether this ingredient is optional.</param>
        /// <param name="displayName">Optional display name for the ingredient.</param>
        public RecipeIngredient(string itemId, int quantity = 1, bool isOptional = false, string displayName = null)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentException("Item ID cannot be null or empty", nameof(itemId));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
            ItemId = itemId;
            Quantity = quantity;
            IsOptional = isOptional;
            DisplayName = displayName;
        }

        /// <summary>
        /// Creates a recipe ingredient from a resource.
        /// </summary>
        /// <param name="resource">The resource to use as an ingredient.</param>
        /// <param name="quantity">The quantity required.</param>
        /// <param name="isOptional">Whether this ingredient is optional.</param>
        /// <returns>A new recipe ingredient.</returns>
        public static RecipeIngredient FromResource(Resource resource, int quantity = 1, bool isOptional = false)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            
            return new RecipeIngredient(resource.Id, quantity, isOptional, resource.Name);
        }
    }

    /// <summary>
    /// Represents a result produced by a recipe.
    /// </summary>
    public class RecipeResult
    {
        /// <summary>
        /// Gets the ID of the result item.
        /// </summary>
        public string ItemId { get; }
        
        /// <summary>
        /// Gets the quantity of the item produced.
        /// </summary>
        public int Quantity { get; }
        
        /// <summary>
        /// Gets or sets the display name of the result (optional).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeResult"/> class.
        /// </summary>
        /// <param name="itemId">The ID of the result item.</param>
        /// <param name="quantity">The quantity of the item produced.</param>
        /// <param name="displayName">Optional display name for the result.</param>
        public RecipeResult(string itemId, int quantity = 1, string displayName = null)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentException("Item ID cannot be null or empty", nameof(itemId));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
            ItemId = itemId;
            Quantity = quantity;
            DisplayName = displayName;
        }

        /// <summary>
        /// Creates a recipe result from a resource.
        /// </summary>
        /// <param name="resource">The resource to produce as a result.</param>
        /// <param name="quantity">The quantity produced.</param>
        /// <returns>A new recipe result.</returns>
        public static RecipeResult FromResource(Resource resource, int quantity = 1)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            
            return new RecipeResult(resource.Id, quantity, resource.Name);
        }
    }
}
