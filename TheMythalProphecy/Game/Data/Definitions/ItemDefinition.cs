using System.Collections.Generic;

namespace TheMythalProphecy.Game.Data.Definitions;

/// <summary>
/// Defines an item template that can be loaded from JSON
/// Items include consumables, key items, and other usable objects
/// </summary>
public class ItemDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public ItemCategory Category { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public bool IsStackable { get; set; } = true;
    public int MaxStackSize { get; set; } = 99;
    public bool IsConsumable { get; set; }
    public bool IsUsableInBattle { get; set; }
    public bool IsUsableInMenu { get; set; } = true;

    // Effect properties
    public int HPRestore { get; set; }
    public int MPRestore { get; set; }
    public float HPRestorePercent { get; set; }
    public float MPRestorePercent { get; set; }
    public List<string> RemovesStatusEffects { get; set; } = new();
    public bool RevivesCharacter { get; set; }

    // Targeting properties
    public TargetType ItemTargetType { get; set; } = TargetType.Ally;
    public bool IsMultiTarget { get; set; } = false;

    // Icon and visual properties
    public string IconPath { get; set; }
}

/// <summary>
/// Type of item - determines how it can be used
/// </summary>
public enum ItemType
{
    Consumable,     // Can be used and consumed (potions, ethers)
    KeyItem,        // Story/quest items that cannot be discarded
    Material,       // Crafting materials (future use)
    Misc            // Other items
}

/// <summary>
/// Category for organizational purposes
/// </summary>
public enum ItemCategory
{
    All,
    Consumables,
    KeyItems,
    Materials,
    Miscellaneous
}

/// <summary>
/// Defines what type of targets an item can affect
/// </summary>
public enum TargetType
{
    Ally,    // Healing items, buffs (targets party members)
    Enemy,   // Damage items (targets enemies)
    Self,    // Self-only items
    Any      // Flexible items (can target anyone)
}

/// <summary>
/// Container for loading multiple item definitions from JSON
/// </summary>
public class ItemCollection
{
    public List<ItemDefinition> Items { get; set; } = new();
}
