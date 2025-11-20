using UnityEngine;
using System.Collections.Generic;
using System;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Manages the player's inventory including item storage, equipped items, and quick slots.
    /// Singleton pattern for global access to inventory.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private int maxInventorySlots = 20;
        [SerializeField] private int quickSlotsCount = 4;
        [SerializeField] private float maxCarryWeight = 100f;

        private List<Item> inventory = new List<Item>();
        private Item[] quickSlots;
        private Item equippedWeapon;
        private Item equippedTool;
        private float currentWeight;

        // Events
        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action<Item> OnItemEquipped;
        public event Action OnInventoryChanged;

        private static InventoryManager instance;
        public static InventoryManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("InventoryManager");
                    instance = managerObject.AddComponent<InventoryManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            InitializeInventory();
        }

        private void InitializeInventory()
        {
            inventory.Clear();
            quickSlots = new Item[quickSlotsCount];
            currentWeight = 0f;
        }

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        public bool AddItem(Item item, int count = 1)
        {
            if (item == null) return false;

            // Check weight
            float itemTotalWeight = item.Weight * count;
            if (currentWeight + itemTotalWeight > maxCarryWeight)
            {
                Debug.LogWarning($"Cannot add {item.Name}: inventory too heavy!");
                return false;
            }

            // Check if stackable and if we can add to existing stack
            if (item.IsStackable)
            {
                Item existingStack = inventory.Find(i => i.ID == item.ID);
                if (existingStack != null)
                {
                    int canAdd = existingStack.MaxStackSize - existingStack.CurrentStack;
                    if (canAdd >= count)
                    {
                        existingStack.CurrentStack += count;
                        currentWeight += itemTotalWeight;
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                    else
                    {
                        existingStack.CurrentStack = existingStack.MaxStackSize;
                        count -= canAdd;
                        currentWeight += item.Weight * canAdd;
                    }
                }
            }

            // Add as new item if not stackable or stack full
            if (inventory.Count < maxInventorySlots)
            {
                Item newItem = item.Clone(count);
                inventory.Add(newItem);
                currentWeight += itemTotalWeight;
                OnItemAdded?.Invoke(newItem);
                OnInventoryChanged?.Invoke();
                return true;
            }

            Debug.LogWarning($"Inventory is full! Cannot add {item.Name}");
            return false;
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        public bool RemoveItem(string itemId, int count = 1)
        {
            Item item = inventory.Find(i => i.ID == itemId);
            if (item == null) return false;

            if (item.CurrentStack > count)
            {
                item.CurrentStack -= count;
                currentWeight -= item.Weight * count;
            }
            else
            {
                currentWeight -= item.Weight * item.CurrentStack;
                inventory.Remove(item);
                
                // Remove from quick slots if equipped
                for (int i = 0; i < quickSlots.Length; i++)
                {
                    if (quickSlots[i] == item)
                    {
                        quickSlots[i] = null;
                    }
                }

                if (equippedWeapon == item) equippedWeapon = null;
                if (equippedTool == item) equippedTool = null;

                OnItemRemoved?.Invoke(item);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Gets all items in inventory.
        /// </summary>
        public List<Item> GetInventory()
        {
            return new List<Item>(inventory);
        }

        /// <summary>
        /// Gets an item by ID.
        /// </summary>
        public Item GetItem(string itemId)
        {
            return inventory.Find(i => i.ID == itemId);
        }

        /// <summary>
        /// Gets all items of a specific type.
        /// </summary>
        public List<Item> GetItemsByType(Item.ItemType type)
        {
            return inventory.FindAll(i => i.Type == type);
        }

        /// <summary>
        /// Gets all tools of a specific tool type.
        /// </summary>
        public List<Item> GetToolsByType(Item.ToolType toolType)
        {
            return inventory.FindAll(i => i.Type == Item.ItemType.Tool && i.Tool == toolType);
        }

        /// <summary>
        /// Equips a weapon from inventory.
        /// </summary>
        public bool EquipWeapon(string weaponId)
        {
            Item weapon = GetItem(weaponId);
            if (weapon == null || !weapon.IsWeapon) return false;

            equippedWeapon = weapon;
            OnItemEquipped?.Invoke(weapon);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Equips a tool from inventory.
        /// </summary>
        public bool EquipTool(string toolId)
        {
            Item tool = GetItem(toolId);
            if (tool == null || !tool.IsTool) return false;

            equippedTool = tool;
            OnItemEquipped?.Invoke(tool);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Gets currently equipped weapon.
        /// </summary>
        public Item GetEquippedWeapon()
        {
            return equippedWeapon;
        }

        /// <summary>
        /// Gets currently equipped tool.
        /// </summary>
        public Item GetEquippedTool()
        {
            return equippedTool;
        }

        /// <summary>
        /// Adds item to quick slot.
        /// </summary>
        public bool AddToQuickSlot(string itemId, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= quickSlots.Length) return false;

            Item item = GetItem(itemId);
            if (item == null) return false;

            quickSlots[slotIndex] = item;
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Gets item from quick slot.
        /// </summary>
        public Item GetQuickSlotItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= quickSlots.Length) return null;
            return quickSlots[slotIndex];
        }

        /// <summary>
        /// Gets all quick slot items.
        /// </summary>
        public Item[] GetQuickSlots()
        {
            return new Item[quickSlots.Length];
        }

        /// <summary>
        /// Gets current inventory weight.
        /// </summary>
        public float GetCurrentWeight()
        {
            return currentWeight;
        }

        /// <summary>
        /// Gets maximum carry weight.
        /// </summary>
        public float GetMaxWeight()
        {
            return maxCarryWeight;
        }

        /// <summary>
        /// Gets inventory slot count.
        /// </summary>
        public int GetSlotCount()
        {
            return inventory.Count;
        }

        /// <summary>
        /// Gets maximum inventory slots.
        /// </summary>
        public int GetMaxSlots()
        {
            return maxInventorySlots;
        }

        /// <summary>
        /// Checks if inventory is full.
        /// </summary>
        public bool IsInventoryFull()
        {
            return inventory.Count >= maxInventorySlots;
        }

        /// <summary>
        /// Clears entire inventory.
        /// </summary>
        public void ClearInventory()
        {
            inventory.Clear();
            quickSlots = new Item[quickSlotsCount];
            equippedWeapon = null;
            equippedTool = null;
            currentWeight = 0f;
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Checks if player has a tool of specified type.
        /// </summary>
        public bool HasTool(Item.ToolType toolType)
        {
            return inventory.Exists(i => i.Type == Item.ItemType.Tool && i.Tool == toolType);
        }
    }
}
