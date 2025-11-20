using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ProtocolEMR.Core.Player;
using ProtocolEMR.Core.Inventory;

using CoreInventoryManager = global::ProtocolEMR.Core.Inventory.InventoryManager;
using LegacyInventoryManager = global::ProtocolEMR.Systems.InventoryManager;
using LegacyItem = global::ProtocolEMR.Systems.Item;

namespace ProtocolEMR.Systems.SaveLoad
{
    /// <summary>
    /// Stores player-centric progression data including position, vitals, inventory, and stats.
    /// </summary>
    [Serializable]
    public class PlayerProgressionData
    {
        [Header("Transform")]
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public bool hasTransform;

        [Header("Vitals")]
        public float health = 100f;
        public float maxHealth = 100f;
        public float stamina = 100f;
        public float maxStamina = 100f;
        public float playtimeSeconds;

        [Header("Inventory")]
        public List<InventoryItemRecord> inventoryItems = new List<InventoryItemRecord>();
        public List<EquippedItemRecord> equippedItems = new List<EquippedItemRecord>();
        public List<QuickSlotRecord> quickSlots = new List<QuickSlotRecord>();
        public LegacyInventorySnapshot legacyInventory = new LegacyInventorySnapshot();

        [Header("Statistics")]
        public List<PlayerStatRecord> statistics = new List<PlayerStatRecord>();

        public static PlayerProgressionData Capture(GameObject playerOverride = null)
        {
            var data = new PlayerProgressionData();
            var player = ResolvePlayer(playerOverride);
            data.playtimeSeconds = Mathf.Max(Time.timeSinceLevelLoad, 0f);

            if (player != null)
            {
                data.position = SerializableVector3.FromVector3(player.transform.position);
                data.rotation = SerializableQuaternion.FromQuaternion(player.transform.rotation);
                data.hasTransform = true;
            }

            data.CaptureVitals(player);
            data.CaptureCoreInventory();
            data.CaptureLegacyInventory();

            return data;
        }

        public void Apply(GameObject playerOverride = null)
        {
            var player = ResolvePlayer(playerOverride);

            if (player != null && hasTransform)
            {
                var controller = player.GetComponent<CharacterController>();
                bool reEnableController = false;

                if (controller != null && controller.enabled)
                {
                    controller.enabled = false;
                    reEnableController = true;
                }

                player.transform.position = position.ToVector3();
                player.transform.rotation = rotation.ToQuaternion();

                if (reEnableController)
                {
                    controller.enabled = true;
                }
            }

            ApplyVitals(player);
            ApplyCoreInventory();
            ApplyLegacyInventory();
        }

        #region Capture Helpers

        private void CaptureVitals(GameObject player)
        {
            var healthSystem = FindHealthSystem(player);
            if (healthSystem != null)
            {
                health = healthSystem.CurrentHealth;
                maxHealth = healthSystem.MaxHealth;
                stamina = healthSystem.CurrentStamina;
                maxStamina = healthSystem.MaxStamina;
            }
            else
            {
                var staminaSystem = FindStaminaSystem(player);
                if (staminaSystem != null)
                {
                    stamina = staminaSystem.CurrentStamina;
                    maxStamina = staminaSystem.MaxStamina;
                }
            }
        }

        private void CaptureCoreInventory()
        {
            var inventory = CoreInventoryManager.Instance;
            if (inventory == null)
                return;

            inventoryItems.Clear();
            equippedItems.Clear();
            quickSlots.Clear();

            if (inventory.Items != null)
            {
                foreach (var item in inventory.Items)
                {
                    if (item == null)
                        continue;

                    inventoryItems.Add(InventoryItemRecord.FromItemData(item));
                }
            }

            if (inventory.EquippedItems != null)
            {
                for (int i = 0; i < inventory.EquippedItems.Length; i++)
                {
                    var equipped = inventory.EquippedItems[i];
                    if (equipped == null)
                        continue;

                    equippedItems.Add(new EquippedItemRecord
                    {
                        itemId = equipped.itemId,
                        slot = (EquipmentSlot)i
                    });
                }
            }

            if (inventory.QuickSlots != null)
            {
                for (int i = 0; i < inventory.QuickSlots.Length; i++)
                {
                    var quickItem = inventory.QuickSlots[i];
                    if (quickItem == null)
                        continue;

                    quickSlots.Add(new QuickSlotRecord
                    {
                        slotIndex = i,
                        itemId = quickItem.itemId
                    });
                }
            }
        }

        private void CaptureLegacyInventory()
        {
            var inventory = GetLegacyInventoryManager();
            if (inventory == null)
                return;

            legacyInventory.items.Clear();
            legacyInventory.quickSlots.Clear();
            legacyInventory.quickSlotsCapacity = Mathf.Max(1, GetLegacyQuickSlotCount(inventory));

            var legacyItems = inventory.GetInventory();
            if (legacyItems != null)
            {
                foreach (var item in legacyItems)
                {
                    if (item == null)
                        continue;

                    legacyInventory.items.Add(LegacyInventoryItemRecord.FromItem(item));
                }
            }

            for (int i = 0; i < legacyInventory.quickSlotsCapacity; i++)
            {
                var quickItem = inventory.GetQuickSlotItem(i);
                if (quickItem != null)
                {
                    legacyInventory.quickSlots.Add(new LegacyQuickSlotRecord
                    {
                        slotIndex = i,
                        itemId = quickItem.ID
                    });
                }
            }

            var weapon = inventory.GetEquippedWeapon();
            legacyInventory.equippedWeaponId = weapon != null ? weapon.ID : string.Empty;

            var tool = inventory.GetEquippedTool();
            legacyInventory.equippedToolId = tool != null ? tool.ID : string.Empty;
        }

        #endregion

        #region Apply Helpers

        private void ApplyVitals(GameObject player)
        {
            var healthSystem = FindHealthSystem(player);
            if (healthSystem != null)
            {
                if (maxHealth > 0f)
                {
                    healthSystem.SetMaxHealth(maxHealth);
                }

                if (health >= 0f)
                {
                    healthSystem.SetHealth(Mathf.Clamp(health, 0f, maxHealth > 0f ? maxHealth : healthSystem.MaxHealth));
                }

                ApplyHealthSystemStamina(healthSystem);
            }

            var staminaSystem = FindStaminaSystem(player);
            if (staminaSystem != null)
            {
                ApplyStandaloneStamina(staminaSystem);
            }
        }

        private void ApplyCoreInventory()
        {
            var inventory = CoreInventoryManager.Instance;
            if (inventory == null)
                return;

            inventory.ClearInventory();

            var cache = new Dictionary<string, ItemData>();
            foreach (var record in inventoryItems)
            {
                var rebuilt = record.ToItemData();
                if (rebuilt == null)
                    continue;

                if (string.IsNullOrEmpty(rebuilt.itemId))
                {
                    rebuilt.itemId = Guid.NewGuid().ToString();
                }

                if (inventory.AddItem(rebuilt))
                {
                    cache[rebuilt.itemId] = rebuilt;
                }
                else
                {
                    Debug.LogWarning($"[SaveLoad] Failed to restore inventory item {rebuilt.itemName}");
                }
            }

            foreach (var equipped in equippedItems)
            {
                if (equipped == null || !cache.TryGetValue(equipped.itemId, out var item))
                    continue;

                inventory.EquipItem(item, equipped.slot);
            }

            foreach (var slot in quickSlots)
            {
                if (slot == null || !cache.TryGetValue(slot.itemId, out var item))
                    continue;

                inventory.AssignToQuickSlot(item, slot.slotIndex);
            }
        }

        private void ApplyLegacyInventory()
        {
            var inventory = GetLegacyInventoryManager();
            if (inventory == null)
                return;

            inventory.ClearInventory();

            foreach (var record in legacyInventory.items)
            {
                if (record == null)
                    continue;

                var item = record.ToItem();
                if (item == null)
                    continue;

                int stackCount = Mathf.Max(1, record.currentStack);
                if (!inventory.AddItem(item, stackCount))
                {
                    Debug.LogWarning($"[SaveLoad] Failed to restore legacy item {record.name}");
                }
            }

            int slotCapacity = Mathf.Max(1, GetLegacyQuickSlotCount(inventory));
            foreach (var slot in legacyInventory.quickSlots)
            {
                if (slot == null || slot.slotIndex < 0 || slot.slotIndex >= slotCapacity)
                    continue;

                inventory.AddToQuickSlot(slot.itemId, slot.slotIndex);
            }

            if (!string.IsNullOrEmpty(legacyInventory.equippedWeaponId))
            {
                inventory.EquipWeapon(legacyInventory.equippedWeaponId);
            }

            if (!string.IsNullOrEmpty(legacyInventory.equippedToolId))
            {
                inventory.EquipTool(legacyInventory.equippedToolId);
            }
        }

        private void ApplyHealthSystemStamina(HealthSystem healthSystem)
        {
            if (healthSystem == null)
                return;

            if (maxStamina > 0f)
            {
                HealthSystemMaxStaminaField?.SetValue(healthSystem, maxStamina);
            }

            if (stamina >= 0f)
            {
                float clamped = maxStamina > 0f ? Mathf.Clamp(stamina, 0f, maxStamina) : stamina;
                HealthSystemCurrentStaminaField?.SetValue(healthSystem, clamped);
                HealthSystemStaminaTimerField?.SetValue(healthSystem, 0f);

                // Trigger UI/event updates without changing the stored value
                healthSystem.RestoreStamina(0f);
            }
        }

        private void ApplyStandaloneStamina(StaminaSystem staminaSystem)
        {
            if (staminaSystem == null)
                return;

            if (maxStamina > 0f)
            {
                StaminaSystemMaxField?.SetValue(staminaSystem, maxStamina);
            }

            if (stamina >= 0f)
            {
                float clamped = maxStamina > 0f ? Mathf.Clamp(stamina, 0f, maxStamina) : stamina;
                StaminaSystemCurrentField?.SetValue(staminaSystem, clamped);
                StaminaSystemExhaustedField?.SetValue(staminaSystem, false);
                StaminaSystemRegenTimerField?.SetValue(staminaSystem, 0f);
                staminaSystem.RestoreStamina(0f);
            }
        }

        #endregion

        #region Static Helpers

        private static GameObject ResolvePlayer(GameObject root)
        {
            if (root != null)
            {
                return root;
            }

            GameObject taggedPlayer = null;
            try
            {
                taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                // Tag may not exist yet; ignore
            }

            if (taggedPlayer != null)
            {
                return taggedPlayer;
            }

            var controller = UnityEngine.Object.FindObjectOfType<PlayerController>();
            return controller != null ? controller.gameObject : null;
        }

        private static HealthSystem FindHealthSystem(GameObject player)
        {
            if (player != null && player.TryGetComponent(out HealthSystem health))
            {
                return health;
            }

            return UnityEngine.Object.FindObjectOfType<HealthSystem>();
        }

        private static StaminaSystem FindStaminaSystem(GameObject player)
        {
            if (player != null && player.TryGetComponent(out StaminaSystem stamina))
            {
                return stamina;
            }

            return UnityEngine.Object.FindObjectOfType<StaminaSystem>();
        }

        private static readonly FieldInfo HealthSystemCurrentStaminaField = typeof(HealthSystem)
            .GetField("currentStamina", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo HealthSystemMaxStaminaField = typeof(HealthSystem)
            .GetField("maxStamina", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo HealthSystemStaminaTimerField = typeof(HealthSystem)
            .GetField("timeSinceLastStaminaUse", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StaminaSystemCurrentField = typeof(StaminaSystem)
            .GetField("currentStamina", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StaminaSystemMaxField = typeof(StaminaSystem)
            .GetField("maxStamina", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StaminaSystemRegenTimerField = typeof(StaminaSystem)
            .GetField("staminaRegenTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StaminaSystemExhaustedField = typeof(StaminaSystem)
            .GetField("isExhausted", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo LegacyInventoryQuickSlotCountField = typeof(LegacyInventoryManager)
            .GetField("quickSlotsCount", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo LegacyInventoryInstanceField = typeof(LegacyInventoryManager)
            .GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);

        private static int GetLegacyQuickSlotCount(LegacyInventoryManager manager)
        {
            if (manager == null)
                return 0;

            if (LegacyInventoryQuickSlotCountField != null)
            {
                object value = LegacyInventoryQuickSlotCountField.GetValue(manager);
                if (value is int count && count > 0)
                {
                    return count;
                }
            }

            return 4;
        }

        private static LegacyInventoryManager GetLegacyInventoryManager()
        {
            var existing = LegacyInventoryInstanceField?.GetValue(null) as LegacyInventoryManager;
            if (existing != null)
            {
                return existing;
            }

            return UnityEngine.Object.FindObjectOfType<LegacyInventoryManager>();
        }

        #endregion
    }

    /// <summary>
    /// Serializable wrapper for complex ItemData objects.
    /// </summary>
    [Serializable]
    public class InventoryItemRecord
    {
        public string itemId;
        public string itemName;
        public string typeName;
        public string payloadJson;

        public static InventoryItemRecord FromItemData(ItemData item)
        {
            return new InventoryItemRecord
            {
                itemId = item.itemId,
                itemName = item.itemName,
                typeName = item.GetType().AssemblyQualifiedName,
                payloadJson = JsonUtility.ToJson(item)
            };
        }

        public ItemData ToItemData()
        {
            if (string.IsNullOrEmpty(payloadJson))
                return null;

            Type resolvedType = !string.IsNullOrEmpty(typeName) ? Type.GetType(typeName) : typeof(ItemData);
            if (resolvedType == null)
            {
                resolvedType = typeof(ItemData);
            }

            return (ItemData)JsonUtility.FromJson(payloadJson, resolvedType);
        }
    }

    /// <summary>
    /// Stores which equipment slot a specific item occupies.
    /// </summary>
    [Serializable]
    public class EquippedItemRecord
    {
        public string itemId;
        public EquipmentSlot slot;
    }

    /// <summary>
    /// Stores quick slot assignments for the modern inventory system.
    /// </summary>
    [Serializable]
    public class QuickSlotRecord
    {
        public string itemId;
        public int slotIndex;
    }

    /// <summary>
    /// Aggregated statistics for analytics and display.
    /// </summary>
    [Serializable]
    public class PlayerStatRecord
    {
        public string key;
        public float value;
    }

    /// <summary>
    /// Snapshot data for the legacy inventory system.
    /// </summary>
    [Serializable]
    public class LegacyInventorySnapshot
    {
        public List<LegacyInventoryItemRecord> items = new List<LegacyInventoryItemRecord>();
        public List<LegacyQuickSlotRecord> quickSlots = new List<LegacyQuickSlotRecord>();
        public string equippedWeaponId = string.Empty;
        public string equippedToolId = string.Empty;
        public int quickSlotsCapacity = 4;
    }

    /// <summary>
    /// Serializable record for ProtocolEMR.Systems.Item instances.
    /// </summary>
    [Serializable]
    public class LegacyInventoryItemRecord
    {
        public string id;
        public string name;
        public string description;
        public LegacyItem.ItemType itemType;
        public LegacyItem.ToolType toolType;
        public float weight;
        public int maxStack;
        public int currentStack;

        public static LegacyInventoryItemRecord FromItem(LegacyItem item)
        {
            return new LegacyInventoryItemRecord
            {
                id = item.ID,
                name = item.Name,
                description = item.Description,
                itemType = item.Type,
                toolType = item.Tool,
                weight = item.Weight,
                maxStack = item.MaxStackSize,
                currentStack = item.CurrentStack
            };
        }

        public LegacyItem ToItem()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }

            var rebuilt = new LegacyItem(id, name, description, itemType, weight, null, Mathf.Max(1, maxStack), toolType)
            {
                CurrentStack = Mathf.Max(1, currentStack)
            };

            return rebuilt;
        }
    }

    /// <summary>
    /// Quick slot assignment for the legacy inventory.
    /// </summary>
    [Serializable]
    public class LegacyQuickSlotRecord
    {
        public string itemId;
        public int slotIndex;
    }
}
