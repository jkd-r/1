using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Inventory
{
    [Serializable]
    public class ItemData
    {
        public string itemId;
        public string itemName;
        public string description;
        public ItemType itemType;
        public int quantity;
        public float weight;
        public bool isStackable;
        public int maxStackSize;
        public string iconPath;
        public Rarity rarity;
        public List<string> tags;

        public ItemData()
        {
            itemId = System.Guid.NewGuid().ToString();
            tags = new List<string>();
            maxStackSize = 1;
            isStackable = false;
        }
    }

    [Serializable]
    public class EquipmentData : ItemData
    {
        public float damageValue;
        public float defenseValue;
        public float durability;
        public float maxDurability;
        public EquipmentSlot equipmentSlot;
        public List<StatModifier> statModifiers;

        public EquipmentData()
        {
            statModifiers = new List<StatModifier>();
        }
    }

    [Serializable]
    public class ConsumableData : ItemData
    {
        public float healthRestore;
        public float staminaRestore;
        public float effectDuration;
        public string effectType;

        public ConsumableData()
        {
            healthRestore = 0;
            staminaRestore = 0;
            effectDuration = 0;
        }
    }

    [Serializable]
    public class WeaponData : EquipmentData
    {
        public float attackSpeed;
        public float range;
        public int ammoInMagazine;
        public int maxAmmoInMagazine;
        public WeaponType weaponType;
        public bool requiresAmmo;
    }

    [Serializable]
    public class AmmoData : ItemData
    {
        public string compatibleWeaponType;
        public float damageMultiplier;
    }

    [Serializable]
    public class StatModifier
    {
        public string statName;
        public float value;
        public StatModifierType modifierType;
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Ammo,
        Tool,
        Document,
        Miscellaneous
    }

    public enum EquipmentSlot
    {
        Head,
        Chest,
        Hands,
        Legs,
        Feet,
        Accessory
    }

    public enum WeaponType
    {
        Melee,
        Ranged,
        Energy
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum StatModifierType
    {
        Add,
        Multiply,
        Replace
    }
}
