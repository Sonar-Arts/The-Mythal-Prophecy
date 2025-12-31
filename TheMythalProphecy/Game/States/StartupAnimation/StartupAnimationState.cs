using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Systems.Animation;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Startup animation state - Sonar Arts logo animation
/// Submarine centered with green sonar, flashes to airship with purple sonar
/// Sonar rings persist and transition color during flash
/// </summary>
public class StartupAnimationState : IGameState
{
    private enum AnimationPhase
    {
        SubmarineWithSonar,
        FlashTransition,
        AirshipWithSonar,
        TextReveal,
        Hold,
        FadeToBlack,
        Complete
    }

    // Dependencies
    private readonly ContentManager _content;
    private readonly GameStateManager _stateManager;

    // Rendering
    private PrimitiveRenderer _renderer;
    private BackgroundRenderer _backgroundRenderer;
    private Texture2D _retroLogoTexture;
    private Texture2D _fantasyLogoTexture;
    private BlueVortex _blueVortex;
    private Effect _logoGlowEffect;

    // Entities
    private Submarine _submarine;
    private Airship _airship;
    private SonarSystem _sonarSystem;
    private readonly List<Bubble> _bubbles = new();
    private readonly List<Cloud> _clouds = new();
    private readonly List<Bird> _birds = new();

    // Animation state
    private AnimationPhase _phase;
    private float _phaseElapsed;
    private float _totalElapsed;
    private bool _isComplete;
    private KeyboardState _previousKeyState;

    // Tween engine
    private TweenEngine _tweenEngine;
    private FloatTween _flashOpacity;
    private FloatTween _colorTransition;
    private FloatTween _backgroundTransition;
    private FloatTween _fadeToBlackOpacity;

    // Logo reveal
    private float _logoRevealAmount;

    // Screen dimensions
    private int _screenWidth;
    private int _screenHeight;

    public StartupAnimationState(ContentManager content, GameStateManager stateManager)
    {
        _content = content;
        _stateManager = stateManager;
        _tweenEngine = new TweenEngine();
    }

    public void Enter()
    {
        _screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        _screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        // Initialize renderers
        _renderer = new PrimitiveRenderer(GameServices.GraphicsDevice);
        _backgroundRenderer = new BackgroundRenderer(_screenWidth, _screenHeight);
        _blueVortex = new BlueVortex(_screenWidth, _screenHeight);

        // Load logo textures
        var retroLogoPath = Path.Combine("Game", "Art", "Logos", "SonarArts_Retro.png");
        using (var stream = File.OpenRead(retroLogoPath))
        {
            _retroLogoTexture = Texture2D.FromStream(GameServices.GraphicsDevice, stream);
        }

        var fantasyLogoPath = Path.Combine("Game", "Art", "Logos", "SonarArts_Fantasy.png");
        using (var stream = File.OpenRead(fantasyLogoPath))
        {
            _fantasyLogoTexture = Texture2D.FromStream(GameServices.GraphicsDevice, stream);
        }

        // Load glow shader
        _logoGlowEffect = _content.Load<Effect>("Effects/LogoGlow");

        // Initialize entities (static positions)
        _submarine = new Submarine(_screenWidth, _screenHeight);
        _airship = new Airship(_screenWidth, _screenHeight);
        _sonarSystem = new SonarSystem(_screenWidth, _screenHeight);

        // Reset state
        _phase = AnimationPhase.SubmarineWithSonar;
        _phaseElapsed = 0f;
        _totalElapsed = 0f;
        _isComplete = false;
        _previousKeyState = Keyboard.GetState();

        // Clear
        _tweenEngine.Clear();
        _bubbles.Clear();
        _logoRevealAmount = 0f;

        // Spawn initial bubbles
        for (int i = 0; i < 15; i++)
        {
            SpawnBubble(true);
        }
    }

    public void Exit()
    {
        _renderer?.Dispose();
        _retroLogoTexture?.Dispose();
        _fantasyLogoTexture?.Dispose();
        _tweenEngine.Clear();
    }

    public void Pause() { }

    public void Resume()
    {
        _previousKeyState = Keyboard.GetState();
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _totalElapsed += deltaTime;
        _phaseElapsed += deltaTime;

        // Handle skip input
        HandleInput();

        if (_isComplete)
        {
            _stateManager.ChangeState(new TitleScreenScreen(_content, _stateManager));
            return;
        }

        // Update tweens
        _tweenEngine.Update(gameTime);

        // Always update sonar system
        _sonarSystem.Update(deltaTime);

        // Update phase-specific logic
        switch (_phase)
        {
            case AnimationPhase.SubmarineWithSonar:
                // Spawn bubbles
                if (Random.Shared.NextSingle() < 0.15f)
                {
                    SpawnBubble(false);
                }
                if (_phaseElapsed >= StartupAnimationConfig.SubmarineDuration)
                    StartFlashPhase();
                break;

            case AnimationPhase.FlashTransition:
                // Hold flash at full opacity - exit starlight flash handles the transition
                // Update sonar color transition
                if (_colorTransition != null)
                {
                    _sonarSystem.SetColorTransition(_colorTransition.Current);
                }

                // Spawn clouds during flash so they're ready
                UpdateCloudsAndBirds(deltaTime);

                if (_phaseElapsed >= StartupAnimationConfig.FlashDuration)
                    StartAirshipPhase();
                break;

            case AnimationPhase.AirshipWithSonar:
                _airship.Update(_totalElapsed);
                UpdateCloudsAndBirds(deltaTime);
                if (_phaseElapsed >= StartupAnimationConfig.AirshipDuration)
                    StartTextRevealPhase();
                break;

            case AnimationPhase.TextReveal:
                _airship.Update(_totalElapsed);
                UpdateCloudsAndBirds(deltaTime);
                if (_phaseElapsed >= StartupAnimationConfig.TextRevealDuration)
                    StartHoldPhase();
                break;

            case AnimationPhase.Hold:
                _airship.Update(_totalElapsed);
                UpdateCloudsAndBirds(deltaTime);
                if (_phaseElapsed >= StartupAnimationConfig.HoldDuration)
                {
                    StartFadeToBlackPhase();
                }
                break;

            case AnimationPhase.FadeToBlack:
                _airship.Update(_totalElapsed);
                UpdateCloudsAndBirds(deltaTime);
                if (_phaseElapsed >= StartupAnimationConfig.FadeOutDuration)
                {
                    _phase = AnimationPhase.Complete;
                    _isComplete = true;
                }
                break;
        }

        // Update bubbles
        UpdateBubbles(deltaTime);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // End MythalGame's batch to use custom rendering
        spriteBatch.End();

        spriteBatch.Begin(blendState: BlendState.AlphaBlend);

        // Determine if we're in ocean or sky phase
        bool isOceanPhase = _phase == AnimationPhase.SubmarineWithSonar ||
                           (_phase == AnimationPhase.FlashTransition && _phaseElapsed < StartupAnimationConfig.FlashDuration * 0.3f);

        bool isSkyPhase = _phase == AnimationPhase.AirshipWithSonar ||
                         _phase == AnimationPhase.TextReveal ||
                         _phase == AnimationPhase.Hold ||
                         _phase == AnimationPhase.FadeToBlack;

        // Draw sky elements during late flash transition so they're loaded behind the flash
        bool shouldDrawSkyElements = isSkyPhase ||
                                     (_phase == AnimationPhase.FlashTransition && !isOceanPhase);

        // Draw background
        if (isOceanPhase)
        {
            _backgroundRenderer.DrawOceanBackground(spriteBatch, _renderer, _totalElapsed);
        }
        else if (_phase == AnimationPhase.FlashTransition)
        {
            float t = _backgroundTransition?.Current ?? 0f;
            _backgroundRenderer.DrawSkyBackground(spriteBatch, _renderer, t, _totalElapsed);
        }
        else
        {
            _backgroundRenderer.DrawSkyBackground(spriteBatch, _renderer, 1f, _totalElapsed);
        }

        // Draw logo text behind vehicles (revealed by sonar in ocean, visible in sky)
        if (isOceanPhase)
        {
            DrawRetroLogo(spriteBatch);
        }
        else if (shouldDrawSkyElements)
        {
            DrawFantasyLogo(spriteBatch);
        }

        // Draw sonar rings (always visible, color transitions)
        _sonarSystem.Draw(spriteBatch, _renderer);

        // Draw bubbles (ocean phase only)
        if (isOceanPhase)
        {
            DrawBubbles(spriteBatch);
        }

        // Draw clouds behind airship
        if (shouldDrawSkyElements)
        {
            DrawClouds(spriteBatch);
        }

        // Draw entity based on phase
        if (isOceanPhase)
        {
            _submarine.Draw(spriteBatch, _renderer);
        }
        else if (isSkyPhase || _phase == AnimationPhase.FlashTransition)
        {
            // Draw airship during flash transition so it's visible when white fades out
            _airship.Draw(spriteBatch, _renderer);
        }

        // Draw birds in front of airship
        if (shouldDrawSkyElements)
        {
            DrawBirds(spriteBatch);
        }

        // Draw blue vortex flash overlay
        if (_phase == AnimationPhase.FlashTransition && _flashOpacity != null)
        {
            float opacity = _flashOpacity.Current;
            _blueVortex.Draw(spriteBatch, _renderer, _phaseElapsed, opacity, StartupAnimationConfig.FlashDuration);
        }

        // Draw fade to black overlay
        if (_phase == AnimationPhase.FadeToBlack && _fadeToBlackOpacity != null)
        {
            float opacity = _fadeToBlackOpacity.Current;
            _renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * opacity);
        }

        // Keep screen black during Complete phase to prevent flash
        if (_phase == AnimationPhase.Complete)
        {
            _renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black);
        }

        spriteBatch.End();

        // Resume MythalGame's batch
        spriteBatch.Begin();
    }

    private void HandleInput()
    {
        var keyState = Keyboard.GetState();

        bool skipPressed =
            (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            (keyState.IsKeyDown(Keys.Space) && !_previousKeyState.IsKeyDown(Keys.Space)) ||
            (keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter));

        if (skipPressed)
        {
            _isComplete = true;
        }

        _previousKeyState = keyState;
    }

    #region Phase Transitions

    private void StartFlashPhase()
    {
        _phase = AnimationPhase.FlashTransition;
        _phaseElapsed = 0f;
        _tweenEngine.Clear();

        // Quick but smooth fade in, then hold at full opacity
        _flashOpacity = _tweenEngine.TweenFloat(0f, 1f, 0.25f, EasingType.EaseOutQuad);

        // Transition sonar color from green to purple
        _colorTransition = _tweenEngine.TweenFloat(0f, 1f, 0.5f, EasingType.EaseInOutQuad);

        // Background transition
        _backgroundTransition = _tweenEngine.TweenFloat(0f, 1f, 0.5f, EasingType.EaseInOutQuad);

        // Pre-spawn clouds so they're visible immediately after flash
        _clouds.Clear();
        for (int i = 0; i < 8; i++)
        {
            _clouds.Add(new Cloud(_screenWidth, _screenHeight, randomX: true));
        }

        // Pre-spawn birds
        _birds.Clear();
        for (int i = 0; i < 3; i++)
        {
            _birds.Add(new Bird(_screenWidth, _screenHeight, randomX: true));
        }
    }

    private void StartAirshipPhase()
    {
        _phase = AnimationPhase.AirshipWithSonar;
        _phaseElapsed = 0f;
        _bubbles.Clear();

        // Ensure sonar is fully purple
        _sonarSystem.SetColorTransition(1f);

        // Clouds and birds already spawned in StartFlashPhase
    }

    private void StartTextRevealPhase()
    {
        _phase = AnimationPhase.TextReveal;
        _phaseElapsed = 0f;
    }

    private void StartHoldPhase()
    {
        _phase = AnimationPhase.Hold;
        _phaseElapsed = 0f;
    }

    private void StartFadeToBlackPhase()
    {
        _phase = AnimationPhase.FadeToBlack;
        _phaseElapsed = 0f;

        _fadeToBlackOpacity = _tweenEngine.TweenFloat(0f, 1f, StartupAnimationConfig.FadeOutDuration, EasingType.EaseInQuad);
    }

    #endregion

    #region Bubble Management

    private void UpdateBubbles(float deltaTime)
    {
        for (int i = _bubbles.Count - 1; i >= 0; i--)
        {
            _bubbles[i].Update(deltaTime, _totalElapsed);
            if (_bubbles[i].IsExpired)
            {
                _bubbles.RemoveAt(i);
            }
        }
    }

    private void SpawnBubble(bool randomY)
    {
        float centerX = _screenWidth * 0.5f;
        float centerY = _screenHeight * 0.5f;

        float x = centerX + (Random.Shared.NextSingle() - 0.5f) * 120;
        float y = randomY ? Random.Shared.NextSingle() * _screenHeight : centerY - 20;

        _bubbles.Add(new Bubble(x, y));
    }

    private void DrawBubbles(SpriteBatch spriteBatch)
    {
        foreach (var bubble in _bubbles)
        {
            bubble.Draw(spriteBatch, _renderer);
        }
    }

    #endregion

    #region Cloud and Bird Management

    private void UpdateCloudsAndBirds(float deltaTime)
    {
        // Update clouds
        for (int i = _clouds.Count - 1; i >= 0; i--)
        {
            _clouds[i].Update(deltaTime);
            if (_clouds[i].IsExpired)
            {
                _clouds.RemoveAt(i);
            }
        }

        // Spawn new clouds
        if (Random.Shared.NextSingle() < 0.02f && _clouds.Count < 12)
        {
            _clouds.Add(new Cloud(_screenWidth, _screenHeight, randomX: false));
        }

        // Update birds
        for (int i = _birds.Count - 1; i >= 0; i--)
        {
            _birds[i].Update(deltaTime);
            if (_birds[i].IsExpired)
            {
                _birds.RemoveAt(i);
            }
        }

        // Spawn new birds
        if (Random.Shared.NextSingle() < 0.01f && _birds.Count < 6)
        {
            _birds.Add(new Bird(_screenWidth, _screenHeight, randomX: false));
        }
    }

    private void DrawClouds(SpriteBatch spriteBatch)
    {
        foreach (var cloud in _clouds)
        {
            cloud.Draw(spriteBatch, _renderer);
        }
    }

    private void DrawBirds(SpriteBatch spriteBatch)
    {
        foreach (var bird in _birds)
        {
            bird.Draw(spriteBatch, _renderer);
        }
    }

    #endregion

    #region Logo Drawing

    private void DrawRetroLogo(SpriteBatch spriteBatch)
    {
        // Track reveal progress (only increases, never decreases)
        float currentRadius = _sonarSystem.GetLargestRadius();
        float targetReveal = MathHelper.Clamp(currentRadius / 150f, 0f, 1f);
        if (targetReveal > _logoRevealAmount)
            _logoRevealAmount = targetReveal;

        LogoRenderer.DrawRetroLogo(spriteBatch, _retroLogoTexture, _screenWidth, _screenHeight, _logoRevealAmount);
    }

    private void DrawFantasyLogo(SpriteBatch spriteBatch)
    {
        LogoRenderer.DrawFantasyLogo(spriteBatch, _fantasyLogoTexture, _logoGlowEffect, _screenWidth, _screenHeight);
    }

    #endregion
}
