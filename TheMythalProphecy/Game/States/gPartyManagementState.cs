using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.UI.Gleam;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Party management state using GleamUI with cosmic aesthetic.
/// Allows swapping and reordering party members between active and reserve.
/// </summary>
public class gPartyManagementState : IGameState
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

    // UI Elements
    private GleamPanel _mainPanel;
    private GleamLabel _titleLabel;
    private GleamLabel _activePartyTitle;
    private GleamCharacterList _activePartyList;
    private GleamLabel _reservePartyTitle;
    private GleamCharacterList _reservePartyList;
    private GleamPanel _buttonPanel;
    private GleamButton _swapButton;
    private GleamButton _moveToReserveButton;
    private GleamButton _moveToActiveButton;
    private GleamLabel _instructionsLabel;

    // State
    private int _focusedList; // 0=active, 1=reserve
    private KeyboardState _previousKeyState;
    private MouseState _previousMouseState;

    public gPartyManagementState(ContentManager content, GameStateManager stateManager)
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

        // Focus active party list by default
        _focusedList = 0;
        _activePartyList.IsFocused = true;
        _reservePartyList.IsFocused = false;

        // Populate lists
        RefreshPartyLists();
    }

    private void CreateUI()
    {
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Main panel dimensions
        int panelWidth = 750;
        int panelHeight = 650;
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
        int contentWidth = panelWidth - margin * 2;
        int titleHeight = 40;
        int sectionTitleHeight = 28;
        int listItemHeight = 55;
        int activeListHeight = 4 * listItemHeight + 8; // 4 slots + padding
        int reserveListHeight = 3 * listItemHeight + 8; // 3 visible + scroll
        int buttonHeight = 45;
        int instructionHeight = 24;

        float currentY = margin;

        // Title
        _titleLabel = new GleamLabel("Party Management", new Vector2(margin, currentY), new Vector2(contentWidth, titleHeight))
        {
            Alignment = TextAlignment.Center,
            Font = _theme.MenuFont
        };
        _mainPanel.AddChild(_titleLabel);
        currentY += titleHeight + margin;

        // Active party section title
        _activePartyTitle = new GleamLabel("Active Party (Max 4)", new Vector2(margin, currentY), new Vector2(contentWidth, sectionTitleHeight))
        {
            TextColor = _theme.GoldBright
        };
        _mainPanel.AddChild(_activePartyTitle);
        currentY += sectionTitleHeight + 4;

        // Active party list
        _activePartyList = new GleamCharacterList(new Vector2(margin, currentY), new Vector2(contentWidth, activeListHeight))
        {
            ItemHeight = listItemHeight,
            MaxSlots = 4,
            ShowEmptySlots = true
        };
        _activePartyList.OnSelectionChanged += OnActivePartySelectionChanged;
        _mainPanel.AddChild(_activePartyList);
        currentY += activeListHeight + margin;

        // Reserve party section title
        _reservePartyTitle = new GleamLabel("Reserve Party", new Vector2(margin, currentY), new Vector2(contentWidth, sectionTitleHeight))
        {
            TextColor = new Color(100, 200, 220) // Cyan-ish
        };
        _mainPanel.AddChild(_reservePartyTitle);
        currentY += sectionTitleHeight + 4;

        // Reserve party list
        _reservePartyList = new GleamCharacterList(new Vector2(margin, currentY), new Vector2(contentWidth, reserveListHeight))
        {
            ItemHeight = listItemHeight,
            MaxSlots = -1, // Unlimited
            ShowEmptySlots = false
        };
        _reservePartyList.OnSelectionChanged += OnReservePartySelectionChanged;
        _mainPanel.AddChild(_reservePartyList);
        currentY += reserveListHeight + margin;

        // Button panel
        int buttonWidth = 200;
        int buttonSpacing = 15;
        int totalButtonWidth = buttonWidth * 3 + buttonSpacing * 2;
        int buttonStartX = (panelWidth - totalButtonWidth) / 2;

        _buttonPanel = new GleamPanel(new Vector2(buttonStartX, currentY), new Vector2(totalButtonWidth, buttonHeight))
        {
            Layout = GleamLayout.Horizontal,
            Spacing = buttonSpacing,
            DrawBackground = false,
            DrawBorder = false
        };

        _swapButton = new GleamButton("Swap", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _swapButton.OnClick += _ => OnSwapClicked();
        _buttonPanel.AddChild(_swapButton);

        _moveToReserveButton = new GleamButton("To Reserve", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _moveToReserveButton.OnClick += _ => OnMoveToReserveClicked();
        _buttonPanel.AddChild(_moveToReserveButton);

        _moveToActiveButton = new GleamButton("To Active", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _moveToActiveButton.OnClick += _ => OnMoveToActiveClicked();
        _buttonPanel.AddChild(_moveToActiveButton);

        _mainPanel.AddChild(_buttonPanel);
        currentY += buttonHeight + margin;

        // Instructions
        _instructionsLabel = new GleamLabel(
            "Tab: Switch Lists | Arrows: Navigate | Enter: Select | Esc: Close",
            new Vector2(margin, currentY),
            new Vector2(contentWidth, instructionHeight))
        {
            Alignment = TextAlignment.Center,
            TextColor = _theme.TextSecondary
        };
        _mainPanel.AddChild(_instructionsLabel);
    }

    private void RefreshPartyLists()
    {
        var partyManager = GameServices.GameData.Party;

        // Remember selections
        int activeSelection = _activePartyList.SelectedIndex;
        int reserveSelection = _reservePartyList.SelectedIndex;

        // Update active party list
        _activePartyList.SetCharacters(partyManager.ActiveParty);

        // Update reserve party list
        _reservePartyList.SetCharacters(partyManager.ReserveParty);

        // Restore selections if valid
        if (activeSelection >= 0 && activeSelection < 4)
        {
            _activePartyList.SelectedIndex = activeSelection;
        }
        else if (partyManager.ActivePartyCount > 0)
        {
            _activePartyList.SelectedIndex = 0;
        }

        if (reserveSelection >= 0 && reserveSelection < partyManager.ReservePartyCount)
        {
            _reservePartyList.SelectedIndex = reserveSelection;
        }
        else if (partyManager.ReservePartyCount > 0)
        {
            _reservePartyList.SelectedIndex = 0;
        }

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        var partyManager = GameServices.GameData.Party;
        int activeIndex = _activePartyList.SelectedIndex;
        int reserveIndex = _reservePartyList.SelectedIndex;

        bool hasActiveSelection = activeIndex >= 0 && activeIndex < partyManager.ActivePartyCount;
        bool hasReserveSelection = reserveIndex >= 0 && reserveIndex < partyManager.ReservePartyCount;

        _swapButton.Enabled = hasActiveSelection && hasReserveSelection;
        _moveToReserveButton.Enabled = hasActiveSelection;
        _moveToActiveButton.Enabled = hasReserveSelection && partyManager.ActivePartyCount < 4;
    }

    private void OnActivePartySelectionChanged(GleamCharacterList sender, int index)
    {
        UpdateButtonStates();
    }

    private void OnReservePartySelectionChanged(GleamCharacterList sender, int index)
    {
        UpdateButtonStates();
    }

    private void OnSwapClicked()
    {
        var partyManager = GameServices.GameData.Party;
        int activeIndex = _activePartyList.SelectedIndex;
        int reserveIndex = _reservePartyList.SelectedIndex;

        if (activeIndex >= 0 && activeIndex < partyManager.ActivePartyCount &&
            reserveIndex >= 0 && reserveIndex < partyManager.ReservePartyCount)
        {
            partyManager.SwapActiveWithReserve(activeIndex, reserveIndex);
            RefreshPartyLists();
        }
    }

    private void OnMoveToReserveClicked()
    {
        var partyManager = GameServices.GameData.Party;
        int activeIndex = _activePartyList.SelectedIndex;

        if (activeIndex >= 0 && activeIndex < partyManager.ActivePartyCount)
        {
            partyManager.MoveToReserve(activeIndex);
            RefreshPartyLists();
        }
    }

    private void OnMoveToActiveClicked()
    {
        var partyManager = GameServices.GameData.Party;
        int reserveIndex = _reservePartyList.SelectedIndex;

        if (reserveIndex >= 0 && reserveIndex < partyManager.ReservePartyCount &&
            partyManager.ActivePartyCount < 4)
        {
            partyManager.MoveToActive(reserveIndex);
            RefreshPartyLists();
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

        // Tab to switch focus between lists
        if (keyState.IsKeyDown(Keys.Tab) && !_previousKeyState.IsKeyDown(Keys.Tab))
        {
            _focusedList = (_focusedList + 1) % 2;
            _activePartyList.IsFocused = (_focusedList == 0);
            _reservePartyList.IsFocused = (_focusedList == 1);
        }

        // Arrow key navigation
        var focusedList = _focusedList == 0 ? _activePartyList : _reservePartyList;

        if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
        {
            focusedList.SelectPrevious();
        }
        if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
        {
            focusedList.SelectNext();
        }

        // Enter to activate selected action
        if (keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter))
        {
            // Could trigger default action or nothing
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
