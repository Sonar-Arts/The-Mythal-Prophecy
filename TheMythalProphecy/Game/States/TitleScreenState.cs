using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Title screen state - displays game logo and main menu
/// </summary>
public class TitleScreenState : IGameState
{
    private readonly ContentManager _content;
    private readonly GameStateManager _stateManager;

    private Texture2D _logoTexture;
    private UIPanel _menuPanel;
    private UIButton _newGameButton;
    private UIButton _continueButton;
    private UIButton _optionsButton;
    private UIButton _exitButton;

    public TitleScreenState(ContentManager content, GameStateManager stateManager)
    {
        _content = content;
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Load logo texture
        var logoPath = Path.Combine("Game", "Art", "Logos", "TMP Logo.png");
        using var stream = File.OpenRead(logoPath);
        _logoTexture = Texture2D.FromStream(GameServices.GraphicsDevice, stream);

        CreateMenuUI();
    }

    private void CreateMenuUI()
    {
        var screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        var screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Menu button dimensions - scale with screen
        var buttonWidth = (int)(screenWidth * 0.17f);  // ~220px at 1280
        var buttonHeight = (int)(screenHeight * 0.07f); // ~50px at 720
        var buttonSpacing = (int)(screenHeight * 0.015f); // ~12px at 720
        var totalMenuHeight = (buttonHeight * 4) + (buttonSpacing * 3);

        // Center the menu panel in the lower portion of the screen
        var menuX = (screenWidth - buttonWidth) / 2f;
        var menuY = screenHeight * 0.5f; // Start at 50% of screen height

        // Create menu panel with vertical layout (no padding since we position manually)
        _menuPanel = new UIPanel(
            new Vector2(menuX, menuY),
            new Vector2(buttonWidth, totalMenuHeight)
        )
        {
            Layout = PanelLayout.Vertical,
            Spacing = buttonSpacing,
            DrawBackground = false,
            DrawBorder = false
        };
        _menuPanel.SetPadding(0);

        // Create menu buttons
        _newGameButton = new UIButton("New Game", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _newGameButton.OnClick += OnNewGameClicked;

        _continueButton = new UIButton("Continue", Vector2.Zero, new Vector2(buttonWidth, buttonHeight))
        {
            Enabled = false // Disabled until save system exists
        };
        _continueButton.OnClick += OnContinueClicked;

        _optionsButton = new UIButton("Options", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _optionsButton.OnClick += OnOptionsClicked;

        _exitButton = new UIButton("Exit", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _exitButton.OnClick += OnExitClicked;

        // Add buttons to panel
        _menuPanel.AddChild(_newGameButton);
        _menuPanel.AddChild(_continueButton);
        _menuPanel.AddChild(_optionsButton);
        _menuPanel.AddChild(_exitButton);

        // Register with UI manager
        GameServices.UI.AddElement(_menuPanel);
    }

    private void OnNewGameClicked(UIButton button)
    {
        _stateManager.ChangeState(new WorldMapState(_stateManager));
    }

    private void OnContinueClicked(UIButton button)
    {
        // TODO: Load saved game when save system is implemented
    }

    private void OnOptionsClicked(UIButton button)
    {
        _stateManager.PushState(new OptionsMenuState(_stateManager));
    }

    private void OnExitClicked(UIButton button)
    {
        Environment.Exit(0);
    }

    public void Exit()
    {
        // Unregister UI elements
        GameServices.UI.RemoveElement(_menuPanel);

        _logoTexture?.Dispose();
    }

    public void Pause()
    {
        // Hide UI when another state is pushed on top
        if (_menuPanel != null)
            _menuPanel.Visible = false;
    }

    public void Resume()
    {
        // Show UI when this state becomes active again
        if (_menuPanel != null)
            _menuPanel.Visible = true;

        // Note: TitleScreenState doesn't use keyboard input directly,
        // but keeping pattern consistent for future changes
    }

    public void Update(GameTime gameTime)
    {
        // UI input is handled by UIManager
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Clear to black background
        GameServices.GraphicsDevice.Clear(Color.Black);

        // Get screen dimensions
        var screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        var screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Scale logo to fit 50% of screen width
        var targetWidth = screenWidth * 0.5f;
        var logoScale = targetWidth / _logoTexture.Width;
        var logoWidth = _logoTexture.Width * logoScale;
        var logoHeight = _logoTexture.Height * logoScale;

        // Center logo horizontally, position at 25% from top of screen
        var logoPosition = new Vector2(
            (screenWidth - logoWidth) / 2f,
            screenHeight * 0.25f
        );

        // Draw logo (SpriteBatch is already begun by MythalGame)
        spriteBatch.Draw(
            _logoTexture,
            logoPosition,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            logoScale,
            SpriteEffects.None,
            0f
        );

        // UI elements are drawn by UIManager in MythalGame.Draw()
    }
}
