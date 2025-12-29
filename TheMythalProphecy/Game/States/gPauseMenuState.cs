using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.UI.Gleam;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Pause menu using GleamUI with cosmic/mystical aesthetic.
/// Displayed as a modal overlay when the player pauses the game.
/// </summary>
public class gPauseMenuState : IGameState
{
    public bool IsOverlay => true;

    private readonly ContentManager _content;
    private readonly GameStateManager _stateManager;

    private GleamRenderer _renderer;
    private GleamTheme _theme;
    private Texture2D _pixelTexture;

    // UI Elements
    private GleamPanel _mainPanel;

    // Menu buttons
    private GleamButton _itemsButton;
    private GleamButton _equipmentButton;
    private GleamButton _statusButton;
    private GleamButton _partyButton;
    private GleamButton _saveButton;
    private GleamButton _optionsButton;
    private GleamButton _resumeButton;
    private GleamButton _quitButton;

    private KeyboardState _previousKeyState;
    private MouseState _previousMouseState;

    public gPauseMenuState(ContentManager content, GameStateManager stateManager)
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

        // Create pixel texture for semi-transparent overlay
        _pixelTexture = new Texture2D(GameServices.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        CreateUI();

        // Initialize input state to prevent immediate ESC detection
        _previousKeyState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
    }

    private void CreateUI()
    {
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Button dimensions - proportional to screen
        int buttonWidth = Math.Clamp((int)(screenWidth * 0.18f), 200, 280);
        int buttonHeight = Math.Clamp((int)(screenHeight * 0.055f), 40, 50);
        int buttonSpacing = (int)(screenHeight * 0.012f);

        // Calculate panel size based on 8 buttons
        int totalButtonsHeight = (buttonHeight * 8) + (buttonSpacing * 7);
        int panelPadding = 20;
        int panelWidth = buttonWidth + panelPadding * 2;
        int panelHeight = totalButtonsHeight + panelPadding * 2;

        // Center the panel
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;

        // Create main container panel
        _mainPanel = new GleamPanel(new Vector2(panelX, panelY), new Vector2(panelWidth, panelHeight))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.95f,
            Layout = GleamLayout.Vertical,
            Spacing = buttonSpacing,
            Padding = panelPadding,
            CenterChildren = true
        };

        // Create buttons
        Vector2 buttonSize = new Vector2(buttonWidth, buttonHeight);

        _itemsButton = new GleamButton("Items", Vector2.Zero, buttonSize);
        _itemsButton.OnClick += _ => OnItemsClicked();

        _equipmentButton = new GleamButton("Equipment", Vector2.Zero, buttonSize);
        _equipmentButton.OnClick += _ => OnEquipmentClicked();

        _statusButton = new GleamButton("Status", Vector2.Zero, buttonSize);
        _statusButton.OnClick += _ => OnStatusClicked();

        _partyButton = new GleamButton("Party", Vector2.Zero, buttonSize);
        _partyButton.OnClick += _ => OnPartyClicked();

        _saveButton = new GleamButton("Save", Vector2.Zero, buttonSize);
        _saveButton.Enabled = false; // TODO: Enable when save system is implemented
        _saveButton.OnClick += _ => OnSaveClicked();

        _optionsButton = new GleamButton("Options", Vector2.Zero, buttonSize);
        _optionsButton.OnClick += _ => OnOptionsClicked();

        _resumeButton = new GleamButton("Resume", Vector2.Zero, buttonSize);
        _resumeButton.OnClick += _ => OnResumeClicked();

        _quitButton = new GleamButton("Quit to Title", Vector2.Zero, buttonSize);
        _quitButton.OnClick += _ => OnQuitClicked();

        // Add buttons to panel (order matters for vertical layout)
        _mainPanel.AddChild(_itemsButton);
        _mainPanel.AddChild(_equipmentButton);
        _mainPanel.AddChild(_statusButton);
        _mainPanel.AddChild(_partyButton);
        _mainPanel.AddChild(_saveButton);
        _mainPanel.AddChild(_optionsButton);
        _mainPanel.AddChild(_resumeButton);
        _mainPanel.AddChild(_quitButton);
    }

    private void OnItemsClicked()
    {
        _stateManager.PushState(new InventoryState(_stateManager));
    }

    private void OnEquipmentClicked()
    {
        _stateManager.PushState(new EquipmentState(_stateManager));
    }

    private void OnStatusClicked()
    {
        _stateManager.PushState(new gCharacterStatusState(_content, _stateManager));
    }

    private void OnPartyClicked()
    {
        _stateManager.PushState(new gPartyManagementState(_content, _stateManager));
    }

    private void OnSaveClicked()
    {
        // TODO: Push SaveMenuState when implemented
    }

    private void OnOptionsClicked()
    {
        _stateManager.PushState(new gOptionsMenuState(_content, _stateManager));
    }

    private void OnResumeClicked()
    {
        _stateManager.PopState();
    }

    private void OnQuitClicked()
    {
        // Clear all states and return to title
        while (_stateManager.CurrentState != null)
        {
            _stateManager.PopState();
        }
        _stateManager.ChangeState(new TitleScreenState(GameServices.Content, _stateManager));
    }

    public void Exit()
    {
        _pixelTexture?.Dispose();
    }

    public void Pause()
    {
        // Hide panel when another state is pushed on top
        if (_mainPanel != null)
            _mainPanel.Visible = false;
    }

    public void Resume()
    {
        // Show panel when this state becomes active again
        if (_mainPanel != null)
            _mainPanel.Visible = true;

        // Reset input state to prevent immediate re-triggering
        _previousKeyState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
    }

    public void Update(GameTime gameTime)
    {
        // Handle ESC to resume game
        var keyState = Keyboard.GetState();
        if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
        {
            OnResumeClicked();
            _previousKeyState = keyState;
            return;
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

        // Draw semi-transparent overlay (dims the game behind the menu)
        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        spriteBatch.Draw(_pixelTexture, screenRect, new Color(0, 0, 0, 180));
        spriteBatch.End();

        // Draw UI elements
        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        _mainPanel.Draw(spriteBatch, _renderer);
        spriteBatch.End();

        // Resume normal batch for other rendering
        spriteBatch.Begin();
    }
}
