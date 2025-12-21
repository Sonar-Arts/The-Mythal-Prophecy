using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.UI;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Pause menu state - provides access to all game menus
/// Displayed as a modal overlay when the player pauses the game
/// </summary>
public class PauseMenuState : IGameState
{
    private readonly GameStateManager _stateManager;
    private UIWindow _menuWindow;
    private UIListBox _menuOptions;
    private KeyboardState _previousKeyState;

    // Menu option indices
    private const int MENU_ITEMS = 0;
    private const int MENU_EQUIPMENT = 1;
    private const int MENU_STATUS = 2;
    private const int MENU_PARTY = 3;
    private const int MENU_SAVE = 4;
    private const int MENU_OPTIONS = 5;
    private const int MENU_RESUME = 6;
    private const int MENU_QUIT = 7;

    public PauseMenuState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Create centered modal window (600x400)
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        Vector2 windowSize = new Vector2(600, 500);
        Vector2 windowPos = new Vector2(
            (screenWidth - windowSize.X) / 2,
            (screenHeight - windowSize.Y) / 2
        );

        _menuWindow = new UIWindow(windowPos, windowSize, "Pause Menu")
        {
            IsModal = true,
            ShowCloseButton = false
        };

        // Create menu options list
        Vector2 listSize = new Vector2(windowSize.X - 40, windowSize.Y - 100);
        _menuOptions = new UIListBox(new Vector2(20, 20), listSize)
        {
            ItemHeight = 45
        };

        // Add menu options
        _menuOptions.AddItem("Items");
        _menuOptions.AddItem("Equipment");
        _menuOptions.AddItem("Status");
        _menuOptions.AddItem("Party");
        _menuOptions.AddItem("Save");
        _menuOptions.AddItem("Options");
        _menuOptions.AddItem("Resume");
        _menuOptions.AddItem("Quit to Title");

        // Set initial selection
        _menuOptions.SelectedIndex = 0;

        // Handle item activation
        _menuOptions.OnItemActivated += OnMenuItemActivated;

        // Add list to window
        _menuWindow.ContentPanel.AddChild(_menuOptions);

        // Register window with UI manager
        GameServices.UI.AddElement(_menuWindow);

        // Give focus to the listbox
        _menuOptions.IsFocused = true;

        // Initialize previous key state to current state to prevent immediate ESC detection
        _previousKeyState = Keyboard.GetState();
    }

    public void Exit()
    {
        // Remove window from UI manager
        GameServices.UI.RemoveElement(_menuWindow);
    }

    public void Pause()
    {
        // Hide window when another state is pushed on top
        if (_menuWindow != null)
            _menuWindow.Visible = false;
    }

    public void Resume()
    {
        // Show window when this state becomes active again
        if (_menuWindow != null)
            _menuWindow.Visible = true;

        // Reset keyboard state to prevent immediate re-triggering
        _previousKeyState = Keyboard.GetState();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState keyState = Keyboard.GetState();

        // Check for escape/cancel to resume game
        if ((keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            GameServices.Input.IsCancelPressed())
        {
            // Resume game (pop this state)
            _stateManager.PopState();
        }

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // The UIManager will handle drawing the window
        // We just need to ensure the background states are still visible
        // Draw a semi-transparent overlay is handled by the UIWindow's IsModal property
    }

    /// <summary>
    /// Handle menu item activation (Enter/Space/Click)
    /// </summary>
    private void OnMenuItemActivated(UIListBox sender, int index)
    {
        switch (index)
        {
            case MENU_ITEMS:
                _stateManager.PushState(new InventoryState(_stateManager));
                break;

            case MENU_EQUIPMENT:
                _stateManager.PushState(new EquipmentState(_stateManager));
                break;

            case MENU_STATUS:
                _stateManager.PushState(new CharacterStatusState(_stateManager));
                break;

            case MENU_PARTY:
                _stateManager.PushState(new PartyManagementState(_stateManager));
                break;

            case MENU_SAVE:
                // TODO: Push SaveMenuState when implemented
                // _stateManager.PushState(new SaveMenuState(_stateManager));
                break;

            case MENU_OPTIONS:
                _stateManager.PushState(new OptionsMenuState(_stateManager));
                break;

            case MENU_RESUME:
                // Pop this state to resume game
                _stateManager.PopState();
                break;

            case MENU_QUIT:
                // Return to title screen (clears all states and changes to title)
                while (_stateManager.CurrentState != null)
                {
                    _stateManager.PopState();
                }
                _stateManager.ChangeState(new TitleScreenState(GameServices.Content, _stateManager));
                break;
        }
    }
}
