using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using ProtocolEMR.Core.Inventory;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.UI
{
    public class InventoryUIManager : MonoBehaviour
    {
        public static InventoryUIManager Instance { get; private set; }

        [Header("Inventory Canvas")]
        [SerializeField] private Canvas inventoryCanvas;
        [SerializeField] private CanvasGroup inventoryCanvasGroup;
        [SerializeField] private float animationDuration = 0.4f;

        [Header("Grid Layout")]
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private int columnsCount = 5;
        [SerializeField] private int rowsCount = 4;

        [Header("Item Details Panel")]
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemDescriptionText;
        [SerializeField] private Text itemStatsText;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private Text itemWeightText;
        [SerializeField] private Text itemTypeText;

        [Header("Quick Slots")]
        [SerializeField] private Transform quickSlotsContainer;
        [SerializeField] private GameObject quickSlotPrefab;
        [SerializeField] private Text weightText;

        [Header("Sorting & Filtering")]
        [SerializeField] private Dropdown sortDropdown;
        [SerializeField] private Dropdown filterDropdown;
        [SerializeField] private InputField searchInputField;

        [Header("Equipped Items Display")]
        [SerializeField] private Image equippedMeleeIcon;
        [SerializeField] private Image equippedRangedIcon;

        private Dictionary<int, InventorySlotUI> slotUIMap;
        private List<ItemData> filteredItems;
        private ItemData selectedItem;
        private bool isOpen = false;
        private int totalSlots;

        public event Action<ItemData> OnItemSelected;
        public event Action OnInventoryToggled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            slotUIMap = new Dictionary<int, InventorySlotUI>();
            filteredItems = new List<ItemData>();
            totalSlots = columnsCount * rowsCount;
        }

        private void Start()
        {
            InitializeInventoryUI();

            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventory += ToggleInventory;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshInventoryDisplay;
            }

            inventoryCanvasGroup.alpha = 0f;
            inventoryCanvas.enabled = false;
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventory -= ToggleInventory;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryDisplay;
            }
        }

        private void InitializeInventoryUI()
        {
            CreateInventorySlots();
            CreateQuickSlots();
            SetupSortingAndFiltering();
        }

        private void CreateInventorySlots()
        {
            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObj = Instantiate(inventorySlotPrefab, slotsContainer);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Initialize(i, OnSlotClicked);
                    slotUIMap[i] = slotUI;
                }
            }

            if (gridLayout != null)
            {
                gridLayout.constraintCount = columnsCount;
            }
        }

        private void CreateQuickSlots()
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject quickSlotObj = Instantiate(quickSlotPrefab, quickSlotsContainer);
                QuickSlotUI quickSlotUI = quickSlotObj.GetComponent<QuickSlotUI>();
                if (quickSlotUI != null)
                {
                    quickSlotUI.Initialize(i, OnQuickSlotClicked);
                }
            }
        }

        private void SetupSortingAndFiltering()
        {
            if (sortDropdown != null)
            {
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }

            if (filterDropdown != null)
            {
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (searchInputField != null)
            {
                searchInputField.onValueChanged.AddListener(OnSearchChanged);
            }
        }

        public void ToggleInventory()
        {
            if (isOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        public void OpenInventory()
        {
            if (isOpen) return;

            isOpen = true;
            inventoryCanvas.enabled = true;
            StartCoroutine(AnimateInventoryOpen());
            RefreshInventoryDisplay();
            OnInventoryToggled?.Invoke();
        }

        public void CloseInventory()
        {
            if (!isOpen) return;

            isOpen = false;
            StartCoroutine(AnimateInventoryClose());
        }

        private System.Collections.IEnumerator AnimateInventoryOpen()
        {
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                inventoryCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
                yield return null;
            }
            inventoryCanvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator AnimateInventoryClose()
        {
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                inventoryCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
                yield return null;
            }
            inventoryCanvasGroup.alpha = 0f;
            inventoryCanvas.enabled = false;
        }

        private void RefreshInventoryDisplay()
        {
            if (InventoryManager.Instance == null) return;

            List<ItemData> items = InventoryManager.Instance.Items.ToList();

            ApplyFiltersAndSorting(items);

            for (int i = 0; i < totalSlots; i++)
            {
                if (slotUIMap.TryGetValue(i, out InventorySlotUI slotUI))
                {
                    if (i < filteredItems.Count)
                    {
                        slotUI.SetItem(filteredItems[i]);
                    }
                    else
                    {
                        slotUI.Clear();
                    }
                }
            }

            UpdateWeightDisplay();
            UpdateEquippedItemsDisplay();
        }

        private void ApplyFiltersAndSorting(List<ItemData> items)
        {
            filteredItems = items;

            // Apply filter
            if (filterDropdown != null && filterDropdown.value > 0)
            {
                ItemType filterType = (ItemType)filterDropdown.value - 1;
                filteredItems = filteredItems.Where(x => x.itemType == filterType).ToList();
            }

            // Apply search
            if (searchInputField != null && !string.IsNullOrEmpty(searchInputField.text))
            {
                string searchTerm = searchInputField.text.ToLower();
                filteredItems = filteredItems.Where(x => x.itemName.ToLower().Contains(searchTerm)).ToList();
            }

            // Apply sorting
            if (sortDropdown != null)
            {
                switch (sortDropdown.value)
                {
                    case 1:
                        filteredItems = filteredItems.OrderBy(x => x.itemName).ToList();
                        break;
                    case 2:
                        filteredItems = filteredItems.OrderBy(x => x.weight).ToList();
                        break;
                    case 3:
                        filteredItems = filteredItems.OrderByDescending(x => x.rarity).ToList();
                        break;
                }
            }
        }

        private void UpdateWeightDisplay()
        {
            if (weightText != null && InventoryManager.Instance != null)
            {
                float currentWeight = InventoryManager.Instance.CurrentWeight;
                float maxWeight = 50f;
                weightText.text = $"Weight: {currentWeight:F1}/{maxWeight:F1}";
            }
        }

        private void UpdateEquippedItemsDisplay()
        {
            if (InventoryManager.Instance == null) return;

            ItemData[] equipped = InventoryManager.Instance.EquippedItems;

            // Display equipped melee weapon
            if (equipped[(int)EquipmentSlot.Chest] != null && equipped[(int)EquipmentSlot.Chest] is WeaponData meleeWeapon)
            {
                if (equippedMeleeIcon != null && !string.IsNullOrEmpty(meleeWeapon.iconPath))
                {
                    equippedMeleeIcon.sprite = Resources.Load<Sprite>(meleeWeapon.iconPath);
                }
            }

            // Display equipped ranged weapon
            if (equipped[(int)EquipmentSlot.Head] != null && equipped[(int)EquipmentSlot.Head] is WeaponData rangedWeapon)
            {
                if (equippedRangedIcon != null && !string.IsNullOrEmpty(rangedWeapon.iconPath))
                {
                    equippedRangedIcon.sprite = Resources.Load<Sprite>(rangedWeapon.iconPath);
                }
            }
        }

        private void DisplayItemDetails(ItemData item)
        {
            if (item == null)
            {
                ClearItemDetails();
                return;
            }

            selectedItem = item;

            if (itemNameText != null)
                itemNameText.text = item.itemName;

            if (itemDescriptionText != null)
                itemDescriptionText.text = item.description;

            if (itemWeightText != null)
                itemWeightText.text = $"Weight: {item.weight}";

            if (itemTypeText != null)
                itemTypeText.text = $"Type: {item.itemType}";

            if (itemIconImage != null && !string.IsNullOrEmpty(item.iconPath))
            {
                itemIconImage.sprite = Resources.Load<Sprite>(item.iconPath);
            }

            if (item is EquipmentData equipData && itemStatsText != null)
            {
                itemStatsText.text = $"Damage: {equipData.damageValue}\nDefense: {equipData.defenseValue}\nDurability: {equipData.durability}/{equipData.maxDurability}";
            }

            OnItemSelected?.Invoke(item);
        }

        private void ClearItemDetails()
        {
            if (itemNameText != null)
                itemNameText.text = "Select an item";

            if (itemDescriptionText != null)
                itemDescriptionText.text = "";

            if (itemStatsText != null)
                itemStatsText.text = "";

            if (itemWeightText != null)
                itemWeightText.text = "";

            if (itemTypeText != null)
                itemTypeText.text = "";

            if (itemIconImage != null)
                itemIconImage.sprite = null;
        }

        private void OnSlotClicked(InventorySlotUI slot)
        {
            DisplayItemDetails(slot.Item);
        }

        private void OnQuickSlotClicked(QuickSlotUI quickSlot, int slotIndex)
        {
            if (selectedItem != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AssignToQuickSlot(selectedItem, slotIndex);
            }
        }

        private void OnSortChanged(int index)
        {
            RefreshInventoryDisplay();
        }

        private void OnFilterChanged(int index)
        {
            RefreshInventoryDisplay();
        }

        private void OnSearchChanged(string searchText)
        {
            RefreshInventoryDisplay();
        }
    }
}
