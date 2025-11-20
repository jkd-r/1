using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtocolEMR.Core.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private int inventorySlotCount = 20;
        [SerializeField] private float maxCarryWeight = 50f;

        private List<ItemData> inventoryItems;
        private ItemData[] equippedItems;
        private ItemData[] quickSlots;

        public event Action<ItemData> OnItemAdded;
        public event Action<ItemData> OnItemRemoved;
        public event Action<ItemData> OnItemEquipped;
        public event Action<ItemData> OnItemUnequipped;
        public event Action OnInventoryChanged;

        public float CurrentWeight { get; private set; }
        public IReadOnlyList<ItemData> Items => inventoryItems.AsReadOnly();
        public ItemData[] EquippedItems => equippedItems;
        public ItemData[] QuickSlots => quickSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeInventory();
        }

        private void InitializeInventory()
        {
            inventoryItems = new List<ItemData>();
            equippedItems = new ItemData[System.Enum.GetValues(typeof(EquipmentSlot)).Length];
            quickSlots = new ItemData[4];
        }

        public bool AddItem(ItemData item)
        {
            if (item == null) return false;

            float totalWeight = CurrentWeight + item.weight;
            if (totalWeight > maxCarryWeight)
            {
                Debug.LogWarning($"Cannot add {item.itemName} - inventory full!");
                return false;
            }

            if (item.isStackable)
            {
                ItemData existingStack = inventoryItems.FirstOrDefault(x => x.itemId == item.itemId);
                if (existingStack != null && existingStack.quantity < item.maxStackSize)
                {
                    existingStack.quantity += item.quantity;
                    CurrentWeight += item.weight;
                    OnItemAdded?.Invoke(item);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }

            inventoryItems.Add(item);
            CurrentWeight += item.weight;
            OnItemAdded?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null || !inventoryItems.Contains(item))
                return false;

            if (item.isStackable && item.quantity > quantity)
            {
                item.quantity -= quantity;
                CurrentWeight -= item.weight * quantity;
            }
            else
            {
                inventoryItems.Remove(item);
                CurrentWeight -= item.weight;
            }

            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool EquipItem(ItemData item, EquipmentSlot slot)
        {
            if (!(item is EquipmentData equipmentData))
                return false;

            if (equippedItems[(int)slot] != null)
            {
                UnequipItem(slot);
            }

            equippedItems[(int)slot] = item;
            OnItemEquipped?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool UnequipItem(EquipmentSlot slot)
        {
            if (equippedItems[(int)slot] == null)
                return false;

            ItemData unequipped = equippedItems[(int)slot];
            equippedItems[(int)slot] = null;
            OnItemUnequipped?.Invoke(unequipped);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool AssignToQuickSlot(ItemData item, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= quickSlots.Length)
                return false;

            quickSlots[slotIndex] = item;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public ItemData GetQuickSlotItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= quickSlots.Length)
                return null;
            return quickSlots[slotIndex];
        }

        public List<ItemData> GetItemsByType(ItemType itemType)
        {
            return inventoryItems.Where(item => item.itemType == itemType).ToList();
        }

        public List<ItemData> GetItemsByTag(string tag)
        {
            return inventoryItems.Where(item => item.tags != null && item.tags.Contains(tag)).ToList();
        }

        public int GetItemCount(string itemId)
        {
            ItemData item = inventoryItems.FirstOrDefault(x => x.itemId == itemId);
            return item?.quantity ?? 0;
        }

        public void ClearInventory()
        {
            inventoryItems.Clear();
            CurrentWeight = 0;
            OnInventoryChanged?.Invoke();
        }

        public void SetMaxCarryWeight(float newMaxWeight)
        {
            maxCarryWeight = newMaxWeight;
        }

        public bool CanAddItem(ItemData item)
        {
            return (CurrentWeight + item.weight) <= maxCarryWeight;
        }
    }
}
