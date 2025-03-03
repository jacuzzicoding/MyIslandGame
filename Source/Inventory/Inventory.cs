using System;
using System.Collections.Generic;
using System.Linq;
using MyIslandGame.Core.Resources;

namespace MyIslandGame.Inventory
{
    /// <summary>
    /// Represents an inventory that can store items.
    /// </summary>
    public class Inventory
    {
        private readonly InventorySlot[] _slots;
        private readonly int _hotbarSize;

        /// <summary>
        /// Gets the total size of the inventory.
        /// </summary>
        public int Size => _slots.Length;

        /// <summary>
        /// Gets the size of the hotbar (quick access slots).
        /// </summary>
        public int HotbarSize => _hotbarSize;

        /// <summary>
        /// Event raised when the inventory contents change.
        /// </summary>
        public event EventHandler<InventoryChangedEventArgs> InventoryChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="size">The total size of the inventory.</param>
        /// <param name="hotbarSize">The size of the hotbar (quick access slots).</param>
        public Inventory(int size = 27, int hotbarSize = 9)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Inventory size must be positive.");
            }

            if (hotbarSize < 0 || hotbarSize > size)
            {
                throw new ArgumentOutOfRangeException(nameof(hotbarSize), "Hotbar size must be between 0 and inventory size.");
            }

            _slots = new InventorySlot[size];
            _hotbarSize = hotbarSize;

            // Initialize slots
            for (int i = 0; i < size; i++)
            {
                _slots[i] = new InventorySlot();
            }
        }

        /// <summary>
        /// Gets the slot at the specified index.
        /// </summary>
        /// <param name="index">The index of the slot to get.</param>
        /// <returns>The inventory slot at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Length)
            {
                throw new IndexOutOfRangeException($"Slot index {index} is out of range.");
            }

            return _slots[index];
        }

        /// <summary>
        /// Gets the hotbar slots (first n slots).
        /// </summary>
        /// <returns>An enumerable collection of hotbar slots.</returns>
        public IEnumerable<InventorySlot> GetHotbarSlots()
        {
            return _slots.Take(_hotbarSize);
        }

        /// <summary>
        /// Gets the main inventory slots (excluding hotbar).
        /// </summary>
        /// <returns>An enumerable collection of main inventory slots.</returns>
        public IEnumerable<InventorySlot> GetMainInventorySlots()
        {
            return _slots.Skip(_hotbarSize);
        }

        /// <summary>
        /// Gets all inventory slots.
        /// </summary>
        /// <returns>An enumerable collection of all inventory slots.</returns>
        public IEnumerable<InventorySlot> GetAllSlots()
        {
            return _slots;
        }

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The quantity of the item to add.</param>
        /// <returns>True if the item was added successfully, otherwise false.</returns>
        public bool TryAddItem(Item item, int quantity = 1)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
            }

            int remainingQuantity = quantity;

            // First try to stack with existing items
            foreach (var slot in _slots)
            {
                if (slot.CanStack(item) && !slot.IsFull)
                {
                    int added = slot.AddItem(item, remainingQuantity);
                    remainingQuantity -= added;

                    if (remainingQuantity <= 0)
                    {
                        OnInventoryChanged(InventoryChangeType.ItemAdded);
                        return true;
                    }
                }
            }

            // Then try to find empty slots
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    int added = slot.AddItem(item, remainingQuantity);
                    remainingQuantity -= added;

                    if (remainingQuantity <= 0)
                    {
                        OnInventoryChanged(InventoryChangeType.ItemAdded);
                        return true;
                    }
                }
            }

            // If we still have items to add, we couldn't fit everything
            if (remainingQuantity < quantity)
            {
                OnInventoryChanged(InventoryChangeType.ItemAdded);
            }

            return remainingQuantity <= 0;
        }

        /// <summary>
        /// Adds a resource to the inventory.
        /// </summary>
        /// <param name="resource">The resource to add.</param>
        /// <param name="quantity">The quantity of the resource to add.</param>
        /// <returns>True if the resource was added successfully, otherwise false.</returns>
        public bool TryAddResource(Resource resource, int quantity = 1)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            Item item = Item.FromResource(resource);
            return TryAddItem(item, quantity);
        }

        /// <summary>
        /// Checks if the inventory has the specified item.
        /// </summary>
        /// <param name="itemId">The ID of the item to check for.</param>
        /// <param name="quantity">The quantity of the item to check for.</param>
        /// <returns>True if the inventory has the specified quantity of the item, otherwise false.</returns>
        public bool HasItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                throw new ArgumentException("Item ID cannot be null or empty.", nameof(itemId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
            }

            int totalQuantity = 0;

            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Item.Id == itemId)
                {
                    totalQuantity += slot.Quantity;

                    if (totalQuantity >= quantity)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the specified item from the inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to remove.</param>
        /// <param name="quantity">The quantity of the item to remove.</param>
        /// <returns>True if the item was removed successfully, otherwise false.</returns>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                throw new ArgumentException("Item ID cannot be null or empty.", nameof(itemId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
            }

            // Check if we have enough of the item
            if (!HasItem(itemId, quantity))
            {
                return false;
            }

            int remainingQuantity = quantity;

            // Remove items starting from last slot (to avoid shifting items in UI)
            for (int i = _slots.Length - 1; i >= 0; i--)
            {
                var slot = _slots[i];

                if (!slot.IsEmpty && slot.Item.Id == itemId)
                {
                    int removed = slot.RemoveItem(remainingQuantity);
                    remainingQuantity -= removed;

                    if (remainingQuantity <= 0)
                    {
                        OnInventoryChanged(InventoryChangeType.ItemRemoved);
                        return true;
                    }
                }
            }

            // Should never reach here if HasItem check passed
            return false;
        }

        /// <summary>
        /// Swaps the contents of two inventory slots.
        /// </summary>
        /// <param name="sourceIndex">The index of the source slot.</param>
        /// <param name="destinationIndex">The index of the destination slot.</param>
        /// <returns>True if the swap was successful, otherwise false.</returns>
        public bool SwapSlots(int sourceIndex, int destinationIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), "Source index is out of range.");
            }

            if (destinationIndex < 0 || destinationIndex >= _slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), "Destination index is out of range.");
            }

            if (sourceIndex == destinationIndex)
            {
                return true; // No swap needed
            }

            var sourceSlot = _slots[sourceIndex];
            var destinationSlot = _slots[destinationIndex];

            // Try to stack if items are the same
            if (!sourceSlot.IsEmpty && !destinationSlot.IsEmpty && 
                sourceSlot.Item.CanStack(destinationSlot.Item))
            {
                // If destination can hold all source items, move them there
                if (destinationSlot.Quantity + sourceSlot.Quantity <= destinationSlot.Item.MaxStackSize)
                {
                    destinationSlot.AddItem(sourceSlot.Item, sourceSlot.Quantity);
                    sourceSlot.Clear();
                    OnInventoryChanged(InventoryChangeType.ItemsMoved);
                    return true;
                }
                // Otherwise, fill destination and keep remainder in source
                else
                {
                    int spaceInDestination = destinationSlot.Item.MaxStackSize - destinationSlot.Quantity;
                    if (spaceInDestination > 0)
                    {
                        destinationSlot.AddItem(sourceSlot.Item, spaceInDestination);
                        sourceSlot.RemoveItem(spaceInDestination);
                        OnInventoryChanged(InventoryChangeType.ItemsMoved);
                        return true;
                    }
                }
            }

            // Swap slots
            Item tempItem = sourceSlot.Item;
            int tempQuantity = sourceSlot.Quantity;

            sourceSlot.Clear();
            
            if (!destinationSlot.IsEmpty)
            {
                sourceSlot.AddItem(destinationSlot.Item, destinationSlot.Quantity);
            }

            destinationSlot.Clear();
            
            if (tempItem != null)
            {
                destinationSlot.AddItem(tempItem, tempQuantity);
            }

            OnInventoryChanged(InventoryChangeType.ItemsMoved);
            return true;
        }

        /// <summary>
        /// Gets the selected hotbar item based on the hotbar index.
        /// </summary>
        /// <param name="hotbarIndex">The hotbar index (0-based).</param>
        /// <returns>The selected item, or null if the slot is empty.</returns>
        public Item GetSelectedHotbarItem(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= _hotbarSize)
            {
                throw new ArgumentOutOfRangeException(nameof(hotbarIndex), "Hotbar index is out of range.");
            }

            return _slots[hotbarIndex].IsEmpty ? null : _slots[hotbarIndex].Item;
        }

        /// <summary>
        /// Gets the quantity of the selected hotbar item.
        /// </summary>
        /// <param name="hotbarIndex">The hotbar index (0-based).</param>
        /// <returns>The quantity of the selected item, or 0 if the slot is empty.</returns>
        public int GetSelectedHotbarItemQuantity(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= _hotbarSize)
            {
                throw new ArgumentOutOfRangeException(nameof(hotbarIndex), "Hotbar index is out of range.");
            }

            return _slots[hotbarIndex].IsEmpty ? 0 : _slots[hotbarIndex].Quantity;
        }

        /// <summary>
        /// Clears the entire inventory.
        /// </summary>
        public void Clear()
        {
            foreach (var slot in _slots)
            {
                slot.Clear();
            }

            OnInventoryChanged(InventoryChangeType.Cleared);
        }

        /// <summary>
        /// Raises the InventoryChanged event.
        /// </summary>
        /// <param name="changeType">The type of change that occurred.</param>
        protected virtual void OnInventoryChanged(InventoryChangeType changeType)
        {
            InventoryChanged?.Invoke(this, new InventoryChangedEventArgs(changeType));
        }
    }

    /// <summary>
    /// Represents a single slot in an inventory.
    /// </summary>
    public class InventorySlot
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
        /// Initializes a new instance of the <see cref="InventorySlot"/> class.
        /// </summary>
        public InventorySlot()
        {
            Item = null;
            Quantity = 0;
        }

        /// <summary>
        /// Checks if the specified item can be stacked with the item in this slot.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item can be stacked, otherwise false.</returns>
        public bool CanStack(Item item)
        {
            return !IsEmpty && Item.CanStack(item);
        }

        /// <summary>
        /// Adds an item to this slot.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <returns>The quantity that was actually added.</returns>
        public int AddItem(Item item, int quantity)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (quantity <= 0)
            {
                return 0;
            }

            // If slot is empty, set the item
            if (IsEmpty)
            {
                Item = item;
                Quantity = Math.Min(quantity, item.MaxStackSize);
                return Quantity;
            }

            // If slot has a stackable item
            if (CanStack(item))
            {
                int spaceLeft = Item.MaxStackSize - Quantity;
                int amountToAdd = Math.Min(spaceLeft, quantity);
                Quantity += amountToAdd;
                return amountToAdd;
            }

            // Can't add to this slot
            return 0;
        }

        /// <summary>
        /// Removes a quantity of the item from this slot.
        /// </summary>
        /// <param name="quantity">The quantity to remove.</param>
        /// <returns>The quantity that was actually removed.</returns>
        public int RemoveItem(int quantity)
        {
            if (IsEmpty)
            {
                return 0;
            }

            int amountToRemove = Math.Min(Quantity, quantity);
            Quantity -= amountToRemove;

            if (Quantity <= 0)
            {
                Clear();
            }

            return amountToRemove;
        }

        /// <summary>
        /// Clears this slot.
        /// </summary>
        public void Clear()
        {
            Item = null;
            Quantity = 0;
        }
    }

    /// <summary>
    /// Enumeration of inventory change types.
    /// </summary>
    public enum InventoryChangeType
    {
        /// <summary>
        /// An item was added to the inventory.
        /// </summary>
        ItemAdded,

        /// <summary>
        /// An item was removed from the inventory.
        /// </summary>
        ItemRemoved,

        /// <summary>
        /// Items were moved within the inventory.
        /// </summary>
        ItemsMoved,

        /// <summary>
        /// The inventory was cleared.
        /// </summary>
        Cleared
    }

    /// <summary>
    /// Event arguments for inventory changes.
    /// </summary>
    public class InventoryChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the type of change that occurred.
        /// </summary>
        public InventoryChangeType ChangeType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryChangedEventArgs"/> class.
        /// </summary>
        /// <param name="changeType">The type of change that occurred.</param>
        public InventoryChangedEventArgs(InventoryChangeType changeType)
        {
            ChangeType = changeType;
        }
    }
}
