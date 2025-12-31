using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data;
using TheMythalProphecy.Game.UI.Gleam;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Options menu using GleamUI with cosmic/mystical aesthetic.
/// </summary>
public class gOptionsMenuScreen : IGameState
{
    private readonly ContentManager _content;
    private readonly GameStateManager _stateManager;

    private GleamRenderer _renderer;
    private GleamTheme _theme;
    private GameSettings _settings;

    // Shader effects
    private Effect _nebulaEffect;
    private Effect _starfallEffect;
    private Texture2D _pixelTexture;
    private float _elapsedTime;

    // UI Elements
    private GleamPanel _mainPanel;
    private GleamPanel _categoryPanel;
    private GleamPanel _settingsPanel;
    private GleamPanel _buttonPanel;

    // Category buttons
    private GleamButton _audioButton;
    private GleamButton _videoButton;
    private int _selectedCategory;

    // Audio controls
    private GleamPanel _audioSettingsPanel;
    private GleamLabel _masterVolumeLabel;
    private GleamSlider _masterVolumeSlider;
    private GleamLabel _musicVolumeLabel;
    private GleamSlider _musicVolumeSlider;
    private GleamLabel _sfxVolumeLabel;
    private GleamSlider _sfxVolumeSlider;

    // Video controls
    private GleamPanel _videoSettingsPanel;
    private GleamLabel _resolutionLabel;
    private GleamSelector _resolutionSelector;
    private GleamLabel _fullscreenLabel;
    private GleamToggle _fullscreenToggle;

    // Action buttons
    private GleamButton _applyButton;
    private GleamButton _cancelButton;

    private KeyboardState _previousKeyState;
    private MouseState _previousMouseState;

    private readonly (int Width, int Height)[] _resolutions =
    {
        (1280, 720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };

    public gOptionsMenuScreen(ContentManager content, GameStateManager stateManager)
    {
        _content = content;
        _stateManager = stateManager;
    }

    public void Enter()
    {
        _settings = GameSettings.Load();

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
    }

    private void CreateUI()
    {
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Main panel dimensions (60% width, 70% height, centered)
        int panelWidth = Math.Clamp((int)(screenWidth * 0.6f), 600, 900);
        int panelHeight = Math.Clamp((int)(screenHeight * 0.7f), 450, 650);
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;

        // Main container
        _mainPanel = new GleamPanel(new Vector2(panelX, panelY), new Vector2(panelWidth, panelHeight))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.95f
        };

        // Layout dimensions
        int margin = 16;
        int categoryWidth = 140;
        int buttonHeight = 45;
        int buttonAreaHeight = 60;
        int settingsWidth = panelWidth - categoryWidth - margin * 3;
        int mainAreaHeight = panelHeight - buttonAreaHeight - margin * 2;

        // Category panel (left side)
        _categoryPanel = new GleamPanel(
            new Vector2(margin, margin),
            new Vector2(categoryWidth, mainAreaHeight))
        {
            Layout = GleamLayout.Vertical,
            Spacing = 8,
            Padding = 8,
            DrawBackground = false,
            DrawBorder = false
        };

        _audioButton = new GleamButton("Audio", Vector2.Zero, new Vector2(categoryWidth - 16, buttonHeight));
        _audioButton.OnClick += _ => SelectCategory(0);

        _videoButton = new GleamButton("Video", Vector2.Zero, new Vector2(categoryWidth - 16, buttonHeight));
        _videoButton.OnClick += _ => SelectCategory(1);

        _categoryPanel.AddChild(_audioButton);
        _categoryPanel.AddChild(_videoButton);
        _mainPanel.AddChild(_categoryPanel);

        // Settings panel (right side)
        _settingsPanel = new GleamPanel(
            new Vector2(categoryWidth + margin * 2, margin),
            new Vector2(settingsWidth, mainAreaHeight))
        {
            DrawBackground = true,
            DrawBorder = true,
            BackgroundAlpha = 0.5f
        };
        _mainPanel.AddChild(_settingsPanel);

        // Create audio settings
        CreateAudioSettings(settingsWidth);

        // Create video settings
        CreateVideoSettings(settingsWidth);

        // Button panel (bottom)
        _buttonPanel = new GleamPanel(
            new Vector2(margin, panelHeight - buttonAreaHeight - margin),
            new Vector2(panelWidth - margin * 2, buttonAreaHeight))
        {
            Layout = GleamLayout.Horizontal,
            Spacing = 16,
            Padding = 8,
            DrawBackground = false,
            DrawBorder = false
        };

        int actionButtonWidth = 140;
        _applyButton = new GleamButton("Apply", Vector2.Zero, new Vector2(actionButtonWidth, buttonHeight - 8));
        _applyButton.OnClick += _ => OnApply();

        _cancelButton = new GleamButton("Cancel", Vector2.Zero, new Vector2(actionButtonWidth, buttonHeight - 8));
        _cancelButton.OnClick += _ => OnCancel();

        _buttonPanel.AddChild(_applyButton);
        _buttonPanel.AddChild(_cancelButton);
        _mainPanel.AddChild(_buttonPanel);

        // Select audio category by default
        SelectCategory(0);
    }

    private void CreateAudioSettings(int width)
    {
        int contentWidth = width - 32;

        _audioSettingsPanel = new GleamPanel(Vector2.Zero, new Vector2(width, 300))
        {
            Layout = GleamLayout.Vertical,
            Spacing = 12,
            Padding = 16,
            DrawBackground = false,
            DrawBorder = false
        };

        // Master Volume
        _masterVolumeLabel = new GleamLabel($"Master Volume: {(int)(_settings.MasterVolume * 100)}%", Vector2.Zero);
        _audioSettingsPanel.AddChild(_masterVolumeLabel);

        _masterVolumeSlider = new GleamSlider(Vector2.Zero, new Vector2(contentWidth, 28), 0, 1, _settings.MasterVolume);
        _masterVolumeSlider.OnValueChanged += (_, value) =>
        {
            _settings.MasterVolume = value;
            _masterVolumeLabel.Text = $"Master Volume: {(int)(value * 100)}%";
        };
        _audioSettingsPanel.AddChild(_masterVolumeSlider);

        // Music Volume
        _musicVolumeLabel = new GleamLabel($"Music Volume: {(int)(_settings.MusicVolume * 100)}%", Vector2.Zero);
        _audioSettingsPanel.AddChild(_musicVolumeLabel);

        _musicVolumeSlider = new GleamSlider(Vector2.Zero, new Vector2(contentWidth, 28), 0, 1, _settings.MusicVolume);
        _musicVolumeSlider.OnValueChanged += (_, value) =>
        {
            _settings.MusicVolume = value;
            _musicVolumeLabel.Text = $"Music Volume: {(int)(value * 100)}%";
        };
        _audioSettingsPanel.AddChild(_musicVolumeSlider);

        // SFX Volume
        _sfxVolumeLabel = new GleamLabel($"SFX Volume: {(int)(_settings.SFXVolume * 100)}%", Vector2.Zero);
        _audioSettingsPanel.AddChild(_sfxVolumeLabel);

        _sfxVolumeSlider = new GleamSlider(Vector2.Zero, new Vector2(contentWidth, 28), 0, 1, _settings.SFXVolume);
        _sfxVolumeSlider.OnValueChanged += (_, value) =>
        {
            _settings.SFXVolume = value;
            _sfxVolumeLabel.Text = $"SFX Volume: {(int)(value * 100)}%";
        };
        _audioSettingsPanel.AddChild(_sfxVolumeSlider);
    }

    private void CreateVideoSettings(int width)
    {
        int contentWidth = width - 32;

        _videoSettingsPanel = new GleamPanel(Vector2.Zero, new Vector2(width, 300))
        {
            Layout = GleamLayout.Vertical,
            Spacing = 12,
            Padding = 16,
            DrawBackground = false,
            DrawBorder = false
        };

        // Resolution
        _resolutionLabel = new GleamLabel("Resolution", Vector2.Zero, new Vector2(contentWidth, 24));
        _videoSettingsPanel.AddChild(_resolutionLabel);

        _resolutionSelector = new GleamSelector(Vector2.Zero, new Vector2(contentWidth, 36));
        string[] resOptions = new string[_resolutions.Length];
        int selectedIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            resOptions[i] = $"{_resolutions[i].Width} x {_resolutions[i].Height}";
            if (_resolutions[i].Width == _settings.ResolutionWidth &&
                _resolutions[i].Height == _settings.ResolutionHeight)
            {
                selectedIndex = i;
            }
        }
        _resolutionSelector.SetOptions(resOptions, selectedIndex);
        _resolutionSelector.OnSelectionChanged += (_, index) =>
        {
            _settings.ResolutionWidth = _resolutions[index].Width;
            _settings.ResolutionHeight = _resolutions[index].Height;
        };
        _videoSettingsPanel.AddChild(_resolutionSelector);

        // Fullscreen
        _fullscreenLabel = new GleamLabel("Fullscreen", Vector2.Zero, new Vector2(contentWidth, 24));
        _videoSettingsPanel.AddChild(_fullscreenLabel);

        _fullscreenToggle = new GleamToggle(Vector2.Zero, new Vector2(100, 36), _settings.Fullscreen);
        _fullscreenToggle.OnToggled += (_, isOn) =>
        {
            _settings.Fullscreen = isOn;
        };
        _videoSettingsPanel.AddChild(_fullscreenToggle);
    }

    private void SelectCategory(int index)
    {
        _selectedCategory = index;

        // Update button states
        _audioButton.NormalColor = index == 0 ? _theme.MidPurple : null;
        _videoButton.NormalColor = index == 1 ? _theme.MidPurple : null;

        // Swap settings panel content
        _settingsPanel.ClearChildren();
        var panel = index == 0 ? _audioSettingsPanel : _videoSettingsPanel;
        panel.Size = new Vector2(_settingsPanel.Size.X, _settingsPanel.Size.Y);
        _settingsPanel.AddChild(panel);
    }

    private void OnApply()
    {
        _settings.ApplyAudioSettings();
        _settings.ApplyVideoSettings();
        _settings.Save();
        _stateManager.PopState();
    }

    private void OnCancel()
    {
        _stateManager.PopState();
    }

    public void Exit()
    {
        _pixelTexture?.Dispose();
    }

    public void Pause()
    {
        // Nothing to do
    }

    public void Resume()
    {
        _previousKeyState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Handle ESC to cancel
        var keyState = Keyboard.GetState();
        if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
        {
            OnCancel();
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

        // Clear to black (overrides MythalGame's blue clear)
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
