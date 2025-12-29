using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Data;
using TheMythalProphecy.Game.Data.Mock;
using TheMythalProphecy.Game.States;

namespace TheMythalProphecy.Game.Core;

public class MythalGame : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameStateManager _stateManager;

    public MythalGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window title
        Window.Title = "The Mythal Prophecy";

        // Set default resolution (1280x720)
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Initialize game services (includes font/theme initialization)
        GameServices.Initialize(Content, GraphicsDevice, _graphics);

        // Load and apply saved settings
        var settings = GameSettings.Load();
        settings.ApplyVideoSettings();
        settings.ApplyAudioSettings();

        // Initialize mock data for development
        MockDataInitializer.Initialize();

        // Initialize state manager
        _stateManager = new GameStateManager();

        // Set initial state to title screen
        var titleState = new TitleScreenState(Content, _stateManager);
        _stateManager.ChangeState(titleState);
    }

    protected override void Update(GameTime gameTime)
    {
        // Only handle gamepad Back button for platform-specific exit
        // Allow states to handle Escape key for pause menus
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        // Update game services (input, UI, etc.)
        GameServices.Update(gameTime);

        // Update current state
        _stateManager?.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin sprite batch once for all drawing
        _spriteBatch.Begin();

        // Draw current state (state should NOT call Begin/End)
        _stateManager?.Draw(_spriteBatch, gameTime);

        // Draw UI elements on top
        GameServices.UI.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
