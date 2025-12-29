using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.UI.Gleam;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Character status state using GleamUI with cosmic aesthetic.
/// Displays detailed character information including stats, equipment, and status effects.
/// </summary>
public class gCharacterStatusScreen : IGameState
{
    private readonly ContentManager _content;
    private readonly GameStateManager _stateManager;

    private GleamRenderer _renderer;
    private GleamTheme _theme;

    // Shader effects
    private Effect _nebulaEffect;
    private Effect _starfallEffect;
    private Texture2D _pixelTexture;
    private float _elapsedTime;

    // UI Elements - Main structure
    private GleamPanel _mainPanel;
    private GleamLabel _titleLabel;
    private GleamPanel _contentPanel;

    // Left panel - Character list
    private GleamPanel _characterListPanel;
    private GleamCharacterList _characterList;

    // Right panel - Character details (scrollable)
    private GleamScrollPanel _detailPanel;
    private GleamLabel _nameLabel;
    private GleamLabel _levelLabel;
    private GleamProgressBar _hpBar;
    private GleamProgressBar _mpBar;
    private GleamProgressBar _expBar;
    private GleamStatPanel _statPanel;
    private GleamLabel _equipmentTitle;
    private List<GleamEquipmentSlot> _equipmentSlots;
    private GleamLabel _statusTitle;
    private GleamLabel _statusLabel;
    private GleamLabel _instructionsLabel;

    // State
    private Entity _selectedCharacter;
    private KeyboardState _previousKeyState;
    private MouseState _previousMouseState;

    public gCharacterStatusScreen(ContentManager content, GameStateManager stateManager)
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
            _starfallEffect = _content.Load<Effect>("Effects/Starfall");
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
        int panelWidth = System.Math.Clamp((int)(screenWidth * 0.75f), 900, 1150);
        int panelHeight = System.Math.Clamp((int)(screenHeight * 0.85f), 650, 850);
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
        int characterListWidth = 220;
        int contentHeight = panelHeight - margin * 2 - titleHeight - instructionHeight - margin;

        float currentY = margin;

        // Title
        _titleLabel = new GleamLabel("Character Status", new Vector2(margin, currentY), new Vector2(panelWidth - margin * 2, titleHeight))
        {
            Alignment = TextAlignment.Center,
            Font = _theme.MenuFont
        };
        _mainPanel.AddChild(_titleLabel);
        currentY += titleHeight + margin / 2;

        // Content panel (manual layout - no auto-layout to avoid conflicts with scroll panel)
        _contentPanel = new GleamPanel(new Vector2(margin, currentY), new Vector2(panelWidth - margin * 2, contentHeight))
        {
            DrawBackground = false,
            DrawBorder = false,
            Layout = GleamLayout.None // Manual positioning
        };
        _mainPanel.AddChild(_contentPanel);

        // Left panel - Character list (positioned at left)
        CreateCharacterListPanel(characterListWidth, contentHeight);

        // Right panel - Character details (positioned after character list)
        int detailWidth = panelWidth - margin * 2 - characterListWidth - margin;
        int detailX = characterListWidth + margin;
        CreateDetailPanel(detailWidth, contentHeight, detailX);

        currentY += contentHeight + margin / 2;

        // Instructions
        _instructionsLabel = new GleamLabel(
            "Up/Down: Navigate | Esc: Close",
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
            ItemHeight = 60,
            MaxSlots = 4,
            ShowEmptySlots = false
        };
        _characterList.OnSelectionChanged += OnCharacterSelectionChanged;
        _characterListPanel.AddChild(_characterList);

        _contentPanel.AddChild(_characterListPanel);
    }

    private void CreateDetailPanel(int width, int height, int xPosition)
    {
        // Use scrollable panel for detail content
        _detailPanel = new GleamScrollPanel(new Vector2(xPosition, 0), new Vector2(width, height))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f,
            Padding = 12,
            Spacing = 6
        };

        int innerWidth = width - 40; // Account for padding and scrollbar

        // Character name header
        _nameLabel = new GleamLabel("Select a Character", Vector2.Zero, new Vector2(innerWidth, 28))
        {
            TextColor = _theme.GoldBright,
            Scale = 1.2f
        };
        _detailPanel.AddChild(_nameLabel);

        // Level
        _levelLabel = new GleamLabel("Level: --", Vector2.Zero, new Vector2(innerWidth, 22))
        {
            TextColor = _theme.TextPrimary
        };
        _detailPanel.AddChild(_levelLabel);

        // HP Bar
        _hpBar = new GleamProgressBar(Vector2.Zero, new Vector2(innerWidth, 24))
        {
            FillColor = new Color(80, 200, 80),
            FillColorDark = new Color(30, 100, 30),
            LowFillColor = new Color(220, 80, 80),
            LowFillColorDark = new Color(100, 30, 30),
            GlowColor = new Color(150, 255, 150),
            LowGlowColor = new Color(255, 150, 150),
            ShowText = true,
            TextFormat = "HP: {0} / {1}"
        };
        _detailPanel.AddChild(_hpBar);

        // MP Bar
        _mpBar = new GleamProgressBar(Vector2.Zero, new Vector2(innerWidth, 24))
        {
            FillColor = new Color(80, 140, 220),
            FillColorDark = new Color(30, 60, 100),
            LowFillColor = new Color(160, 100, 200),
            LowFillColorDark = new Color(60, 40, 80),
            GlowColor = new Color(150, 200, 255),
            LowGlowColor = new Color(200, 150, 255),
            ShowText = true,
            TextFormat = "MP: {0} / {1}"
        };
        _detailPanel.AddChild(_mpBar);

        // EXP Bar
        _expBar = new GleamProgressBar(Vector2.Zero, new Vector2(innerWidth, 20))
        {
            FillColor = new Color(220, 200, 80),
            FillColorDark = new Color(120, 100, 30),
            GlowColor = new Color(255, 240, 150),
            ShowText = true,
            TextFormat = "EXP: {0} / {1}",
            LowThreshold = 0f // No low state for EXP
        };
        _detailPanel.AddChild(_expBar);

        // Stat panel (Cosmic Constellation layout)
        int statPanelHeight = 170;
        _statPanel = new GleamStatPanel(Vector2.Zero, new Vector2(innerWidth, statPanelHeight))
        {
            MaxStatValue = 100
        };
        _detailPanel.AddChild(_statPanel);

        // Equipment section title
        _equipmentTitle = new GleamLabel("Equipment", Vector2.Zero, new Vector2(innerWidth, 22))
        {
            TextColor = _theme.GoldBright
        };
        _detailPanel.AddChild(_equipmentTitle);

        // Equipment slots (compact single-line display)
        _equipmentSlots = new List<GleamEquipmentSlot>();
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            var equipSlot = new GleamEquipmentSlot(Vector2.Zero, new Vector2(innerWidth, 26))
            {
                SlotType = slot.ToString()
            };
            _equipmentSlots.Add(equipSlot);
            _detailPanel.AddChild(equipSlot);
        }

        // Status effects section title
        _statusTitle = new GleamLabel("Status Effects", Vector2.Zero, new Vector2(innerWidth, 22))
        {
            TextColor = new Color(200, 100, 220) // Purple-ish for status
        };
        _detailPanel.AddChild(_statusTitle);

        // Status effects label (placeholder)
        _statusLabel = new GleamLabel("None", Vector2.Zero, new Vector2(innerWidth, 22))
        {
            TextColor = _theme.TextDisabled
        };
        _detailPanel.AddChild(_statusLabel);

        // Refresh layout to calculate content height
        _detailPanel.RefreshLayout();

        _contentPanel.AddChild(_detailPanel);
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
            UpdateStatusDisplay();
        }
        else
        {
            _selectedCharacter = null;
            ClearStatusDisplay();
        }
    }

    private void UpdateStatusDisplay()
    {
        if (_selectedCharacter == null)
        {
            ClearStatusDisplay();
            return;
        }

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        // Update name and level
        _nameLabel.Text = _selectedCharacter.Name;
        _levelLabel.Text = $"Level: {stats.Level}";

        // Update HP bar
        _hpBar.MaxValue = stats.MaxHP;
        _hpBar.CurrentValue = stats.CurrentHP;

        // Update MP bar
        _mpBar.MaxValue = stats.MaxMP;
        _mpBar.CurrentValue = stats.CurrentMP;

        // Update EXP bar
        _expBar.MaxValue = stats.ExperienceToNext;
        _expBar.CurrentValue = stats.Experience;

        // Update stat panel
        _statPanel.SetStats(stats);

        // Update equipment slots
        var equippedItems = stats.GetAllEquippedItems();
        foreach (var slot in _equipmentSlots)
        {
            string slotType = slot.SlotType;
            if (System.Enum.TryParse<EquipmentSlot>(slotType, out var equipSlot))
            {
                string itemName = equippedItems.GetValueOrDefault(equipSlot, null);
                slot.SetItem(slotType, itemName, null);
            }
        }

        // Update status effects (placeholder for now)
        _statusLabel.Text = "None";
        _statusLabel.TextColor = _theme.TextDisabled;
    }

    private void ClearStatusDisplay()
    {
        _nameLabel.Text = "Select a Character";
        _levelLabel.Text = "Level: --";
        _hpBar.MaxValue = 100;
        _hpBar.CurrentValue = 0;
        _mpBar.MaxValue = 100;
        _mpBar.CurrentValue = 0;
        _expBar.MaxValue = 100;
        _expBar.CurrentValue = 0;
        _statPanel.SetStats(0, 0, 0, 0, 0, 0);

        foreach (var slot in _equipmentSlots)
        {
            slot.Clear();
        }

        _statusLabel.Text = "None";
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

        // Refresh in case party changed
        RefreshCharacterList();
        if (_selectedCharacter != null)
        {
            UpdateStatusDisplay();
        }
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        var keyState = Keyboard.GetState();

        // Escape to close
        if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
        {
            _stateManager.PopState();
            _previousKeyState = keyState;
            return;
        }

        // Arrow key navigation for character list
        if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
        {
            _characterList.SelectPrevious();
        }
        if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
        {
            _characterList.SelectNext();
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

        // Layer 2: Starfall effect (additive blend)
        if (_starfallEffect != null)
        {
            _starfallEffect.Parameters["Time"]?.SetValue(_elapsedTime);
            _starfallEffect.Parameters["Resolution"]?.SetValue(new Vector2(screenWidth, screenHeight));
            _starfallEffect.Parameters["Intensity"]?.SetValue(0.5f);

            spriteBatch.Begin(blendState: BlendState.Additive, effect: _starfallEffect);
            spriteBatch.Draw(_pixelTexture, screenRect, Color.White);
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
