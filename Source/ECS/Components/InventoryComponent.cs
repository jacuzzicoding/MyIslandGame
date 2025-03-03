using System;
using MyIslandGame.Inventory;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that provides inventory capabilities to an entity.
    /// </summary>
    public class InventoryComponent : Component
    {
        /// <summary>
        /// Gets the inventory managed by this component.
        /// </summary>
        public Inventory.Inventory Inventory { get; }

        /// <summary>
        /// Gets or sets the currently selected hotbar index.
        /// </summary>
        public int SelectedHotbarIndex { get; set; }

        /// <summary>
        /// Event raised when the selected hotbar index changes.
        /// </summary>
        public event EventHandler<EventArgs> HotbarSelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryComponent"/> class.
        /// </summary>
        /// <param name="inventorySize">The total size of the inventory.</param>
        /// <param name="hotbarSize">The size of the hotbar (quick access slots).</param>
        public InventoryComponent(int inventorySize = 27, int hotbarSize = 9)
        {
            Inventory = new Inventory.Inventory(inventorySize, hotbarSize);
            SelectedHotbarIndex = 0;
        }

        /// <summary>
        /// Gets the currently selected item from the hotbar.
        /// </summary>
        /// <returns>The selected item, or null if no item is selected.</returns>
        public Item GetSelectedItem()
        {
            return Inventory.GetSelectedHotbarItem(SelectedHotbarIndex);
        }

        /// <summary>
        /// Gets the quantity of the currently selected item.
        /// </summary>
        /// <returns>The quantity of the selected item, or 0 if no item is selected.</returns>
        public int GetSelectedItemQuantity()
        {
            return Inventory.GetSelectedHotbarItemQuantity(SelectedHotbarIndex);
        }

        /// <summary>
        /// Tries to use the currently selected item.
        /// </summary>
        /// <returns>True if the item was used successfully, otherwise false.</returns>
        public bool UseSelectedItem()
        {
            Item selectedItem = GetSelectedItem();
            
            if (selectedItem == null)
            {
                return false;
            }

            bool success = selectedItem.Use(Owner);
            
            // If the item is a tool, handle durability
            if (success && selectedItem is Tool tool)
            {
                tool.UseTool();
                
                // Remove broken tools
                if (tool.IsBroken)
                {
                    Inventory.RemoveItem(tool.Id);
                }
            }
            
            return success;
        }

        /// <summary>
        /// Changes the selected hotbar index.
        /// </summary>
        /// <param name="index">The new hotbar index.</param>
        public void SelectHotbarSlot(int index)
        {
            if (index < 0 || index >= Inventory.HotbarSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Hotbar index is out of range.");
            }

            if (SelectedHotbarIndex != index)
            {
                SelectedHotbarIndex = index;
                OnHotbarSelectionChanged();
            }
        }

        /// <summary>
        /// Gets the tool type of the currently selected item.
        /// </summary>
        /// <returns>The tool type, or null if the selected item is not a tool.</returns>
        public string GetSelectedToolType()
        {
            Item selectedItem = GetSelectedItem();
            
            if (selectedItem is Tool tool)
            {
                return tool.ToolType;
            }
            
            return null;
        }

        /// <summary>
        /// Raises the HotbarSelectionChanged event.
        /// </summary>
        protected virtual void OnHotbarSelectionChanged()
        {
            HotbarSelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
