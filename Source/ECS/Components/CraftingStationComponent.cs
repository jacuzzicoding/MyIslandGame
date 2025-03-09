using System;
using Microsoft.Xna.Framework;
using MyIslandGame.Crafting;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component for entities that function as crafting stations.
    /// </summary>
    public class CraftingStationComponent : Component
    {
        /// <summary>
        /// Gets the type of crafting station.
        /// </summary>
        public CraftingStationType StationType { get; }
        
        /// <summary>
        /// Gets the interaction range for this crafting station.
        /// </summary>
        public float InteractionRange { get; }
        
        /// <summary>
        /// Gets or sets the entity that is currently using this crafting station.
        /// </summary>
        public Entity CurrentUser { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether the station is currently in use.
        /// </summary>
        public bool InUse => CurrentUser != null;
        
        /// <summary>
        /// Event raised when the station is used.
        /// </summary>
        public event EventHandler<StationUsedEventArgs> StationUsed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CraftingStationComponent"/> class.
        /// </summary>
        /// <param name="stationType">The type of crafting station.</param>
        /// <param name="interactionRange">The interaction range for this station.</param>
        public CraftingStationComponent(CraftingStationType stationType, float interactionRange = 100f)
        {
            if (stationType == CraftingStationType.None)
                throw new ArgumentException("Crafting station type cannot be None", nameof(stationType));
            
            StationType = stationType;
            InteractionRange = interactionRange;
            CurrentUser = null;
        }
        
        /// <summary>
        /// Sets the current user of this crafting station.
        /// </summary>
        /// <param name="user">The entity using the station, or null to clear.</param>
        /// <returns>True if the user was set or cleared successfully, false if already in use.</returns>
        public bool SetUser(Entity user)
        {
            // If we're clearing the user, always allow it
            if (user == null)
            {
                CurrentUser = null;
                return true;
            }
            
            // Don't allow if already in use by someone else
            if (InUse && CurrentUser != user)
                return false;
            
            // Set the user
            CurrentUser = user;
            OnStationUsed(user);
            return true;
        }
        
        /// <summary>
        /// Checks if an entity is within range to use this station.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the entity is within range, otherwise false.</returns>
        public bool IsEntityInRange(Entity entity)
        {
            if (entity == null || Owner == null)
                return false;
            
            // Both entities need transform components
            if (!entity.HasComponent<TransformComponent>() || !Owner.HasComponent<TransformComponent>())
                return false;
            
            var entityTransform = entity.GetComponent<TransformComponent>();
            var stationTransform = Owner.GetComponent<TransformComponent>();
            
            // Calculate distance
            float distance = Vector2.Distance(entityTransform.Position, stationTransform.Position);
            
            return distance <= InteractionRange;
        }
        
        /// <summary>
        /// Raises the StationUsed event.
        /// </summary>
        /// <param name="user">The entity using the station.</param>
        protected virtual void OnStationUsed(Entity user)
        {
            StationUsed?.Invoke(this, new StationUsedEventArgs(user));
        }
    }
    
    /// <summary>
    /// Event arguments for station used events.
    /// </summary>
    public class StationUsedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the entity using the station.
        /// </summary>
        public Entity User { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StationUsedEventArgs"/> class.
        /// </summary>
        /// <param name="user">The entity using the station.</param>
        public StationUsedEventArgs(Entity user)
        {
            User = user;
        }
    }
}
