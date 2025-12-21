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
    private Texture2D _pixelTexture;
    private Effect _nebulaEffect;
    private Effect _starfallEffect;
    private SpriteFont _menuFont;
    private float _elapsedTime;
    private UIPanel _menuPanel;
    private DiamondButton _newGameButton;
    private DiamondButton _continueButton;
    private DiamondButton _optionsButton;
    private DiamondButton _exitButton;

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

        // Create 1x1 white pixel for full-screen shader quad
        _pixelTexture = new Texture2D(GameServices.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load shader effects
        _nebulaEffect = _content.Load<Effect>("Effects/Nebula");
        _starfallEffect = _content.Load<Effect>("Effects/Starfall");

        // Load fancy menu font
        _menuFont = _content.Load<SpriteFont>("Fonts/MenuTitle");

        CreateMenuUI();
    }

    private void CreateMenuUI()
    {
        var screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        var screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Parallelogram button dimensions - wider than tall
        var buttonWidth = (int)(screenWidth * 0.15f);  // ~192px at 1280
        var buttonHeight = (int)(screenHeight * 0.06f); // ~43px at 720
        var buttonSpacing = (int)(screenHeight * 0.015f); // ~11px at 720
        var totalMenuHeight = (buttonHeight * 4) + (buttonSpacing * 3);

        // Position in lower left corner with margin
        var margin = (int)(screenHeight * 0.05f);
        var menuX = margin;
        var menuY = screenHeight - totalMenuHeight - margin;

        // Create menu panel with vertical layout
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

        // Create parallelogram menu buttons with mystical styling
        _newGameButton = new DiamondButton("New Game", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _newGameButton.OnClick += OnNewGameClicked;
        ApplyMysticalStyle(_newGameButton);

        _continueButton = new DiamondButton("Continue", Vector2.Zero, new Vector2(buttonWidth, buttonHeight))
        {
            Enabled = false // Disabled until save system exists
        };
        _continueButton.OnClick += OnContinueClicked;
        ApplyMysticalStyle(_continueButton);

        _optionsButton = new DiamondButton("Options", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _optionsButton.OnClick += OnOptionsClicked;
        ApplyMysticalStyle(_optionsButton);

        _exitButton = new DiamondButton("Exit", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _exitButton.OnClick += OnExitClicked;
        ApplyMysticalStyle(_exitButton);

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

    private void ApplyMysticalStyle(UIButton button)
    {
        button.NormalColor = new Color(15, 4, 31);      // Deep purple
        button.HoverColor = new Color(31, 8, 56);       // Mid purple
        button.PressedColor = new Color(8, 3, 15);      // Nearly black purple
        button.DisabledColor = new Color(20, 10, 30);   // Muted purple
        button.BorderColor = new Color(179, 128, 51);   // Gold border
        button.BorderThickness = 2;
        button.Font = _menuFont;                        // Fancy menu font
    }

    public void Exit()
    {
        // Unregister UI elements
        GameServices.UI.RemoveElement(_menuPanel);

        _logoTexture?.Dispose();
        _pixelTexture?.Dispose();
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
        // Track elapsed time for shader animation
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // UI input is handled by UIManager
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Clear to black background
        GameServices.GraphicsDevice.Clear(Color.Black);

        // Get screen dimensions
        var screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        var screenHeight = GameServices.GraphicsDevice.Viewport.Height;
        var screenRect = new Rectangle(0, 0, screenWidth, screenHeight);

        // End the batch started by MythalGame to draw our layers
        spriteBatch.End();

        // === Layer 1: Nebula mist (alpha blend) ===
        _nebulaEffect.Parameters["Time"]?.SetValue(_elapsedTime);
        _nebulaEffect.Parameters["Intensity"]?.SetValue(1.0f);

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            effect: _nebulaEffect
        );
        spriteBatch.Draw(_pixelTexture, screenRect, Color.White);
        spriteBatch.End();

        // === Layer 2: Starfall (additive) ===
        _starfallEffect.Parameters["Time"]?.SetValue(_elapsedTime);
        _starfallEffect.Parameters["Resolution"]?.SetValue(new Vector2(screenWidth, screenHeight));
        _starfallEffect.Parameters["Intensity"]?.SetValue(0.7f);

        spriteBatch.Begin(
            blendState: BlendState.Additive,
            effect: _starfallEffect
        );
        spriteBatch.Draw(_pixelTexture, screenRect, Color.White);
        spriteBatch.End();

        // === Layer 3: Logo ===
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

        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
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
        spriteBatch.End();

        // Resume normal batch for UI
        spriteBatch.Begin();

        // UI elements are drawn by UIManager in MythalGame.Draw()
    }
}
