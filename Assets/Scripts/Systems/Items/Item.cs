using UnityEngine;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Represents an item in the game world or inventory.
    /// Contains data about the item's properties, type, and icon.
    /// </summary>
    public class Item
    {
        public enum ItemType
        {
            Weapon,
            Tool,
            Consumable,
            Document,
            Equipment,
            Misc
        }

        public enum ToolType
        {
            None,
            Lockpick,
            HackingDevice,
            Scanner,
            EMPDevice,
            Flashlight,
            GrapplingHook
        }

        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ItemType Type { get; private set; }
        public ToolType Tool { get; private set; }
        public float Weight { get; private set; }
        public Sprite Icon { get; private set; }
        public int MaxStackSize { get; private set; }
        public int CurrentStack { get; set; }
        public bool IsStackable => MaxStackSize > 1;

        public Item(string id, string name, string description, ItemType type, float weight, Sprite icon = null, int maxStack = 1, ToolType toolType = ToolType.None)
        {
            ID = id;
            Name = name;
            Description = description;
            Type = type;
            Weight = weight;
            Icon = icon;
            MaxStackSize = maxStack;
            Tool = toolType;
            CurrentStack = 1;
        }

        /// <summary>
        /// Creates a copy of this item with a specific stack size.
        /// </summary>
        public Item Clone(int stackSize = 1)
        {
            var clone = new Item(ID, Name, Description, Type, Weight, Icon, MaxStackSize, Tool);
            clone.CurrentStack = Mathf.Min(stackSize, MaxStackSize);
            return clone;
        }

        /// <summary>
        /// Checks if this item can be equipped as a weapon.
        /// </summary>
        public bool IsWeapon => Type == ItemType.Weapon;

        /// <summary>
        /// Checks if this item is a tool.
        /// </summary>
        public bool IsTool => Type == ItemType.Tool;

        /// <summary>
        /// Checks if this item is a consumable.
        /// </summary>
        public bool IsConsumable => Type == ItemType.Consumable;

        /// <summary>
        /// Checks if this item is a document.
        /// </summary>
        public bool IsDocument => Type == ItemType.Document;
    }
}
