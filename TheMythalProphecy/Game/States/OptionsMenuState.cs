using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Options/Settings menu - configure audio, video, and controls
/// </summary>
public class OptionsMenuState : IGameState
{
    private readonly GameStateManager _stateManager;
    private UIWindow _window;
    private UIListBox _categoryList;
    private KeyboardState _previousKeyState;

    private GameSettings _settings;

    // Layout panels
    private UIPanel _settingsContainer;
    private UIPanel _audioPanel;
    private UIPanel _videoPanel;
    private UIPanel _buttonPanel;

    // Audio controls (references for dynamic updates)
    private UILabel _masterVolumeLabel;
    private UISlider _masterVolumeSlider;
    private UILabel _musicVolumeLabel;
    private UISlider _musicVolumeSlider;
    private UILabel _sfxVolumeLabel;
    private UISlider _sfxVolumeSlider;

    // Video controls (references for dynamic updates)
    private UIButton _resolutionButton;
    private UIButton _fullscreenButton;

    // Buttons
    private UIButton _applyButton;
    private UIButton _cancelButton;

    // Layout dimensions (calculated from screen size)
    private float _settingsWidth;

    private int _selectedResolutionIndex = 0;
    private readonly (int Width, int Height)[] _resolutions = new[]
    {
        (1280, 720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };

    public OptionsMenuState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Load current settings
        _settings = GameSettings.Load();

        // Calculate responsive window size
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        int windowWidth = Math.Clamp((int)(screenWidth * 0.6f), 700, 1000);
        int windowHeight = Math.Clamp((int)(screenHeight * 0.7f), 500, 700);

        _window = new UIWindow(Vector2.Zero, new Vector2(windowWidth, windowHeight), "Options")
        {
            IsModal = true,
            ShowCloseButton = false
        };
        _window.Center(screenWidth, screenHeight);

        // Disable auto-layout on content panel (we position main areas manually)
        _window.ContentPanel.Layout = PanelLayout.None;

        // Calculate layout dimensions
        float contentHeight = windowHeight - _window.TitleBarHeight;
        float buttonAreaHeight = 60;
        float mainAreaHeight = contentHeight - buttonAreaHeight;
        float categoryListWidth = 160;
        float margin = 10;
        _settingsWidth = windowWidth - categoryListWidth - (margin * 3);

        // Create main layout areas
        CreateCategoryList(mainAreaHeight, categoryListWidth, margin);
        CreateSettingsContainer(mainAreaHeight, categoryListWidth, margin);
        CreateButtonPanel(contentHeight - buttonAreaHeight, margin);

        // Create category-specific panels
        CreateAudioPanel();
        CreateVideoPanel();

        // Register window and show initial category
        GameServices.UI.AddElement(_window);
        _categoryList.IsFocused = true;
        SwitchToCategory(0);
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

        // Escape to cancel
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
    /// Create the category list on the left side
    /// </summary>
    private void CreateCategoryList(float height, float width, float margin)
    {
        _categoryList = new UIListBox(new Vector2(margin, margin), new Vector2(width, height - margin * 2))
        {
            ItemHeight = 40
        };
        _categoryList.AddItem("Audio");
        _categoryList.AddItem("Video");
        _categoryList.SelectedIndex = 0;
        _categoryList.OnSelectionChanged += OnCategoryChanged;
        _window.ContentPanel.AddChild(_categoryList);
    }

    /// <summary>
    /// Create the settings container on the right side
    /// </summary>
    private void CreateSettingsContainer(float height, float categoryListWidth, float margin)
    {
        float xPos = categoryListWidth + margin * 2;
        _settingsContainer = new UIPanel(new Vector2(xPos, margin), new Vector2(_settingsWidth, height - margin * 2))
        {
            BackgroundColor = new Color(30, 30, 50, 200),
            Layout = PanelLayout.None
        };
        _settingsContainer.SetPadding(0);
        _window.ContentPanel.AddChild(_settingsContainer);
    }

    /// <summary>
    /// Create the button panel at the bottom
    /// </summary>
    private void CreateButtonPanel(float yPosition, float margin)
    {
        _buttonPanel = new UIPanel(new Vector2(margin, yPosition), new Vector2(320, 50))
        {
            Layout = PanelLayout.Horizontal,
            Spacing = 10,
            DrawBackground = false,
            DrawBorder = false
        };
        _buttonPanel.SetPadding(0);

        _applyButton = new UIButton("Apply", Vector2.Zero, new Vector2(150, 40));
        _applyButton.OnClick += OnApplyClicked;
        _buttonPanel.AddChild(_applyButton);

        _cancelButton = new UIButton("Cancel", Vector2.Zero, new Vector2(150, 40));
        _cancelButton.OnClick += OnCancelClicked;
        _buttonPanel.AddChild(_cancelButton);

        _window.ContentPanel.AddChild(_buttonPanel);
    }

    /// <summary>
    /// Create audio settings panel with vertical layout
    /// </summary>
    private void CreateAudioPanel()
    {
        _audioPanel = new UIPanel(Vector2.Zero, new Vector2(_settingsWidth, 400))
        {
            Layout = PanelLayout.Vertical,
            Spacing = 8,
            DrawBackground = false,
            DrawBorder = false
        };
        _audioPanel.SetPadding(15);

        float sliderWidth = _settingsWidth - 40;

        // Master Volume
        _masterVolumeLabel = new UILabel($"Master Volume: {(int)(_settings.MasterVolume * 100)}%", Vector2.Zero)
        {
            TextColor = Color.White
        };
        _audioPanel.AddChild(_masterVolumeLabel);

        _masterVolumeSlider = new UISlider(Vector2.Zero, new Vector2(sliderWidth, 24), 0, 1, _settings.MasterVolume);
        _masterVolumeSlider.OnValueChanged += (slider, value) =>
        {
            _settings.MasterVolume = value;
            _masterVolumeLabel.Text = $"Master Volume: {(int)(value * 100)}%";
        };
        _audioPanel.AddChild(_masterVolumeSlider);

        // Music Volume
        _musicVolumeLabel = new UILabel($"Music Volume: {(int)(_settings.MusicVolume * 100)}%", Vector2.Zero)
        {
            TextColor = Color.White
        };
        _audioPanel.AddChild(_musicVolumeLabel);

        _musicVolumeSlider = new UISlider(Vector2.Zero, new Vector2(sliderWidth, 24), 0, 1, _settings.MusicVolume);
        _musicVolumeSlider.OnValueChanged += (slider, value) =>
        {
            _settings.MusicVolume = value;
            _musicVolumeLabel.Text = $"Music Volume: {(int)(value * 100)}%";
        };
        _audioPanel.AddChild(_musicVolumeSlider);

        // SFX Volume
        _sfxVolumeLabel = new UILabel($"SFX Volume: {(int)(_settings.SFXVolume * 100)}%", Vector2.Zero)
        {
            TextColor = Color.White
        };
        _audioPanel.AddChild(_sfxVolumeLabel);

        _sfxVolumeSlider = new UISlider(Vector2.Zero, new Vector2(sliderWidth, 24), 0, 1, _settings.SFXVolume);
        _sfxVolumeSlider.OnValueChanged += (slider, value) =>
        {
            _settings.SFXVolume = value;
            _sfxVolumeLabel.Text = $"SFX Volume: {(int)(value * 100)}%";
        };
        _audioPanel.AddChild(_sfxVolumeSlider);
    }

    /// <summary>
    /// Create video settings panel with vertical layout
    /// </summary>
    private void CreateVideoPanel()
    {
        _videoPanel = new UIPanel(Vector2.Zero, new Vector2(_settingsWidth, 400))
        {
            Layout = PanelLayout.Vertical,
            Spacing = 8,
            DrawBackground = false,
            DrawBorder = false
        };
        _videoPanel.SetPadding(15);

        // Find current resolution index
        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i].Width == _settings.ResolutionWidth &&
                _resolutions[i].Height == _settings.ResolutionHeight)
            {
                _selectedResolutionIndex = i;
                break;
            }
        }

        // Resolution label
        var resolutionLabel = new UILabel("Resolution:", Vector2.Zero)
        {
            TextColor = Color.White
        };
        _videoPanel.AddChild(resolutionLabel);

        // Resolution button
        var currentRes = _resolutions[_selectedResolutionIndex];
        _resolutionButton = new UIButton($"{currentRes.Width} x {currentRes.Height}", Vector2.Zero, new Vector2(300, 40));
        _resolutionButton.OnClick += OnResolutionClicked;
        _videoPanel.AddChild(_resolutionButton);

        // Fullscreen label
        var fullscreenLabel = new UILabel("Fullscreen:", Vector2.Zero)
        {
            TextColor = Color.White
        };
        _videoPanel.AddChild(fullscreenLabel);

        // Fullscreen button
        _fullscreenButton = new UIButton(_settings.Fullscreen ? "On" : "Off", Vector2.Zero, new Vector2(150, 40));
        _fullscreenButton.OnClick += OnFullscreenClicked;
        _videoPanel.AddChild(_fullscreenButton);
    }

    /// <summary>
    /// Switch to the specified category panel
    /// </summary>
    private void SwitchToCategory(int index)
    {
        _settingsContainer.ClearChildren();

        var panel = index switch
        {
            0 => _audioPanel,
            1 => _videoPanel,
            _ => _audioPanel
        };

        panel.Size = new Vector2(_settingsWidth, _settingsContainer.Size.Y);
        _settingsContainer.AddChild(panel);
    }

    /// <summary>
    /// Handle category change
    /// </summary>
    private void OnCategoryChanged(UIListBox sender, int index)
    {
        SwitchToCategory(index);
    }

    /// <summary>
    /// Cycle through resolutions
    /// </summary>
    private void OnResolutionClicked(UIButton sender)
    {
        _selectedResolutionIndex = (_selectedResolutionIndex + 1) % _resolutions.Length;
        var res = _resolutions[_selectedResolutionIndex];
        _resolutionButton.Text = $"{res.Width} x {res.Height}";
        _settings.ResolutionWidth = res.Width;
        _settings.ResolutionHeight = res.Height;
    }

    /// <summary>
    /// Toggle fullscreen
    /// </summary>
    private void OnFullscreenClicked(UIButton sender)
    {
        _settings.Fullscreen = !_settings.Fullscreen;
        _fullscreenButton.Text = _settings.Fullscreen ? "On" : "Off";
    }

    /// <summary>
    /// Apply settings and close
    /// </summary>
    private void OnApplyClicked(UIButton sender)
    {
        // Apply audio settings
        _settings.ApplyAudioSettings();

        // Save settings
        _settings.Save();

        // Close menu
        _stateManager.PopState();
    }

    /// <summary>
    /// Cancel and close without saving
    /// </summary>
    private void OnCancelClicked(UIButton sender)
    {
        _stateManager.PopState();
    }
}
