using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Systems.Animation;

namespace TheMythalProphecy.Game.Data.Mock;

/// <summary>
/// Central initializer for all mock data
/// Call this once during game startup to populate databases and GameData
/// </summary>
public static class MockDataInitializer
{
    /// <summary>
    /// Initialize all mock data (databases + game data)
    /// </summary>
    /// <param name="clearExisting">If true, clears existing data first</param>
    public static void Initialize(bool clearExisting = true)
    {
        if (clearExisting)
        {
            ClearAllData();
        }

        // Step 1: Populate definition databases
        InitializeDefinitionDatabases();

        // Step 2: Initialize animation library
        AnimationLibrary.Initialize(GameServices.Animations);

        // Step 3: Populate game data (party, inventory, progress)
        InitializeGameData();
    }

    /// <summary>
    /// Clear all game data and databases
    /// </summary>
    private static void ClearAllData()
    {
        GameServices.GameData.Reset();
        GameServices.GameData.ItemDatabase.Clear();
        GameServices.GameData.EquipmentDatabase.Clear();
        GameServices.GameData.CharacterDatabase.Clear();
    }

    /// <summary>
    /// Populate all definition databases
    /// </summary>
    private static void InitializeDefinitionDatabases()
    {
        MockCharacterData.PopulateDatabase(GameServices.GameData.CharacterDatabase);
        MockItemData.PopulateDatabase(GameServices.GameData.ItemDatabase);
        MockEquipmentData.PopulateDatabase(GameServices.GameData.EquipmentDatabase);
    }

    /// <summary>
    /// Populate runtime game data
    /// </summary>
    private static void InitializeGameData()
    {
        CreatePartyMembers();
        PopulateInventory();
        InitializeProgress();
    }

    /// <summary>
    /// Create and add party members
    /// </summary>
    private static void CreatePartyMembers()
    {
        var charDb = GameServices.GameData.CharacterDatabase;
        var party = GameServices.GameData.Party;

        // Create 5 playable characters at various levels
        var aria = MockCharacterFactory.CreateCharacter(charDb.Get("aria"), 7);
        var kael = MockCharacterFactory.CreateCharacter(charDb.Get("kael"), 6);
        var lyra = MockCharacterFactory.CreateCharacter(charDb.Get("lyra"), 5);
        var zephyr = MockCharacterFactory.CreateCharacter(charDb.Get("zephyr"), 8);
        var finn = MockCharacterFactory.CreateCharacter(charDb.Get("finn"), 4);

        // Add test status effects to some characters
        MockCharacterFactory.ApplyTestStatusEffect(aria, StatusEffectType.Haste);
        MockCharacterFactory.ApplyTestStatusEffect(lyra, StatusEffectType.Regen);

        // Add to party (first 4 active, 5th goes to reserves)
        party.AddToParty(aria);
        party.AddToParty(kael);
        party.AddToParty(lyra);
        party.AddToParty(zephyr);
        party.AddToParty(finn); // Goes to reserves
    }

    /// <summary>
    /// Populate inventory with test items and equipment
    /// </summary>
    private static void PopulateInventory()
    {
        var inventory = GameServices.GameData.Inventory;

        // Consumable items
        inventory.AddItem("potion", 15);
        inventory.AddItem("hi_potion", 8);
        inventory.AddItem("ether", 5);
        inventory.AddItem("hi_ether", 3);
        inventory.AddItem("elixir", 2);
        inventory.AddItem("phoenix_down", 4);
        inventory.AddItem("antidote", 6);
        inventory.AddItem("eye_drops", 3);

        // Equipment in inventory (not equipped)
        inventory.AddItem("iron_sword", 2);
        inventory.AddItem("steel_sword", 1);
        inventory.AddItem("mystic_staff", 1);
        inventory.AddItem("hunters_bow", 1);

        inventory.AddItem("leather_armor", 1);
        inventory.AddItem("iron_armor", 2);
        inventory.AddItem("mystic_robe", 1);

        inventory.AddItem("power_ring", 2);
        inventory.AddItem("magic_ring", 1);
        inventory.AddItem("speed_boots", 1);
        inventory.AddItem("guardian_amulet", 1);
    }

    /// <summary>
    /// Initialize player progress data
    /// </summary>
    private static void InitializeProgress()
    {
        var progress = GameServices.GameData.Progress;

        progress.Gold = 2500;
        progress.CurrentLocation = "Mythal Village";
        progress.PlayTimeSeconds = 3600; // 1 hour of "playtime"

        // Set some story flags
        progress.SetFlag("tutorial_complete", true);
        progress.SetFlag("first_battle_won", true);
        progress.SetFlag("met_merchant", true);

        // Unlock some locations
        progress.UnlockedLocations.Add("mythal_village");
        progress.UnlockedLocations.Add("forest_path");
        progress.UnlockedLocations.Add("ancient_ruins");
    }
}
