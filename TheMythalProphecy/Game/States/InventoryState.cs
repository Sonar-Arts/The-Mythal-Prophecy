using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Inventory menu state - browse and use items
/// </summary>
public class InventoryState : IGameState
{
    private readonly GameStateManager _stateManager;
    private UIWindow _window;
    private UIListBox _categoryList;
    private UIListBox _itemList;
    private UIPanel _detailsPanel;
    private UILabel _itemNameLabel;
    private UILabel _itemDescriptionLabel;
    private UILabel _itemQuantityLabel;
    private UILabel _goldLabel;
    private KeyboardState _previousKeyState;

    // Category tracking
    private ItemCategory _selectedCategory = ItemCategory.All;

    public InventoryState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Create window (800x550)
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        Vector2 windowSize = new Vector2(900, 550);
        Vector2 windowPos = new Vector2(
            (screenWidth - windowSize.X) / 2,
            (screenHeight - windowSize.Y) / 2
        );

        _window = new UIWindow(windowPos, windowSize, "Inventory")
        {
            IsModal = true,
            ShowCloseButton = false
        };

        // Create three-column layout
        // Left: Category list (150px)
        _categoryList = new UIListBox(new Vector2(10, 10), new Vector2(150, 400))
        {
            ItemHeight = 40
        };
        _categoryList.AddItem("All");
        _categoryList.AddItem("Consumables");
        _categoryList.AddItem("Key Items");
        _categoryList.SelectedIndex = 0;
        _categoryList.OnSelectionChanged += OnCategoryChanged;
        _window.ContentPanel.AddChild(_categoryList);

        // Center: Item list (300px)
        _itemList = new UIListBox(new Vector2(170, 10), new Vector2(300, 400))
        {
            ItemHeight = 35
        };
        _itemList.OnSelectionChanged += OnItemSelectionChanged;
        _itemList.OnItemActivated += OnItemActivated;
        _window.ContentPanel.AddChild(_itemList);

        // Right: Details panel (400px)
        _detailsPanel = new UIPanel(new Vector2(480, 10), new Vector2(380, 400))
        {
            BackgroundColor = new Color(30, 30, 50, 200)
        };

        _itemNameLabel = new UILabel("Select an item", new Vector2(10, 10));
        _itemNameLabel.TextColor = Color.Gold;
        _detailsPanel.AddChild(_itemNameLabel);

        _itemDescriptionLabel = new UILabel("", new Vector2(10, 50));
        _itemDescriptionLabel.Size = new Vector2(360, 250);
        _detailsPanel.AddChild(_itemDescriptionLabel);

        _itemQuantityLabel = new UILabel("", new Vector2(10, 320));
        _detailsPanel.AddChild(_itemQuantityLabel);

        _window.ContentPanel.AddChild(_detailsPanel);

        // Bottom: Gold display
        int gold = GameServices.GameData.Progress.Gold;
        _goldLabel = new UILabel($"Gold: {gold}", new Vector2(10, 420));
        _goldLabel.TextColor = Color.Yellow;
        _window.ContentPanel.AddChild(_goldLabel);

        // Add instructions
        var instructionsLabel = new UILabel("Enter: Use Item | Tab: Change Category | Esc: Close", new Vector2(170, 420));
        instructionsLabel.TextColor = Color.LightGray;
        _window.ContentPanel.AddChild(instructionsLabel);

        // Register window
        GameServices.UI.AddElement(_window);

        // Give focus to category list initially
        _categoryList.IsFocused = true;

        // Populate items for selected category
        RefreshItemList();
    }

    public void Exit()
    {
        GameServices.UI.RemoveElement(_window);
    }

    public void Pause()
    {
        // Hide window when another state is pushed on top
        if (_window != null)
            _window.Visible = false;
    }

    public void Resume()
    {
        // Show window when this state becomes active again
        if (_window != null)
            _window.Visible = true;

        // Reset keyboard state to prevent immediate re-triggering
        _previousKeyState = Keyboard.GetState();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState keyState = Keyboard.GetState();

        // Check for escape to close menu
        if ((keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            GameServices.Input.IsCancelPressed())
        {
            _stateManager.PopState();
        }

        // Tab to switch focus between category and item list
        if (keyState.IsKeyDown(Keys.Tab) && !_previousKeyState.IsKeyDown(Keys.Tab))
        {
            if (_categoryList.IsFocused)
            {
                _categoryList.IsFocused = false;
                _itemList.IsFocused = true;
            }
            else
            {
                _itemList.IsFocused = false;
                _categoryList.IsFocused = true;
            }
        }

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // UI Manager handles drawing
    }

    /// <summary>
    /// Handle category selection change
    /// </summary>
    private void OnCategoryChanged(UIListBox sender, int index)
    {
        _selectedCategory = index switch
        {
            0 => ItemCategory.All,
            1 => ItemCategory.Consumables,
            2 => ItemCategory.KeyItems,
            _ => ItemCategory.All
        };

        RefreshItemList();
    }

    /// <summary>
    /// Refresh the item list based on selected category
    /// </summary>
    private void RefreshItemList()
    {
        _itemList.ClearItems();

        var inventory = GameServices.GameData.Inventory;

        foreach (var itemId in inventory.GetItemIds())
        {
            var def = GameServices.GameData.ItemDatabase.Get(itemId);
            if (def != null)
            {
                // Filter by category
                if (_selectedCategory == ItemCategory.All || def.Category == _selectedCategory)
                {
                    int quantity = inventory.GetItemCount(itemId);
                    _itemList.AddItem($"{def.Name} x{quantity}");
                }
            }
        }

        // Select first item if available
        if (_itemList.Items.Count > 0)
        {
            _itemList.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Handle item selection change to update details panel
    /// </summary>
    private void OnItemSelectionChanged(UIListBox sender, int index)
    {
        if (index < 0 || index >= _itemList.Items.Count)
        {
            _itemNameLabel.Text = "Select an item";
            _itemDescriptionLabel.Text = "";
            _itemQuantityLabel.Text = "";
            return;
        }

        // Get the selected item ID from the inventory
        var itemIds = GameServices.GameData.Inventory.GetItemIds().ToList();
        var filteredIds = itemIds.Where(id =>
        {
            var def = GameServices.GameData.ItemDatabase.Get(id);
            if (def != null)
            {
                return _selectedCategory == ItemCategory.All || def.Category == _selectedCategory;
            }
            return false;
        }).ToList();

        if (index >= filteredIds.Count) return;

        string selectedItemId = filteredIds[index];

        var itemDef = GameServices.GameData.ItemDatabase.Get(selectedItemId);
        if (itemDef != null)
        {
            int quantity = GameServices.GameData.Inventory.GetItemCount(selectedItemId);

            _itemNameLabel.Text = itemDef.Name;
            _itemDescriptionLabel.Text = itemDef.Description;
            _itemQuantityLabel.Text = $"Quantity: {quantity}";

            // Add effect description
            string effects = GetEffectDescription(itemDef);
            if (!string.IsNullOrEmpty(effects))
            {
                _itemDescriptionLabel.Text += "\n\n" + effects;
            }
        }
    }

    /// <summary>
    /// Handle item activation (use item)
    /// </summary>
    private void OnItemActivated(UIListBox sender, int index)
    {
        var itemIds = GameServices.GameData.Inventory.GetItemIds().ToList();
        var filteredIds = itemIds.Where(id =>
        {
            var def = GameServices.GameData.ItemDatabase.Get(id);
            if (def != null)
            {
                return _selectedCategory == ItemCategory.All || def.Category == _selectedCategory;
            }
            return false;
        }).ToList();

        if (index >= filteredIds.Count) return;

        string selectedItemId = filteredIds[index];

        var itemDef = GameServices.GameData.ItemDatabase.Get(selectedItemId);
        if (itemDef != null)
        {
            if (itemDef.IsConsumable && itemDef.IsUsableInMenu)
            {
                // For now, use on the first party member
                // TODO: Add character selection menu
                var party = GameServices.GameData.Party.ActiveParty;
                if (party.Count > 0)
                {
                    UseItem(selectedItemId, itemDef, party[0]);
                }
            }
        }
    }

    /// <summary>
    /// Use an item on a target character
    /// </summary>
    private void UseItem(string itemId, ItemDefinition itemDef, Entity target)
    {
        var stats = target.GetComponent<StatsComponent>();
        if (stats == null) return;

        bool itemUsed = false;

        // Apply HP restoration
        if (itemDef.HPRestore > 0)
        {
            stats.Heal(itemDef.HPRestore);
            itemUsed = true;
        }

        // Apply MP restoration
        if (itemDef.MPRestore > 0)
        {
            stats.RestoreMP(itemDef.MPRestore);
            itemUsed = true;
        }

        // Apply percentage-based restoration
        if (itemDef.HPRestorePercent > 0)
        {
            int healAmount = (int)(stats.MaxHP * itemDef.HPRestorePercent);
            stats.Heal(healAmount);
            itemUsed = true;
        }

        if (itemDef.MPRestorePercent > 0)
        {
            int restoreAmount = (int)(stats.MaxMP * itemDef.MPRestorePercent);
            stats.RestoreMP(restoreAmount);
            itemUsed = true;
        }

        // Handle revival
        if (itemDef.RevivesCharacter && stats.IsDead)
        {
            stats.Heal(stats.MaxHP / 4); // Revive with 25% HP
            itemUsed = true;
        }

        // Remove item from inventory if used
        if (itemUsed)
        {
            GameServices.GameData.Inventory.RemoveItem(itemId, 1);

            // Refresh the item list
            RefreshItemList();

            // TODO: Publish ItemUsedEvent
            // GameServices.Events.Publish(new ItemUsedEvent(itemId, target));
        }
    }

    /// <summary>
    /// Get effect description for an item
    /// </summary>
    private string GetEffectDescription(ItemDefinition item)
    {
        var effects = new List<string>();

        if (item.HPRestore > 0)
            effects.Add($"Restores {item.HPRestore} HP");

        if (item.MPRestore > 0)
            effects.Add($"Restores {item.MPRestore} MP");

        if (item.HPRestorePercent > 0)
            effects.Add($"Restores {item.HPRestorePercent * 100}% HP");

        if (item.MPRestorePercent > 0)
            effects.Add($"Restores {item.MPRestorePercent * 100}% MP");

        if (item.RevivesCharacter)
            effects.Add("Revives fallen ally");

        if (item.RemovesStatusEffects.Count > 0)
            effects.Add($"Removes: {string.Join(", ", item.RemovesStatusEffects)}");

        return string.Join("\n", effects);
    }
}
