using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.UI.Gleam;

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
    private Effect _logoShimmerEffect;
    private SpriteFont _menuFont;
    private float _elapsedTime;

    // GleamUI
    private GleamTheme _theme;
    private GleamRenderer _renderer;
    private GleamPanel _menuPanel;
    private GleamButton _newGameButton;
    private GleamButton _continueButton;
    private GleamButton _optionsButton;
    private GleamButton _exitButton;
    private MouseState _previousMouseState;

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
        _logoShimmerEffect = _content.Load<Effect>("Effects/LogoShimmer");

        // Load fancy menu font
        _menuFont = _content.Load<SpriteFont>("Fonts/MenuTitle");

        // Initialize GleamUI
        var defaultFont = _content.Load<SpriteFont>("Fonts/Default");
        _theme = new GleamTheme();
        _theme.Initialize(defaultFont, _menuFont);
        _renderer = new GleamRenderer();
        _renderer.Initialize(GameServices.GraphicsDevice, _content, _theme);

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

        // Position in middle left (vertically centered)
        var margin = (int)(screenHeight * 0.05f);
        var menuX = margin;
        var menuY = (screenHeight - totalMenuHeight) / 2f;

        // Create menu panel with vertical layout
        _menuPanel = new GleamPanel(
            new Vector2(menuX, menuY),
            new Vector2(buttonWidth, totalMenuHeight))
        {
            Layout = GleamLayout.Vertical,
            Spacing = buttonSpacing,
            Padding = 0,
            DrawBackground = false,
            DrawBorder = false
        };

        // Create parallelogram menu buttons (GleamButton uses theme colors by default)
        _newGameButton = new GleamButton("New Game", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _newGameButton.OnClick += _ => OnNewGameClicked();

        _continueButton = new GleamButton("Continue", Vector2.Zero, new Vector2(buttonWidth, buttonHeight))
        {
            Enabled = false // Disabled until save system exists
        };
        _continueButton.OnClick += _ => OnContinueClicked();

        _optionsButton = new GleamButton("Options", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _optionsButton.OnClick += _ => OnOptionsClicked();

        _exitButton = new GleamButton("Exit", Vector2.Zero, new Vector2(buttonWidth, buttonHeight));
        _exitButton.OnClick += _ => OnExitClicked();

        // Add buttons to panel
        _menuPanel.AddChild(_newGameButton);
        _menuPanel.AddChild(_continueButton);
        _menuPanel.AddChild(_optionsButton);
        _menuPanel.AddChild(_exitButton);
    }

    private void OnNewGameClicked()
    {
        _stateManager.ChangeState(new WorldMapState(_stateManager));
    }

    private void OnContinueClicked()
    {
        // TODO: Load saved game when save system is implemented
    }

    private void OnOptionsClicked()
    {
        _stateManager.PushState(new gOptionsMenuState(_content, _stateManager));
    }

    private void OnExitClicked()
    {
        Environment.Exit(0);
    }

    public void Exit()
    {
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

        _previousMouseState = Mouse.GetState();
    }

    public void Update(GameTime gameTime)
    {
        // Track elapsed time for shader animation
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Handle mouse input for GleamUI
        var mouseState = Mouse.GetState();
        Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
        bool mouseDown = mouseState.LeftButton == ButtonState.Pressed;
        bool mouseClicked = mouseDown && _previousMouseState.LeftButton == ButtonState.Released;
        _previousMouseState = mouseState;

        _menuPanel.Update(gameTime, _renderer);
        _menuPanel.HandleInput(mousePos, mouseDown, mouseClicked);
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

        // === Layer 3: Logo with effects ===
        // Scale logo to fit 65% of screen width (1.3x original)
        var targetWidth = screenWidth * 0.65f;
        var logoScale = targetWidth / _logoTexture.Width;
        var logoWidth = _logoTexture.Width * logoScale;
        var logoHeight = _logoTexture.Height * logoScale;

        // Position logo to the right to avoid UI buttons on left
        var logoPosition = new Vector2(
            screenWidth * 0.55f - logoWidth / 2f,
            (screenHeight - logoHeight) / 2f
        );

        // Layer 3a: Drop shadow (separate, not affected by shimmer)
        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        var shadowOffset = new Vector2(5, 5) * logoScale;
        var shadowColor = new Color(0, 0, 0, 128);
        spriteBatch.Draw(_logoTexture, logoPosition + shadowOffset, null, shadowColor, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);

        // Layer 3b: Dark outline (4 draws at cardinal directions)
        var outlineColor = new Color(20, 10, 30);
        float outlineOffset = 2f * logoScale;
        spriteBatch.Draw(_logoTexture, logoPosition + new Vector2(-outlineOffset, 0), null, outlineColor, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(_logoTexture, logoPosition + new Vector2(outlineOffset, 0), null, outlineColor, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(_logoTexture, logoPosition + new Vector2(0, -outlineOffset), null, outlineColor, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(_logoTexture, logoPosition + new Vector2(0, outlineOffset), null, outlineColor, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        spriteBatch.End();

        // Layer 3c: Main logo with vertical shimmer effect
        // First shimmer after 15 seconds, then every 60 seconds, takes 5 seconds to sweep
        float initialDelay = 15f;
        float shimmerInterval = 60f;
        float shimmerDuration = 5f;
        float adjustedTime = MathF.Max(0f, _elapsedTime - initialDelay);
        float cycleTime = adjustedTime % shimmerInterval;
        float shimmerPhase = cycleTime < shimmerDuration ? cycleTime / shimmerDuration : -1f;

        _logoShimmerEffect.Parameters["Time"]?.SetValue(_elapsedTime);
        _logoShimmerEffect.Parameters["ShimmerPhase"]?.SetValue(shimmerPhase);

        spriteBatch.Begin(blendState: BlendState.AlphaBlend, effect: _logoShimmerEffect);
        spriteBatch.Draw(_logoTexture, logoPosition, null, Color.White, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        spriteBatch.End();

        // === Layer 4: GleamUI Menu ===
        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        _menuPanel.Draw(spriteBatch, _renderer);
        spriteBatch.End();

        // Resume normal batch for MythalGame
        spriteBatch.Begin();
    }
}
