using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Character status menu - view detailed character stats and information
/// </summary>
public class CharacterStatusState : IGameState
{
    private readonly GameStateManager _stateManager;
    private UIWindow _window;
    private KeyboardState _previousKeyState;

    // Character list
    private UIListBox _characterList;

    // Status panel and labels
    private UIPanel _statusPanel;
    private UILabel _nameLabel;
    private UILabel _levelLabel;
    private UILabel _hpLabel;
    private UILabel _mpLabel;
    private UILabel _expLabel;
    private UIProgressBar _expBar;
    private UILabel _statsLabel;
    private UILabel _statusEffectsLabel;

    // Layout dimensions
    private float _characterListWidth;
    private float _statusPanelWidth;

    private Entity _selectedCharacter;

    public CharacterStatusState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Calculate responsive window size
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        int windowWidth = Math.Clamp((int)(screenWidth * 0.7f), 850, 1100);
        int windowHeight = Math.Clamp((int)(screenHeight * 0.8f), 600, 800);

        _window = new UIWindow(Vector2.Zero, new Vector2(windowWidth, windowHeight), "Character Status")
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
        _statusPanelWidth = windowWidth - _characterListWidth - margin * 3;

        // Create layout sections
        CreateCharacterList(mainAreaHeight, margin);
        CreateStatusPanel(mainAreaHeight, margin);
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
    /// Create status panel on the right with vertical layout
    /// </summary>
    private void CreateStatusPanel(float height, float margin)
    {
        float xPos = _characterListWidth + margin * 2;
        float panelHeight = height - margin * 2;
        float innerWidth = _statusPanelWidth - 30; // Account for padding

        _statusPanel = new UIPanel(
            new Vector2(xPos, margin),
            new Vector2(_statusPanelWidth, panelHeight))
        {
            BackgroundColor = new Color(30, 30, 50, 200),
            Layout = PanelLayout.Vertical,
            Spacing = 5
        };
        _statusPanel.SetPadding(15);

        // Character header
        _nameLabel = new UILabel("Character Name", Vector2.Zero)
        {
            TextColor = Color.Gold,
            Scale = 1.2f
        };
        _statusPanel.AddChild(_nameLabel);

        _levelLabel = new UILabel("Level: 1", Vector2.Zero)
        {
            TextColor = Color.White
        };
        _statusPanel.AddChild(_levelLabel);

        // HP/MP display
        _hpLabel = new UILabel("HP: 100 / 100", Vector2.Zero)
        {
            TextColor = Color.LightGreen
        };
        _statusPanel.AddChild(_hpLabel);

        _mpLabel = new UILabel("MP: 20 / 20", Vector2.Zero)
        {
            TextColor = Color.LightBlue
        };
        _statusPanel.AddChild(_mpLabel);

        // Experience
        _expLabel = new UILabel("EXP: 0 / 100", Vector2.Zero)
        {
            TextColor = Color.Yellow
        };
        _statusPanel.AddChild(_expLabel);

        _expBar = new UIProgressBar(Vector2.Zero, new Vector2(innerWidth, 20))
        {
            FillColor = Color.Yellow,
            BackgroundColor = new Color(40, 40, 40),
            ShowText = false
        };
        _statusPanel.AddChild(_expBar);

        // Stats display
        var statsTitle = new UILabel("Stats", Vector2.Zero)
        {
            TextColor = Color.Cyan,
            Scale = 1.1f
        };
        _statusPanel.AddChild(statsTitle);

        _statsLabel = new UILabel("", Vector2.Zero)
        {
            TextColor = Color.White,
            Size = new Vector2(innerWidth, 180)
        };
        _statusPanel.AddChild(_statsLabel);

        // Status effects
        var statusEffectsTitle = new UILabel("Status Effects", Vector2.Zero)
        {
            TextColor = Color.Magenta,
            Scale = 1.1f
        };
        _statusPanel.AddChild(statusEffectsTitle);

        _statusEffectsLabel = new UILabel("None", Vector2.Zero)
        {
            TextColor = Color.LightGray,
            Size = new Vector2(innerWidth, 60)
        };
        _statusPanel.AddChild(_statusEffectsLabel);

        _window.ContentPanel.AddChild(_statusPanel);
    }

    /// <summary>
    /// Create instructions at the bottom
    /// </summary>
    private void CreateInstructions(float yPosition, float margin)
    {
        var instructionsLabel = new UILabel("Esc: Close", new Vector2(margin, yPosition))
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

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // UI Manager handles drawing
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
            RefreshStatusDisplay();
        }
    }

    /// <summary>
    /// Refresh the status display for the selected character
    /// </summary>
    private void RefreshStatusDisplay()
    {
        if (_selectedCharacter == null) return;

        var stats = _selectedCharacter.GetComponent<StatsComponent>();
        if (stats == null) return;

        // Update name and level
        _nameLabel.Text = _selectedCharacter.Name;
        _levelLabel.Text = $"Level: {stats.Level}";

        // Update HP/MP
        _hpLabel.Text = $"HP: {stats.CurrentHP} / {stats.MaxHP}";
        _mpLabel.Text = $"MP: {stats.CurrentMP} / {stats.MaxMP}";

        // Update experience
        int currentExp = stats.Experience;
        int expToNext = stats.ExperienceToNext;
        _expLabel.Text = $"EXP: {currentExp} / {expToNext}";
        _expBar.CurrentValue = currentExp;
        _expBar.MaxValue = expToNext;

        // Update stats
        var statsText = BuildStatsText(stats);
        _statsLabel.Text = statsText;

        // Update status effects
        var statusEffectsText = BuildStatusEffectsText(stats);
        _statusEffectsLabel.Text = statusEffectsText;
    }

    /// <summary>
    /// Build stats text display
    /// </summary>
    private string BuildStatsText(StatsComponent stats)
    {
        var lines = new System.Collections.Generic.List<string>();

        // Base stats
        lines.Add("--- Combat Stats ---");
        lines.Add($"Strength:      {stats.GetStat(StatType.Strength):D3}");
        lines.Add($"Defense:       {stats.GetStat(StatType.Defense):D3}");
        lines.Add($"Magic Power:   {stats.GetStat(StatType.MagicPower):D3}");
        lines.Add($"Magic Defense: {stats.GetStat(StatType.MagicDefense):D3}");
        lines.Add($"Speed:         {stats.GetStat(StatType.Speed):D3}");
        lines.Add($"Luck:          {stats.GetStat(StatType.Luck):D3}");

        // Show equipped items
        lines.Add("");
        lines.Add("--- Equipment ---");
        var equippedItems = stats.GetAllEquippedItems();
        if (equippedItems.Count > 0)
        {
            foreach (var kvp in equippedItems)
            {
                lines.Add($"{kvp.Key}: {kvp.Value}");
            }
        }
        else
        {
            lines.Add("No equipment");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Build status effects text display
    /// </summary>
    private string BuildStatusEffectsText(StatsComponent stats)
    {
        // TODO: Get actual status effects from StatsComponent
        // For now, just return "None"
        return "None";
    }
}
