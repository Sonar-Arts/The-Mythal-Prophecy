using System.Collections.Generic;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Data.Definitions.Databases;

namespace TheMythalProphecy.Game.Data.Mock;

/// <summary>
/// Defines mock consumable items for testing
/// </summary>
public static class MockItemData
{
    public static void PopulateDatabase(ItemDatabase database)
    {
        // HP Restoration
        database.Register(new ItemDefinition
        {
            Id = "potion",
            Name = "Potion",
            Description = "A basic healing potion that restores a small amount of HP.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            HPRestore = 50,
            BuyPrice = 50,
            SellPrice = 25,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        database.Register(new ItemDefinition
        {
            Id = "hi_potion",
            Name = "Hi-Potion",
            Description = "A powerful healing potion that restores a large amount of HP.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            HPRestore = 150,
            BuyPrice = 200,
            SellPrice = 100,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        // MP Restoration
        database.Register(new ItemDefinition
        {
            Id = "ether",
            Name = "Ether",
            Description = "Restores magical power, replenishing MP.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            MPRestore = 30,
            BuyPrice = 150,
            SellPrice = 75,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        database.Register(new ItemDefinition
        {
            Id = "hi_ether",
            Name = "Hi-Ether",
            Description = "Restores a large amount of magical power.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            MPRestore = 80,
            BuyPrice = 400,
            SellPrice = 200,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        // Full Restoration
        database.Register(new ItemDefinition
        {
            Id = "elixir",
            Name = "Elixir",
            Description = "A rare and powerful item that fully restores HP and MP.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            HPRestorePercent = 1.0f,
            MPRestorePercent = 1.0f,
            BuyPrice = 1000,
            SellPrice = 500,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        // Revival
        database.Register(new ItemDefinition
        {
            Id = "phoenix_down",
            Name = "Phoenix Down",
            Description = "A mystical feather that revives a fallen ally.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            RevivesCharacter = true,
            BuyPrice = 500,
            SellPrice = 250,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        // Status Cure Items
        database.Register(new ItemDefinition
        {
            Id = "antidote",
            Name = "Antidote",
            Description = "Cures poison status.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            RemovesStatusEffects = new List<string> { "Poison" },
            BuyPrice = 80,
            SellPrice = 40,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });

        database.Register(new ItemDefinition
        {
            Id = "eye_drops",
            Name = "Eye Drops",
            Description = "Cures blindness status.",
            Type = ItemType.Consumable,
            Category = ItemCategory.Consumables,
            RemovesStatusEffects = new List<string> { "Blind" },
            BuyPrice = 100,
            SellPrice = 50,
            IsConsumable = true,
            IsUsableInBattle = true,
            IsUsableInMenu = true,
            ItemTargetType = TargetType.Ally,
            IsMultiTarget = false
        });
    }
}
