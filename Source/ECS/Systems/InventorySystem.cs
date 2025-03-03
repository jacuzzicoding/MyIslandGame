using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MyIslandGame.ECS.Components;
using MyIslandGame.Input;

namespace MyIslandGame.ECS.Systems
{
    /// <summary>
    /// System that manages inventory-related functionality.
    /// </summary>
    public class InventorySystem : System
    {
        private readonly InputManager _inputManager;
        private bool _inventoryVisible = false;

        /// <summary>
        /// Gets a value indicating whether the inventory UI is visible.
        /// </summary>
        public bool InventoryVisible => _inventoryVisible;

        /// <summary>
        /// Event raised when the inventory visibility changes.
        /// </summary>
        public event EventHandler<bool> InventoryVisibilityChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventorySystem"/> class.
        /// </summary>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="inputManager">The input manager.</param>
        public InventorySystem(EntityManager entityManager, InputManager inputManager)
            : base(entityManager)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
        }

        /// <summary>
        /// Initializes the inventory input actions.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            // Ensure the input actions for inventory are registered
            RegisterInventoryInputActions();
        }

        /// <summary>
        /// Updates this system.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            var entities = GetInterestingEntities();
            
            foreach (var entity in entities)
            {
                var inventoryComponent = entity.GetComponent<InventoryComponent>();
                
                // Handle hotbar selection with number keys
                for (int i = 0; i < inventoryComponent.Inventory.HotbarSize; i++)
                {
                    if (_inputManager.WasActionPressed($"Hotbar{i + 1}"))
                    {
                        inventoryComponent.SelectHotbarSlot(i);
                        break;
                    }
                }
                
                // Handle scrolling through hotbar
                if (_inputManager.WasActionPressed("ScrollHotbarLeft"))
                {
                    int newIndex = (inventoryComponent.SelectedHotbarIndex - 1 + inventoryComponent.Inventory.HotbarSize) % inventoryComponent.Inventory.HotbarSize;
                    inventoryComponent.SelectHotbarSlot(newIndex);
                }
                else if (_inputManager.WasActionPressed("ScrollHotbarRight"))
                {
                    int newIndex = (inventoryComponent.SelectedHotbarIndex + 1) % inventoryComponent.Inventory.HotbarSize;
                    inventoryComponent.SelectHotbarSlot(newIndex);
                }
                
                // Toggle inventory visibility
                if (_inputManager.WasActionPressed("ToggleInventory"))
                {
                    _inventoryVisible = !_inventoryVisible;
                    OnInventoryVisibilityChanged(_inventoryVisible);
                }
                
                // Use selected item
                if (_inputManager.WasActionPressed("UseItem"))
                {
                    inventoryComponent.UseSelectedItem();
                }
                
                // Drop selected item
                if (_inputManager.WasActionPressed("DropItem") && !_inventoryVisible)
                {
                    var selectedItem = inventoryComponent.GetSelectedItem();
                    
                    if (selectedItem != null)
                    {
                        // Logic for dropping items will be implemented later
                        // For now, just remove from inventory
                        inventoryComponent.Inventory.RemoveItem(selectedItem.Id, 1);
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
            return entity.HasComponent<InventoryComponent>();
        }

        /// <summary>
        /// Shows or hides the inventory UI.
        /// </summary>
        /// <param name="visible">True to show the inventory, false to hide it.</param>
        public void SetInventoryVisible(bool visible)
        {
            if (_inventoryVisible != visible)
            {
                _inventoryVisible = visible;
                OnInventoryVisibilityChanged(_inventoryVisible);
            }
        }

        /// <summary>
        /// Raises the InventoryVisibilityChanged event.
        /// </summary>
        /// <param name="visible">True if the inventory is visible, false otherwise.</param>
        protected virtual void OnInventoryVisibilityChanged(bool visible)
        {
            InventoryVisibilityChanged?.Invoke(this, visible);
        }

        /// <summary>
        /// Registers the input actions for inventory management.
        /// </summary>
        private void RegisterInventoryInputActions()
        {
            // Register hotbar number keys (1-9)
            for (int i = 1; i <= 9; i++)
            {
                Keys key = Keys.D1 + (i - 1);
                _inputManager.RegisterAction($"Hotbar{i}", new InputAction().MapKey(key));
            }
            
            // Register hotbar scrolling
            _inputManager.RegisterAction("ScrollHotbarLeft", new InputAction().MapMouseButton(MouseButton.ScrollUp));
            _inputManager.RegisterAction("ScrollHotbarRight", new InputAction().MapMouseButton(MouseButton.ScrollDown));
            
            // Register inventory toggle
            _inputManager.RegisterAction("ToggleInventory", new InputAction().MapKey(Keys.E));
            
            // Register item use/drop
            _inputManager.RegisterAction("UseItem", new InputAction().MapMouseButton(MouseButton.Left));
            _inputManager.RegisterAction("DropItem", new InputAction().MapKey(Keys.Q));
        }
    }
}
