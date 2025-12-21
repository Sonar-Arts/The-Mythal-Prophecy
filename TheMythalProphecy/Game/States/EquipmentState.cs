using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Equipment menu state - equip/unequip weapons, armor, and accessories
/// </summary>
public class EquipmentState : IGameState
{
    private readonly GameStateManager _stateManager;
    private UIWindow _window;
    private KeyboardState _previousKeyState;

    // Layout panels
    private UIPanel _rightPanel;

    // Character list
    private UIListBox _characterList;

    // Equipped panel and labels
    private UIPanel _equippedPanel;
    private UILabel _weaponLabel;
    private UILabel _armorLabel;
    private UILabel _accessoryLabel;

    // Available equipment
    private UIListBox _availableEquipmentList;

    // Stats comparison
    private UIPanel _statsPanel;
    private UILabel _statsComparisonLabel;

    // Layout dimensions (calculated from screen size)
    private float _characterListWidth;
    private float _equippedPanelWidth;
    private float _rightPanelWidth;

    // Currently selected character and slot
    private Entity _selectedCharacter;
    private EquipmentSlot _selectedSlot = EquipmentSlot.Weapon;
    private int _focusedPanel = 0; // 0=characters, 1=slots, 2=available equipment

    public EquipmentState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Calculate responsive window size
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        int windowWidth = Math.Clamp((int)(screenWidth * 0.75f), 900, 1200);
        int windowHeight = Math.Clamp((int)(screenHeight * 0.75f), 550, 750);

        _window = new UIWindow(Vector2.Zero, new Vector2(windowWidth, windowHeight), "Equipment")
        {
            IsModal = true,
            ShowCloseButton = false
        };
        _window.Center(screenWidth, screenHeight);

        // Disable auto-layout on content panel (we position columns manually)
        _window.ContentPanel.Layout = PanelLayout.None;

        // Calculate layout dimensions
        float contentHeight = windowHeight - _window.TitleBarHeight;
        float instructionsHeight = 40;
        float mainAreaHeight = contentHeight - instructionsHeight;
        float margin = 10;

        _characterListWidth = 180;
        _equippedPanelWidth = 220;
        _rightPanelWidth = windowWidth - _characterListWidth - _equippedPanelWidth - margin * 4;

        // Create layout sections
        CreateCharacterList(mainAreaHeight, margin);
        CreateEquippedPanel(mainAreaHeight, margin);
        CreateRightPanel(mainAreaHeight, margin);
        CreateInstructions(contentHeight - instructionsHeight, margin);

        // Register window
        GameServices.UI.AddElement(_window);

        // Give focus to character list
        _characterList.IsFocused = true;

        // Set initial character selection (must be after all UI is created)
        if (_characterList.Items.Count > 0)
        {
            _characterList.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Create character list on the left
    /// </summary>
    private void CreateCharacterList(float height, float margin)
    {
        _characterList = new UIListBox(
            new Vector2(margin, margin),
            new Vector2(_characterListWidth, height - margin * 2))
        {
            ItemHeight = 50
        };
        _characterList.OnSelectionChanged += OnCharacterSelectionChanged;

        // Populate character list
        var party = GameServices.GameData.Party.ActiveParty;
        foreach (var character in party)
        {
            var stats = character.GetComponent<StatsComponent>();
            string displayName = $"{character.Name}\nLv.{stats.Level}";
            _characterList.AddItem(displayName);
        }

        _window.ContentPanel.AddChild(_characterList);
    }

    /// <summary>
    /// Create equipped items panel in the middle
    /// </summary>
    private void CreateEquippedPanel(float height, float margin)
    {
        float xPos = _characterListWidth + margin * 2;

        _equippedPanel = new UIPanel(
            new Vector2(xPos, margin),
            new Vector2(_equippedPanelWidth, height - margin * 2))
        {
            BackgroundColor = new Color(30, 30, 50, 200),
            Layout = PanelLayout.Vertical,
            Spacing = 15
        };
        _equippedPanel.SetPadding(15);

        var equippedTitle = new UILabel("Equipped", Vector2.Zero)
        {
            TextColor = Color.Gold
        };
        _equippedPanel.AddChild(equippedTitle);

        _weaponLabel = new UILabel("[Weapon]\nNone", Vector2.Zero)
        {
            TextColor = Color.White,
            Size = new Vector2(_equippedPanelWidth - 30, 60)
        };
        _equippedPanel.AddChild(_weaponLabel);

        _armorLabel = new UILabel("[Armor]\nNone", Vector2.Zero)
        {
            TextColor = Color.White,
            Size = new Vector2(_equippedPanelWidth - 30, 60)
        };
        _equippedPanel.AddChild(_armorLabel);

        _accessoryLabel = new UILabel("[Accessory]\nNone", Vector2.Zero)
        {
            TextColor = Color.White,
            Size = new Vector2(_equippedPanelWidth - 30, 60)
        };
        _equippedPanel.AddChild(_accessoryLabel);

        _window.ContentPanel.AddChild(_equippedPanel);
    }

    /// <summary>
    /// Create right panel with available equipment and stats comparison
    /// </summary>
    private void CreateRightPanel(float height, float margin)
    {
        float xPos = _characterListWidth + _equippedPanelWidth + margin * 3;
        float panelHeight = height - margin * 2;

        _rightPanel = new UIPanel(
            new Vector2(xPos, margin),
            new Vector2(_rightPanelWidth, panelHeight))
        {
            Layout = PanelLayout.Vertical,
            Spacing = 10,
            DrawBackground = false,
            DrawBorder = false
        };
        _rightPanel.SetPadding(0);

        // Available equipment list (takes most of the space)
        float listHeight = panelHeight * 0.65f;
        _availableEquipmentList = new UIListBox(Vector2.Zero, new Vector2(_rightPanelWidth, listHeight))
        {
            ItemHeight = 35
        };
        _availableEquipmentList.OnSelectionChanged += OnEquipmentSelectionChanged;
        _availableEquipmentList.OnItemActivated += OnEquipmentActivated;
        _rightPanel.AddChild(_availableEquipmentList);

        // Stats comparison panel
        float statsHeight = panelHeight * 0.35f - 10;
        _statsPanel = new UIPanel(Vector2.Zero, new Vector2(_rightPanelWidth, statsHeight))
        {
            BackgroundColor = new Color(40, 40, 60, 200)
        };
        _statsPanel.SetPadding(10);

        _statsComparisonLabel = new UILabel("Select equipment to compare", Vector2.Zero)
        {
            TextColor = Color.LightGray,
            Size = new Vector2(_rightPanelWidth - 20, statsHeight - 20)
        };
        _statsPanel.AddChild(_statsComparisonLabel);

        _rightPanel.AddChild(_statsPanel);
        _window.ContentPanel.AddChild(_rightPanel);
    }

    /// <summary>
    /// Create instructions at the bottom
    /// </summary>
    private void CreateInstructions(float yPosition, float margin)
    {
        var instructionsLabel = new UILabel(
            "Tab: Switch Focus | Enter: Equip/Unequip | Esc: Close",
            new Vector2(margin, yPosition))
        {
            TextColor = Color.LightGray
        };
        _window.ContentPanel.AddChild(instructionsLabel);
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

        // Escape to close
        if ((keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            GameServices.Input.IsCancelPressed())
        {
            _stateManager.PopState();
        }

        // Tab to switch focus
        if (keyState.IsKeyDown(Keys.Tab) && !_previousKeyState.IsKeyDown(Keys.Tab))
        {
            CycleFocus();
        }

        // Arrow keys to navigate slots when in equipped panel
        if (_focusedPanel == 1)
        {
            if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
            {
                _selectedSlot = _selectedSlot switch
                {
                    EquipmentSlot.Weapon => EquipmentSlot.Armor,
                    EquipmentSlot.Armor => EquipmentSlot.Accessory,
                    _ => _selectedSlot
                };
                RefreshAvailableEquipment();
                UpdateSlotHighlight();
            }
            else if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
            {
                _selectedSlot = _selectedSlot switch
                {
                    EquipmentSlot.Accessory => EquipmentSlot.Armor,
                    EquipmentSlot.Armor => EquipmentSlot.Weapon,
                    _ => _selectedSlot
                };
                RefreshAvailableEquipment();
                UpdateSlotHighlight();
            }
            else if ((keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter)) ||
                     GameServices.Input.IsAcceptPressed())
            {
                // Unequip current item in selected slot
                UnequipSlot(_selectedSlot);
            }
        }

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // UI Manager handles drawing
    }

    /// <summary>
    /// Cycle focus between panels
    /// </summary>
    private void CycleFocus()
    {
        _characterList.IsFocused = false;
        _availableEquipmentList.IsFocused = false;

        _focusedPanel = (_focusedPanel + 1) % 3;

        switch (_focusedPanel)
        {
            case 0:
                _characterList.IsFocused = true;
                break;
            case 1:
                // Equipped panel (manual keyboard handling)
                UpdateSlotHighlight();
                break;
            case 2:
                _availableEquipmentList.IsFocused = true;
                break;
        }
    }

    /// <summary>
    /// Update slot highlight colors
    /// </summary>
    private void UpdateSlotHighlight()
    {
        _weaponLabel.TextColor = _selectedSlot == EquipmentSlot.Weapon ? Color.Yellow : Color.White;
        _armorLabel.TextColor = _selectedSlot == EquipmentSlot.Armor ? Color.Yellow : Color.White;
        _accessoryLabel.TextColor = _selectedSlot == EquipmentSlot.Accessory ? Color.Yellow : Color.White;
    }

    /// <summary>
    /// Handle character selection change
    /// </summary>
    private void OnCharacterSelectionChanged(UIListBox sender, int index)
    {
        var party = GameServices.GameData.Party.ActiveParty;
        if (index >= 0 && index < party.Count)
        {
            _selectedCharacter = party[index];
            RefreshEquippedDisplay();
            RefreshAvailableEquipment();
        }
    }

    /// <summary>
    /// Refresh the equipped items display
    /// </summary>
    private void RefreshEquippedDisplay()
    {
        if (_selectedCharacter == null) return;

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        // Update weapon
        string weaponId = stats.GetEquippedItem(EquipmentSlot.Weapon);
        var weapon = weaponId != null ? GameServices.GameData.EquipmentDatabase.Get(weaponId) : null;
        _weaponLabel.Text = weapon != null
            ? $"[Weapon]\n{weapon.Name}"
            : "[Weapon]\nNone";

        // Update armor
        string armorId = stats.GetEquippedItem(EquipmentSlot.Armor);
        var armor = armorId != null ? GameServices.GameData.EquipmentDatabase.Get(armorId) : null;
        _armorLabel.Text = armor != null
            ? $"[Armor]\n{armor.Name}"
            : "[Armor]\nNone";

        // Update accessory
        string accessoryId = stats.GetEquippedItem(EquipmentSlot.Accessory);
        var accessory = accessoryId != null ? GameServices.GameData.EquipmentDatabase.Get(accessoryId) : null;
        _accessoryLabel.Text = accessory != null
            ? $"[Accessory]\n{accessory.Name}"
            : "[Accessory]\nNone";

        UpdateSlotHighlight();
    }

    /// <summary>
    /// Refresh available equipment list based on selected slot
    /// </summary>
    private void RefreshAvailableEquipment()
    {
        _availableEquipmentList.ClearItems();

        var inventory = GameServices.GameData.Inventory;

        // Add "None" option to unequip
        _availableEquipmentList.AddItem("--- None ---");

        foreach (var itemId in inventory.GetItemIds())
        {
            var equipment = GameServices.GameData.EquipmentDatabase.Get(itemId);
            if (equipment != null)
            {
                if (equipment.Slot == _selectedSlot)
                {
                    _availableEquipmentList.AddItem(equipment.Name);
                }
            }
        }

        if (_availableEquipmentList.Items.Count > 0)
        {
            _availableEquipmentList.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Handle equipment selection change for stat comparison
    /// </summary>
    private void OnEquipmentSelectionChanged(UIListBox sender, int index)
    {
        if (_selectedCharacter == null) return;
        if (index <= 0) // "None" selected
        {
            _statsComparisonLabel.Text = "Select equipment to compare";
            return;
        }

        // Get selected equipment
        var equipmentIds = GameServices.GameData.Inventory.GetItemIds()
            .Where(id =>
            {
                var eq = GameServices.GameData.EquipmentDatabase.Get(id);
                return eq != null && eq.Slot == _selectedSlot;
            })
            .ToList();

        int equipmentIndex = index - 1; // Adjust for "None" option
        if (equipmentIndex < 0 || equipmentIndex >= equipmentIds.Count) return;

        string selectedEquipmentId = equipmentIds[equipmentIndex];
        var newEquipment = GameServices.GameData.EquipmentDatabase.Get(selectedEquipmentId);
        if (newEquipment == null) return;

        // Calculate stat changes
        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        var comparison = CompareEquipment(stats, _selectedSlot, newEquipment);

        _statsComparisonLabel.Text = comparison;
    }

    /// <summary>
    /// Handle equipment activation (equip item)
    /// </summary>
    private void OnEquipmentActivated(UIListBox sender, int index)
    {
        if (_selectedCharacter == null) return;

        if (index == 0) // "None" selected
        {
            UnequipSlot(_selectedSlot);
            return;
        }

        // Get selected equipment
        var equipmentIds = GameServices.GameData.Inventory.GetItemIds()
            .Where(id =>
            {
                var eq = GameServices.GameData.EquipmentDatabase.Get(id);
                return eq != null && eq.Slot == _selectedSlot;
            })
            .ToList();

        int equipmentIndex = index - 1;
        if (equipmentIndex < 0 || equipmentIndex >= equipmentIds.Count) return;

        string selectedEquipmentId = equipmentIds[equipmentIndex];
        EquipItem(selectedEquipmentId);
    }

    /// <summary>
    /// Equip an item to the character
    /// </summary>
    private void EquipItem(string equipmentId)
    {
        if (_selectedCharacter == null) return;

        var equipment = GameServices.GameData.EquipmentDatabase.Get(equipmentId);
        if (equipment == null) return;

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        // Unequip current item in this slot (if any)
        string currentEquipmentId = stats.GetEquippedItem(equipment.Slot);
        if (currentEquipmentId != null)
        {
            UnequipItem(currentEquipmentId, equipment.Slot);
        }

        // Apply stat bonuses
        foreach (var bonus in equipment.StatBonuses)
        {
            stats.AddEquipmentBonus(bonus.Key, bonus.Value);
        }

        // Mark as equipped
        stats.SetEquippedItem(equipment.Slot, equipmentId);

        // Remove from inventory
        GameServices.GameData.Inventory.RemoveItem(equipmentId, 1);

        // Refresh displays
        RefreshEquippedDisplay();
        RefreshAvailableEquipment();

        // TODO: Publish EquipmentChangedEvent
    }

    /// <summary>
    /// Unequip an item from a slot
    /// </summary>
    private void UnequipSlot(EquipmentSlot slot)
    {
        if (_selectedCharacter == null) return;

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        string equipmentId = stats.GetEquippedItem(slot);
        if (equipmentId == null) return;

        UnequipItem(equipmentId, slot);

        RefreshEquippedDisplay();
        RefreshAvailableEquipment();
    }

    /// <summary>
    /// Unequip an item and return it to inventory
    /// </summary>
    private void UnequipItem(string equipmentId, EquipmentSlot slot)
    {
        var equipment = GameServices.GameData.EquipmentDatabase.Get(equipmentId);
        if (equipment == null) return;

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        // Remove stat bonuses
        foreach (var bonus in equipment.StatBonuses)
        {
            stats.RemoveEquipmentBonus(bonus.Key, bonus.Value);
        }

        // Mark as unequipped
        stats.SetEquippedItem(slot, null);

        // Add back to inventory
        GameServices.GameData.Inventory.AddItem(equipmentId, 1);
    }

    /// <summary>
    /// Compare equipment and generate stat comparison text
    /// </summary>
    private string CompareEquipment(StatsComponent stats, EquipmentSlot slot, EquipmentDefinition newEquipment)
    {
        var comparison = new List<string>();
        comparison.Add($"{newEquipment.Name}\n");

        // Get current equipment in this slot
        string currentEquipmentId = stats.GetEquippedItem(slot);
        Dictionary<StatType, int> currentBonuses = new();

        var currentEquipment = currentEquipmentId != null ? GameServices.GameData.EquipmentDatabase.Get(currentEquipmentId) : null;
        if (currentEquipment != null)
        {
            currentBonuses = currentEquipment.StatBonuses;
        }

        // Compare stats
        foreach (var newBonus in newEquipment.StatBonuses)
        {
            int currentBonus = currentBonuses.GetValueOrDefault(newBonus.Key, 0);
            int difference = newBonus.Value - currentBonus;

            string statName = newBonus.Key.ToString();
            string arrow = difference > 0 ? "+" : difference < 0 ? "-" : "=";
            Color color = difference > 0 ? Color.Green : difference < 0 ? Color.Red : Color.White;

            comparison.Add($"{statName}: {newBonus.Value} ({arrow}{System.Math.Abs(difference)})");
        }

        return string.Join("\n", comparison);
    }
}
