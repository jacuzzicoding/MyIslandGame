namespace MyIslandGame.UI
{
    /// <summary>
    /// Defines layers for UI elements, controlling their rendering order.
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// Background elements rendered first (lowest visibility)
        /// </summary>
        Background,
        
        /// <summary>
        /// Middle layer elements rendered after background
        /// </summary>
        Middle,
        
        /// <summary>
        /// Foreground elements rendered on top of middle layer
        /// </summary>
        Foreground,
        
        /// <summary>
        /// HUD elements always on top of the game
        /// </summary>
        HUD,
        
        /// <summary>
        /// Debug elements rendered last (highest visibility)
        /// </summary>
        Debug
    }
}
