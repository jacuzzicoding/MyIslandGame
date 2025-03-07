using System;
using System.Collections.Generic;
using MyIslandGame.Crafting;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that allows an entity to perform crafting operations.
    /// </summary>
    public class CraftingComponent : Component
    {
        /// <summary>
        /// Gets the highest level crafting station available to this entity.
        /// </summary>
        public CraftingStationType HighestStationType { get; private set; }
        
        /// <summary>
        /// Gets a list of unlocked recipe IDs.
        /// </summary>
        public HashSet<string> UnlockedRecipes { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether the entity is currently crafting.
        /// </summary>
        public bool IsCrafting { get; private set; }
        
        /// <summary>
        /// Event raised when a recipe is unlocked.
        /// </summary>
        public event EventHandler<RecipeUnlockedEventArgs> RecipeUnlocked;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingComponent"/> class.
        /// </summary>
        /// <param name="stationType">The highest crafting station type available.</param>
        public CraftingComponent(CraftingStationType stationType = CraftingStationType.None)
        {
            HighestStationType = stationType;
            UnlockedRecipes = new HashSet<string>();
            IsCrafting = false;
        }
        
        /// <summary>
        /// Sets the crafting state.
        /// </summary>
        /// <param name="isCrafting">Whether the entity is crafting.</param>
        public void SetCraftingState(bool isCrafting)
        {
            IsCrafting = isCrafting;
        }
        
        /// <summary>
        /// Upgrades the crafting station type.
        /// </summary>
        /// <param name="stationType">The new station type.</param>
        /// <returns>True if the station was upgraded, false if downgraded or unchanged.</returns>
        public bool UpgradeStation(CraftingStationType stationType)
        {
            if (stationType > HighestStationType)
            {
                HighestStationType = stationType;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Unlocks a recipe for crafting.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe to unlock.</param>
        /// <returns>True if the recipe was newly unlocked, false if already unlocked.</returns>
        public bool UnlockRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return false;
            
            if (UnlockedRecipes.Add(recipeId))
            {
                OnRecipeUnlocked(recipeId);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if a recipe is unlocked.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe to check.</param>
        /// <returns>True if the recipe is unlocked, otherwise false.</returns>
        public bool IsRecipeUnlocked(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return false;
            
            return UnlockedRecipes.Contains(recipeId);
        }
        
        /// <summary>
        /// Raises the RecipeUnlocked event.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe that was unlocked.</param>
        protected virtual void OnRecipeUnlocked(string recipeId)
        {
            RecipeUnlocked?.Invoke(this, new RecipeUnlockedEventArgs(recipeId));
        }
    }
    
    /// <summary>
    /// Event arguments for recipe unlocked events.
    /// </summary>
    public class RecipeUnlockedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the recipe that was unlocked.
        /// </summary>
        public string RecipeId { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeUnlockedEventArgs"/> class.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe that was unlocked.</param>
        public RecipeUnlockedEventArgs(string recipeId)
        {
            RecipeId = recipeId;
        }
    }
}
