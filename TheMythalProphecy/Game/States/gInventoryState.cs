using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.UI.Gleam;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Inventory menu state using GleamUI with cosmic aesthetic.
/// Allows browsing and using items on party members.
/// </summary>
public class gInventoryState : IGameState
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

    // Left panel - Category list
    private GleamPanel _categoryPanel;
    private GleamListBox _categoryList;

    // Center-left panel - Item list
    private GleamPanel _itemsPanel;
    private GleamListBox _itemList;

    // Center-right panel - Character selection
    private GleamPanel _characterPanel;
    private GleamLabel _characterPanelTitle;
    private GleamCharacterList _characterList;

    // Right panel - Item details
    private GleamPanel _detailsPanel;
    private GleamLabel _itemNameLabel;
    private GleamLabel _itemDescriptionLabel;
    private GleamLabel _itemEffectsLabel;
    private GleamLabel _itemQuantityLabel;
    private GleamLabel _itemUsabilityLabel;

    // Bottom bar
    private GleamLabel _goldLabel;
    private GleamLabel _instructionsLabel;

    // State
    private ItemCategory _selectedCategory = ItemCategory.All;
    private int _focusedPanel; // 0=categories, 1=items, 2=characters
    private KeyboardState _previousKeyState;
    private MouseState _previousMouseState;

    // Cached filtered item IDs
    private List<string> _filteredItemIds = new();

    public gInventoryState(ContentManager content, GameStateManager stateManager)
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

        // Focus category list initially
        _categoryList.IsFocused = true;
        _focusedPanel = 0;

        // Populate lists
        RefreshCharacterList();
        RefreshItemList();

        // Select first character if available
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
        int panelWidth = Math.Clamp((int)(screenWidth * 0.80f), 1000, 1300);
        int panelHeight = Math.Clamp((int)(screenHeight * 0.75f), 600, 800);
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
        int goldDisplayHeight = 28;
        int spacing = 15;

        int contentWidth = panelWidth - margin * 2;
        int contentHeight = panelHeight - margin * 2 - titleHeight - instructionHeight - goldDisplayHeight - margin;

        // Column widths
        int categoryListWidth = 140;
        int itemListWidth = (int)(contentWidth * 0.22f);
        int characterListWidth = (int)(contentWidth * 0.22f);
        int detailsPanelWidth = contentWidth - categoryListWidth - itemListWidth - characterListWidth - (spacing * 3);

        float currentY = margin;

        // Title
        _titleLabel = new GleamLabel("Inventory", new Vector2(margin, currentY), new Vector2(panelWidth - margin * 2, titleHeight))
        {
            Alignment = TextAlignment.Center,
            Font = _theme.MenuFont
        };
        _mainPanel.AddChild(_titleLabel);
        currentY += titleHeight + margin / 2;

        // Content panel (manual layout)
        _contentPanel = new GleamPanel(new Vector2(margin, currentY), new Vector2(contentWidth, contentHeight))
        {
            DrawBackground = false,
            DrawBorder = false,
            Layout = GleamLayout.None
        };
        _mainPanel.AddChild(_contentPanel);

        // Create panels
        int xOffset = 0;

        // Category panel
        CreateCategoryPanel(categoryListWidth, contentHeight, xOffset);
        xOffset += categoryListWidth + spacing;

        // Items panel
        CreateItemsPanel(itemListWidth, contentHeight, xOffset);
        xOffset += itemListWidth + spacing;

        // Character panel
        CreateCharacterPanel(characterListWidth, contentHeight, xOffset);
        xOffset += characterListWidth + spacing;

        // Details panel
        CreateDetailsPanel(detailsPanelWidth, contentHeight, xOffset);

        currentY += contentHeight + margin / 2;

        // Gold display
        int gold = GameServices.GameData.Progress.Gold;
        _goldLabel = new GleamLabel($"Gold: {gold}", new Vector2(margin, currentY), new Vector2(200, goldDisplayHeight))
        {
            TextColor = Color.Yellow
        };
        _mainPanel.AddChild(_goldLabel);

        // Instructions
        _instructionsLabel = new GleamLabel(
            "Tab: Switch Panel | Up/Down: Navigate | Enter: Use Item | Esc: Close",
            new Vector2(margin + 200, currentY),
            new Vector2(panelWidth - margin * 2 - 200, instructionHeight))
        {
            Alignment = TextAlignment.Center,
            TextColor = _theme.TextSecondary
        };
        _mainPanel.AddChild(_instructionsLabel);
    }

    private void CreateCategoryPanel(int width, int height, int xPosition)
    {
        _categoryPanel = new GleamPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f
        };

        _categoryList = new GleamListBox(new Vector2(4, 4), new Vector2(width - 8, height - 8))
        {
            ItemHeight = 40
        };
        _categoryList.AddItem("All");
        _categoryList.AddItem("Consumables");
        _categoryList.AddItem("Key Items");
        _categoryList.SelectedIndex = 0;
        _categoryList.OnSelectionChanged += OnCategoryChanged;
        _categoryPanel.AddChild(_categoryList);

        _contentPanel.AddChild(_categoryPanel);
    }

    private void CreateItemsPanel(int width, int height, int xPosition)
    {
        _itemsPanel = new GleamPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f
        };

        _itemList = new GleamListBox(new Vector2(4, 4), new Vector2(width - 8, height - 8))
        {
            ItemHeight = 35
        };
        _itemList.OnSelectionChanged += OnItemSelectionChanged;
        _itemList.OnItemActivated += OnItemActivated;
        _itemsPanel.AddChild(_itemList);

        _contentPanel.AddChild(_itemsPanel);
    }

    private void CreateCharacterPanel(int width, int height, int xPosition)
    {
        _characterPanel = new GleamPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f,
            Layout = GleamLayout.None
        };

        int innerWidth = width - 16;
        int padding = 8;

        // Title
        _characterPanelTitle = new GleamLabel("Use On:", new Vector2(padding, padding), new Vector2(innerWidth, 24))
        {
            TextColor = _theme.GoldBright
        };
        _characterPanel.AddChild(_characterPanelTitle);

        // Character list
        int listHeight = height - padding * 2 - 32;
        _characterList = new GleamCharacterList(new Vector2(padding, padding + 30), new Vector2(innerWidth, listHeight))
        {
            ItemHeight = 55,
            MaxSlots = 4,
            ShowEmptySlots = false
        };
        _characterList.OnSelectionChanged += OnCharacterSelectionChanged;
        _characterList.OnItemActivated += OnCharacterActivated;
        _characterPanel.AddChild(_characterList);

        _contentPanel.AddChild(_characterPanel);
    }

    private void CreateDetailsPanel(int width, int height, int xPosition)
    {
        _detailsPanel = new GleamPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f,
            Layout = GleamLayout.None
        };

        int innerWidth = width - 20;
        int padding = 10;
        int labelHeight = 24;
        float currentY = padding;

        // Item name
        _itemNameLabel = new GleamLabel("Select an item", new Vector2(padding, currentY), new Vector2(innerWidth, labelHeight * 1.2f))
        {
            TextColor = _theme.GoldBright
        };
        _detailsPanel.AddChild(_itemNameLabel);
        currentY += labelHeight * 1.5f;

        // Item description
        _itemDescriptionLabel = new GleamLabel("", new Vector2(padding, currentY), new Vector2(innerWidth, height * 0.25f))
        {
            TextColor = _theme.TextSecondary
        };
        _detailsPanel.AddChild(_itemDescriptionLabel);
        currentY += height * 0.28f;

        // Item effects
        _itemEffectsLabel = new GleamLabel("", new Vector2(padding, currentY), new Vector2(innerWidth, height * 0.3f))
        {
            TextColor = _theme.TextPrimary
        };
        _detailsPanel.AddChild(_itemEffectsLabel);
        currentY += height * 0.32f;

        // Item quantity
        _itemQuantityLabel = new GleamLabel("", new Vector2(padding, currentY), new Vector2(innerWidth, labelHeight))
        {
            TextColor = _theme.TextSecondary
        };
        _detailsPanel.AddChild(_itemQuantityLabel);
        currentY += labelHeight + 8;

        // Item usability
        _itemUsabilityLabel = new GleamLabel("", new Vector2(padding, currentY), new Vector2(innerWidth, labelHeight))
        {
            TextColor = _theme.TextSecondary
        };
        _detailsPanel.AddChild(_itemUsabilityLabel);

        _contentPanel.AddChild(_detailsPanel);
    }

    private void RefreshCharacterList()
    {
        var party = GameServices.GameData.Party.ActiveParty;
        _characterList.SetCharacters(party);
    }

    private void RefreshItemList()
    {
        _itemList.ClearItems();
        _filteredItemIds.Clear();

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
                    _filteredItemIds.Add(itemId);
                }
            }
        }

        // Select first item if available
        if (_itemList.ItemCount > 0)
        {
            _itemList.SelectedIndex = 0;
        }
        else
        {
            // Clear details panel if no items
            _itemNameLabel.Text = "No items";
            _itemDescriptionLabel.Text = "";
            _itemEffectsLabel.Text = "";
            _itemQuantityLabel.Text = "";
            _itemUsabilityLabel.Text = "";
        }
    }

    private void RefreshGoldDisplay()
    {
        int gold = GameServices.GameData.Progress.Gold;
        _goldLabel.Text = $"Gold: {gold}";
    }

    private void OnCategoryChanged(GleamListBox sender, int index)
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

    private void OnItemSelectionChanged(GleamListBox sender, int index)
    {
        if (index < 0 || index >= _filteredItemIds.Count)
        {
            _itemNameLabel.Text = "Select an item";
            _itemDescriptionLabel.Text = "";
            _itemEffectsLabel.Text = "";
            _itemQuantityLabel.Text = "";
            _itemUsabilityLabel.Text = "";
            return;
        }

        string selectedItemId = _filteredItemIds[index];
        var itemDef = GameServices.GameData.ItemDatabase.Get(selectedItemId);
        if (itemDef != null)
        {
            int quantity = GameServices.GameData.Inventory.GetItemCount(selectedItemId);

            _itemNameLabel.Text = itemDef.Name;
            _itemDescriptionLabel.Text = itemDef.Description;
            _itemQuantityLabel.Text = $"Quantity: {quantity}";

            // Effects
            string effects = GetEffectDescription(itemDef);
            _itemEffectsLabel.Text = !string.IsNullOrEmpty(effects) ? $"Effects:\n{effects}" : "";

            // Usability
            if (itemDef.IsConsumable)
            {
                if (itemDef.IsUsableInMenu && itemDef.IsUsableInBattle)
                    _itemUsabilityLabel.Text = "Usable in Menu and Battle";
                else if (itemDef.IsUsableInMenu)
                    _itemUsabilityLabel.Text = "Usable in Menu";
                else if (itemDef.IsUsableInBattle)
                    _itemUsabilityLabel.Text = "Usable in Battle only";
                else
                    _itemUsabilityLabel.Text = "Not usable";
            }
            else
            {
                _itemUsabilityLabel.Text = "Key Item";
            }
        }
    }

    private void OnItemActivated(GleamListBox sender, int index)
    {
        // When item is activated, move focus to character selection
        if (index >= 0 && index < _filteredItemIds.Count)
        {
            string selectedItemId = _filteredItemIds[index];
            var itemDef = GameServices.GameData.ItemDatabase.Get(selectedItemId);

            if (itemDef != null && itemDef.IsConsumable && itemDef.IsUsableInMenu)
            {
                // Move focus to character list
                _focusedPanel = 2;
                _categoryList.IsFocused = false;
                _itemList.IsFocused = false;
                _characterList.IsFocused = true;

                // Select first character if none selected
                if (_characterList.SelectedIndex < 0 && _characterList.CharacterCount > 0)
                {
                    _characterList.SelectedIndex = 0;
                }
            }
        }
    }

    private void OnCharacterSelectionChanged(GleamCharacterList sender, int index)
    {
        // Character selection changed - could update preview here if desired
    }

    private void OnCharacterActivated(GleamCharacterList sender, int index)
    {
        // Use item on selected character
        TryUseItemOnCharacter();
    }

    private void TryUseItemOnCharacter()
    {
        if (_itemList.SelectedIndex < 0 || _itemList.SelectedIndex >= _filteredItemIds.Count)
            return;

        var selectedCharacter = _characterList.SelectedCharacter;
        if (selectedCharacter == null)
            return;

        string selectedItemId = _filteredItemIds[_itemList.SelectedIndex];
        var itemDef = GameServices.GameData.ItemDatabase.Get(selectedItemId);

        if (itemDef != null && itemDef.IsConsumable && itemDef.IsUsableInMenu)
        {
            UseItem(selectedItemId, itemDef, selectedCharacter);
        }
    }

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

            // Refresh displays
            RefreshItemList();
            RefreshCharacterList(); // Update HP/MP bars
            RefreshGoldDisplay();
        }
    }

    private string GetEffectDescription(ItemDefinition item)
    {
        var effects = new List<string>();

        if (item.HPRestore > 0)
            effects.Add($"  Restores {item.HPRestore} HP");

        if (item.MPRestore > 0)
            effects.Add($"  Restores {item.MPRestore} MP");

        if (item.HPRestorePercent > 0)
            effects.Add($"  Restores {item.HPRestorePercent * 100}% HP");

        if (item.MPRestorePercent > 0)
            effects.Add($"  Restores {item.MPRestorePercent * 100}% MP");

        if (item.RevivesCharacter)
            effects.Add("  Revives fallen ally");

        if (item.RemovesStatusEffects.Count > 0)
            effects.Add($"  Removes: {string.Join(", ", item.RemovesStatusEffects)}");

        return string.Join("\n", effects);
    }

    private void CycleFocus()
    {
        _categoryList.IsFocused = false;
        _itemList.IsFocused = false;
        _characterList.IsFocused = false;

        _focusedPanel = (_focusedPanel + 1) % 3;

        switch (_focusedPanel)
        {
            case 0:
                _categoryList.IsFocused = true;
                break;
            case 1:
                _itemList.IsFocused = true;
                break;
            case 2:
                _characterList.IsFocused = true;
                break;
        }
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

        // Refresh in case inventory or party changed
        RefreshCharacterList();
        RefreshItemList();
        RefreshGoldDisplay();
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
            case 0: // Category list
                if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                    _categoryList.SelectPrevious();
                if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                    _categoryList.SelectNext();
                break;

            case 1: // Item list
                if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                    _itemList.SelectPrevious();
                if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                    _itemList.SelectNext();
                if ((keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter)) ||
                    GameServices.Input.IsAcceptPressed())
                    OnItemActivated(_itemList, _itemList.SelectedIndex);
                break;

            case 2: // Character list
                if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                    _characterList.SelectPrevious();
                if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                    _characterList.SelectNext();
                if ((keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter)) ||
                    GameServices.Input.IsAcceptPressed())
                    TryUseItemOnCharacter();
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
