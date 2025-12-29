using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.UI.Gleam;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Equipment menu state using GleamUI with cosmic aesthetic.
/// Allows equipping/unequipping weapons, armor, and accessories.
/// </summary>
public class gEquipmentState : IGameState
{
    private readonly ContentManager _content;
    private readonly GameStateManager _stateManager;

    private GleamRenderer _renderer;
    private GleamTheme _theme;

    // Shader effects
    private Effect _nebulaEffect;
    private Texture2D _pixelTexture;
    private float _elapsedTime;

    // UI Elements - Main structure
    private GleamPanel _mainPanel;
    private GleamLabel _titleLabel;
    private GleamPanel _contentPanel;

    // Left panel - Character list
    private GleamPanel _characterListPanel;
    private GleamCharacterList _characterList;

    // Middle panel - Equipped items
    private GleamPanel _equippedPanel;
    private GleamLabel _equippedTitle;
    private List<GleamEquipmentSlot> _equipmentSlots;

    // Right panel - Available equipment and stats
    private GleamPanel _rightPanel;
    private GleamListBox _availableEquipmentList;
    private GleamPanel _statsPanel;
    private GleamLabel _statsComparisonLabel;

    // Instructions
    private GleamLabel _instructionsLabel;

    // State
    private Entity _selectedCharacter;
    private EquipmentSlot _selectedSlot = EquipmentSlot.Weapon;
    private int _focusedPanel; // 0=characters, 1=slots, 2=available equipment
    private KeyboardState _previousKeyState;
    private MouseState _previousMouseState;

    // Cached equipment IDs for the current filter
    private List<string> _filteredEquipmentIds = new();

    public gEquipmentState(ContentManager content, GameStateManager stateManager)
    {
        _content = content;
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Initialize GleamUI
        _theme = new GleamTheme();
        var defaultFont = _content.Load<SpriteFont>("Fonts/Default");
        SpriteFont menuFont;
        try
        {
            menuFont = _content.Load<SpriteFont>("Fonts/MenuTitle");
        }
        catch
        {
            menuFont = defaultFont;
        }
        _theme.Initialize(defaultFont, menuFont);

        _renderer = new GleamRenderer();
        _renderer.Initialize(GameServices.GraphicsDevice, _content, _theme);

        // Load shaders for background
        try
        {
            _nebulaEffect = _content.Load<Effect>("Effects/Nebula");
        }
        catch
        {
            // Continue without shaders
        }

        // Create pixel texture for shader quad
        _pixelTexture = new Texture2D(GameServices.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        CreateUI();

        // Focus character list
        _characterList.IsFocused = true;
        _focusedPanel = 0;

        // Populate and select first character
        RefreshCharacterList();
        if (_characterList.SelectedIndex < 0 && GameServices.GameData.Party.ActivePartyCount > 0)
        {
            _characterList.SelectedIndex = 0;
        }

        _previousKeyState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
    }

    private void CreateUI()
    {
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Responsive panel dimensions
        int panelWidth = Math.Clamp((int)(screenWidth * 0.75f), 900, 1200);
        int panelHeight = Math.Clamp((int)(screenHeight * 0.75f), 550, 750);
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;

        // Main container
        _mainPanel = new GleamPanel(new Vector2(panelX, panelY), new Vector2(panelWidth, panelHeight))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.95f
        };

        // Layout constants
        int margin = 20;
        int titleHeight = 45;
        int instructionHeight = 28;
        int characterListWidth = 180;
        int equippedPanelWidth = 220;
        int contentHeight = panelHeight - margin * 2 - titleHeight - instructionHeight - margin;

        float currentY = margin;

        // Title
        _titleLabel = new GleamLabel("Equipment", new Vector2(margin, currentY), new Vector2(panelWidth - margin * 2, titleHeight))
        {
            Alignment = TextAlignment.Center,
            Font = _theme.MenuFont
        };
        _mainPanel.AddChild(_titleLabel);
        currentY += titleHeight + margin / 2;

        // Content panel (manual layout)
        _contentPanel = new GleamPanel(new Vector2(margin, currentY), new Vector2(panelWidth - margin * 2, contentHeight))
        {
            DrawBackground = false,
            DrawBorder = false,
            Layout = GleamLayout.None
        };
        _mainPanel.AddChild(_contentPanel);

        // Left panel - Character list
        CreateCharacterListPanel(characterListWidth, contentHeight);

        // Middle panel - Equipped items
        int equippedX = characterListWidth + margin;
        CreateEquippedPanel(equippedPanelWidth, contentHeight, equippedX);

        // Right panel - Available equipment and stats
        int rightX = equippedX + equippedPanelWidth + margin;
        int rightWidth = panelWidth - margin * 2 - rightX;
        CreateRightPanel(rightWidth, contentHeight, rightX);

        currentY += contentHeight + margin / 2;

        // Instructions
        _instructionsLabel = new GleamLabel(
            "Tab: Switch Focus | Up/Down: Navigate | Enter: Equip/Unequip | Esc: Close",
            new Vector2(margin, currentY),
            new Vector2(panelWidth - margin * 2, instructionHeight))
        {
            Alignment = TextAlignment.Center,
            TextColor = _theme.TextSecondary
        };
        _mainPanel.AddChild(_instructionsLabel);
    }

    private void CreateCharacterListPanel(int width, int height)
    {
        _characterListPanel = new GleamPanel(Vector2.Zero, new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f
        };

        _characterList = new GleamCharacterList(new Vector2(4, 4), new Vector2(width - 8, height - 8))
        {
            ItemHeight = 50,
            MaxSlots = 4,
            ShowEmptySlots = false
        };
        _characterList.OnSelectionChanged += OnCharacterSelectionChanged;
        _characterListPanel.AddChild(_characterList);

        _contentPanel.AddChild(_characterListPanel);
    }

    private void CreateEquippedPanel(int width, int height, int xPosition)
    {
        _equippedPanel = new GleamPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f,
            Layout = GleamLayout.Vertical,
            Spacing = 8
        };

        int innerWidth = width - 24;
        int padding = 12;

        // Title
        _equippedTitle = new GleamLabel("Equipped", new Vector2(padding, padding), new Vector2(innerWidth, 28))
        {
            TextColor = _theme.GoldBright
        };
        _equippedPanel.AddChild(_equippedTitle);

        // Equipment slots
        _equipmentSlots = new List<GleamEquipmentSlot>();
        int slotY = padding + 36;
        int slotHeight = 45;

        foreach (EquipmentSlot slot in new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Accessory })
        {
            var equipSlot = new GleamEquipmentSlot(new Vector2(padding, slotY), new Vector2(innerWidth, slotHeight))
            {
                SlotType = slot.ToString()
            };
            _equipmentSlots.Add(equipSlot);
            _equippedPanel.AddChild(equipSlot);
            slotY += slotHeight + 8;
        }

        _contentPanel.AddChild(_equippedPanel);
    }

    private void CreateRightPanel(int width, int height, int xPosition)
    {
        _rightPanel = new GleamPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = false,
            DrawBorder = false,
            Layout = GleamLayout.None
        };

        int innerWidth = width;
        int listHeight = (int)(height * 0.65f);
        int statsHeight = height - listHeight - 10;

        // Available equipment list
        _availableEquipmentList = new GleamListBox(Vector2.Zero, new Vector2(innerWidth, listHeight))
        {
            ItemHeight = 35
        };
        _availableEquipmentList.OnSelectionChanged += OnEquipmentSelectionChanged;
        _availableEquipmentList.OnItemActivated += OnEquipmentActivated;
        _rightPanel.AddChild(_availableEquipmentList);

        // Stats comparison panel
        _statsPanel = new GleamPanel(new Vector2(0, listHeight + 10), new Vector2(innerWidth, statsHeight))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f
        };

        _statsComparisonLabel = new GleamLabel("Select equipment to compare", new Vector2(10, 10), new Vector2(innerWidth - 20, statsHeight - 20))
        {
            TextColor = _theme.TextSecondary
        };
        _statsPanel.AddChild(_statsComparisonLabel);
        _rightPanel.AddChild(_statsPanel);

        _contentPanel.AddChild(_rightPanel);
    }

    private void RefreshCharacterList()
    {
        var party = GameServices.GameData.Party.ActiveParty;
        _characterList.SetCharacters(party);
    }

    private void OnCharacterSelectionChanged(GleamCharacterList sender, int index)
    {
        var party = GameServices.GameData.Party.ActiveParty;
        if (index >= 0 && index < party.Count)
        {
            _selectedCharacter = party[index];
            RefreshEquippedDisplay();
            RefreshAvailableEquipment();
        }
        else
        {
            _selectedCharacter = null;
        }
    }

    private void RefreshEquippedDisplay()
    {
        if (_selectedCharacter == null) return;

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        // Update each equipment slot
        for (int i = 0; i < _equipmentSlots.Count; i++)
        {
            var slot = _equipmentSlots[i];
            EquipmentSlot slotType = i switch
            {
                0 => EquipmentSlot.Weapon,
                1 => EquipmentSlot.Armor,
                2 => EquipmentSlot.Accessory,
                _ => EquipmentSlot.Weapon
            };

            string equipmentId = stats.GetEquippedItem(slotType);
            var equipment = equipmentId != null ? GameServices.GameData.EquipmentDatabase.Get(equipmentId) : null;

            slot.SetItem(slotType.ToString(), equipment?.Name, null);
        }

        UpdateSlotHighlight();
    }

    private void RefreshAvailableEquipment()
    {
        _availableEquipmentList.ClearItems();
        _filteredEquipmentIds.Clear();

        var inventory = GameServices.GameData.Inventory;

        // Add "None" option to unequip
        _availableEquipmentList.AddItem("--- None ---");

        foreach (var itemId in inventory.GetItemIds())
        {
            var equipment = GameServices.GameData.EquipmentDatabase.Get(itemId);
            if (equipment != null && equipment.Slot == _selectedSlot)
            {
                _availableEquipmentList.AddItem(equipment.Name);
                _filteredEquipmentIds.Add(itemId);
            }
        }

        if (_availableEquipmentList.ItemCount > 0)
        {
            _availableEquipmentList.SelectedIndex = 0;
        }
    }

    private void OnEquipmentSelectionChanged(GleamListBox sender, int index)
    {
        if (_selectedCharacter == null) return;

        if (index <= 0) // "None" selected
        {
            _statsComparisonLabel.Text = "Select equipment to compare";
            _statsComparisonLabel.TextColor = _theme.TextSecondary;
            return;
        }

        int equipmentIndex = index - 1; // Adjust for "None" option
        if (equipmentIndex < 0 || equipmentIndex >= _filteredEquipmentIds.Count) return;

        string selectedEquipmentId = _filteredEquipmentIds[equipmentIndex];
        var newEquipment = GameServices.GameData.EquipmentDatabase.Get(selectedEquipmentId);
        if (newEquipment == null) return;

        // Calculate stat changes
        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        var comparison = CompareEquipment(stats, _selectedSlot, newEquipment);

        _statsComparisonLabel.Text = comparison;
        _statsComparisonLabel.TextColor = _theme.TextPrimary;
    }

    private void OnEquipmentActivated(GleamListBox sender, int index)
    {
        if (_selectedCharacter == null) return;

        if (index == 0) // "None" selected
        {
            UnequipSlot(_selectedSlot);
            return;
        }

        int equipmentIndex = index - 1;
        if (equipmentIndex < 0 || equipmentIndex >= _filteredEquipmentIds.Count) return;

        string selectedEquipmentId = _filteredEquipmentIds[equipmentIndex];
        EquipItem(selectedEquipmentId);
    }

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
    }

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
            string arrow = difference > 0 ? "+" : difference < 0 ? "" : "=";

            comparison.Add($"{statName}: {newBonus.Value} ({arrow}{difference})");
        }

        return string.Join("\n", comparison);
    }

    private void UpdateSlotHighlight()
    {
        for (int i = 0; i < _equipmentSlots.Count; i++)
        {
            EquipmentSlot slotType = i switch
            {
                0 => EquipmentSlot.Weapon,
                1 => EquipmentSlot.Armor,
                2 => EquipmentSlot.Accessory,
                _ => EquipmentSlot.Weapon
            };

            _equipmentSlots[i].IsHighlighted = _focusedPanel == 1 && slotType == _selectedSlot;
        }
    }

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
                // Equipped panel - manual keyboard handling
                break;
            case 2:
                _availableEquipmentList.IsFocused = true;
                break;
        }

        UpdateSlotHighlight();
    }

    private void NavigateSlot(int direction)
    {
        if (direction > 0)
        {
            _selectedSlot = _selectedSlot switch
            {
                EquipmentSlot.Weapon => EquipmentSlot.Armor,
                EquipmentSlot.Armor => EquipmentSlot.Accessory,
                _ => _selectedSlot
            };
        }
        else
        {
            _selectedSlot = _selectedSlot switch
            {
                EquipmentSlot.Accessory => EquipmentSlot.Armor,
                EquipmentSlot.Armor => EquipmentSlot.Weapon,
                _ => _selectedSlot
            };
        }

        RefreshAvailableEquipment();
        UpdateSlotHighlight();
    }

    public void Exit()
    {
        _pixelTexture?.Dispose();
    }

    public void Pause()
    {
        _mainPanel.Visible = false;
    }

    public void Resume()
    {
        _mainPanel.Visible = true;
        _previousKeyState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();

        // Refresh in case party or inventory changed
        RefreshCharacterList();
        if (_selectedCharacter != null)
        {
            RefreshEquippedDisplay();
            RefreshAvailableEquipment();
        }
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        var keyState = Keyboard.GetState();

        // Escape to close
        if ((keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            GameServices.Input.IsCancelPressed())
        {
            _stateManager.PopState();
            _previousKeyState = keyState;
            return;
        }

        // Tab to switch focus
        if (keyState.IsKeyDown(Keys.Tab) && !_previousKeyState.IsKeyDown(Keys.Tab))
        {
            CycleFocus();
        }

        // Handle navigation based on focused panel
        switch (_focusedPanel)
        {
            case 0: // Character list
                if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                    _characterList.SelectPrevious();
                if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                    _characterList.SelectNext();
                break;

            case 1: // Equipped slots
                if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                    NavigateSlot(-1);
                if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                    NavigateSlot(1);
                if ((keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter)) ||
                    GameServices.Input.IsAcceptPressed())
                    UnequipSlot(_selectedSlot);
                break;

            case 2: // Available equipment
                if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                    _availableEquipmentList.SelectPrevious();
                if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                    _availableEquipmentList.SelectNext();
                if ((keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter)) ||
                    GameServices.Input.IsAcceptPressed())
                    OnEquipmentActivated(_availableEquipmentList, _availableEquipmentList.SelectedIndex);
                break;
        }

        _previousKeyState = keyState;

        // Handle mouse input
        var mouseState = Mouse.GetState();
        Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
        bool mouseDown = mouseState.LeftButton == ButtonState.Pressed;
        bool mouseClicked = mouseDown && _previousMouseState.LeftButton == ButtonState.Released;
        _previousMouseState = mouseState;

        // Update UI
        _mainPanel.Update(gameTime, _renderer);
        _mainPanel.HandleInput(mousePos, mouseDown, mouseClicked);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var graphicsDevice = GameServices.GraphicsDevice;
        int screenWidth = graphicsDevice.Viewport.Width;
        int screenHeight = graphicsDevice.Viewport.Height;
        var screenRect = new Rectangle(0, 0, screenWidth, screenHeight);

        // End current batch to draw our layers
        spriteBatch.End();

        // Clear to black
        graphicsDevice.Clear(Color.Black);

        // Layer 1: Nebula background
        if (_nebulaEffect != null)
        {
            _nebulaEffect.Parameters["Time"]?.SetValue(_elapsedTime);
            _nebulaEffect.Parameters["Intensity"]?.SetValue(0.6f);

            spriteBatch.Begin(blendState: BlendState.AlphaBlend, effect: _nebulaEffect);
            spriteBatch.Draw(_pixelTexture, screenRect, Color.White);
            spriteBatch.End();
        }
        else
        {
            // Fallback: solid dark background
            spriteBatch.Begin();
            spriteBatch.Draw(_pixelTexture, screenRect, new Color(10, 5, 20));
            spriteBatch.End();
        }

        // Draw UI elements
        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        _mainPanel.Draw(spriteBatch, _renderer);
        spriteBatch.End();

        // Resume normal batch for other rendering
        spriteBatch.Begin();
    }
}
