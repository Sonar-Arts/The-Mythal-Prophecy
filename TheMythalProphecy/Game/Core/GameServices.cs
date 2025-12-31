using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheMythalProphecy.Game.Systems.Input;
using TheMythalProphecy.Game.Systems.Events;
using TheMythalProphecy.Game.Systems.Animation;
using TheMythalProphecy.Game.Systems.Rendering;
using TheMythalProphecy.Game.Systems.Audio;
using TheMythalProphecy.Game.UI;
using TheMythalProphecy.Game.Data;

namespace TheMythalProphecy.Game.Core;

/// <summary>
/// Service locator for global game systems
/// Provides centralized access to managers and services
/// </summary>
public static class GameServices
{
    public static InputManager Input { get; private set; }
    public static ContentManager Content { get; private set; }
    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static GraphicsDeviceManager Graphics { get; private set; }
    public static UIManager UI { get; private set; }
    public static EventManager Events { get; private set; }
    public static AnimationManager Animations { get; private set; }
    public static RenderManager Rendering { get; private set; }
    public static AudioManager Audio { get; private set; }
    public static GameData GameData { get; private set; }

    /// <summary>
    /// Initialize the game services
    /// </summary>
    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics = null)
    {
        Content = content;
        GraphicsDevice = graphicsDevice;
        Graphics = graphics;
        Input = new InputManager();
        UI = new UIManager(graphicsDevice);
        Events = new EventManager();
        Animations = new AnimationManager(content);
        Rendering = new RenderManager(graphicsDevice, graphicsDevice.Viewport);
        Audio = new AudioManager();
        GameData = new GameData();

        // CRITICAL: Initialize theme with default font immediately
        try
        {
            var defaultFont = content.Load<SpriteFont>("Fonts/Default");
            UI.Theme.Initialize(defaultFont);
            Console.WriteLine($"[GameServices] Theme initialized with font: {UI.Theme.DefaultFont != null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServices] ERROR loading default font: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update all services that require per-frame updates
    /// </summary>
    public static void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        Input?.Update();
        UI?.Update(gameTime);
        Rendering?.Update(gameTime);
        Audio?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }
}
