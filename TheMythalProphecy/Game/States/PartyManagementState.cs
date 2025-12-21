using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Party management state - reorder party members and swap with reserves
/// </summary>
public class PartyManagementState : IGameState
{
    private readonly GameStateManager _stateManager;
    private UIWindow _window;
    private UILabel _activePartyTitle;
    private UIListBox _activePartyList;
    private UILabel _reservePartyTitle;
    private UIListBox _reservePartyList;
    private UIButton _swapButton;
    private UIButton _moveToReserveButton;
    private UIButton _moveToActiveButton;
    private UILabel _instructionsLabel;
    private KeyboardState _previousKeyState;

    private int _focusedList = 0; // 0=active, 1=reserve

    public PartyManagementState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void Enter()
    {
        // Create window (700x600)
        int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
        int screenHeight = GameServices.GraphicsDevice.Viewport.Height;

        Vector2 windowSize = new Vector2(700, 600);
        Vector2 windowPos = new Vector2(
            (screenWidth - windowSize.X) / 2,
            (screenHeight - windowSize.Y) / 2
        );

        _window = new UIWindow(windowPos, windowSize, "Party Management")
        {
            IsModal = true,
            ShowCloseButton = false
        };

        // Active party section
        _activePartyTitle = new UILabel("Active Party (Max 4)", new Vector2(10, 10));
        _activePartyTitle.TextColor = Color.Gold;
        _window.ContentPanel.AddChild(_activePartyTitle);

        _activePartyList = new UIListBox(new Vector2(10, 40), new Vector2(660, 200))
        {
            ItemHeight = 45
        };
        _activePartyList.OnSelectionChanged += OnActivePartySelectionChanged;
        _window.ContentPanel.AddChild(_activePartyList);

        // Reserve party section
        _reservePartyTitle = new UILabel("Reserve Party", new Vector2(10, 250));
        _reservePartyTitle.TextColor = Color.Cyan;
        _window.ContentPanel.AddChild(_reservePartyTitle);

        _reservePartyList = new UIListBox(new Vector2(10, 280), new Vector2(660, 150))
        {
            ItemHeight = 40
        };
        _reservePartyList.OnSelectionChanged += OnReservePartySelectionChanged;
        _window.ContentPanel.AddChild(_reservePartyList);

        // Buttons
        _swapButton = new UIButton("Swap with Reserve", new Vector2(10, 440), new Vector2(200, 40));
        _swapButton.OnClick += OnSwapButtonClicked;
        _window.ContentPanel.AddChild(_swapButton);

        _moveToReserveButton = new UIButton("Move to Reserve", new Vector2(220, 440), new Vector2(200, 40));
        _moveToReserveButton.OnClick += OnMoveToReserveClicked;
        _window.ContentPanel.AddChild(_moveToReserveButton);

        _moveToActiveButton = new UIButton("Move to Active", new Vector2(430, 440), new Vector2(200, 40));
        _moveToActiveButton.OnClick += OnMoveToActiveClicked;
        _window.ContentPanel.AddChild(_moveToActiveButton);

        // Instructions
        _instructionsLabel = new UILabel("Tab: Switch Lists | Arrow Keys: Navigate | Enter: Select | Esc: Close", new Vector2(10, 490));
        _instructionsLabel.TextColor = Color.LightGray;
        _window.ContentPanel.AddChild(_instructionsLabel);

        // Register window
        GameServices.UI.AddElement(_window);

        // Give focus to active party list
        _activePartyList.IsFocused = true;

        // Populate lists
        RefreshPartyLists();
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

        // Escape to close
        if ((keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            GameServices.Input.IsCancelPressed())
        {
            _stateManager.PopState();
        }

        // Tab to switch focus between lists
        if (keyState.IsKeyDown(Keys.Tab) && !_previousKeyState.IsKeyDown(Keys.Tab))
        {
            _focusedList = (_focusedList + 1) % 2;
            _activePartyList.IsFocused = (_focusedList == 0);
            _reservePartyList.IsFocused = (_focusedList == 1);
        }

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // UI Manager handles drawing
    }

    /// <summary>
    /// Refresh both party lists
    /// </summary>
    private void RefreshPartyLists()
    {
        var partyManager = GameServices.GameData.Party;

        // Remember selections
        int activeSelection = _activePartyList.SelectedIndex;
        int reserveSelection = _reservePartyList.SelectedIndex;

        // Clear lists
        _activePartyList.ClearItems();
        _reservePartyList.ClearItems();

        // Populate active party
        var activeParty = partyManager.ActiveParty;
        for (int i = 0; i < activeParty.Count; i++)
        {
            var character = activeParty[i];
            var stats = character.GetComponent<StatsComponent>();
            string displayText = $"{i + 1}. {character.Name} - Lv.{stats.Level} | HP:{stats.CurrentHP}/{stats.MaxHP}";
            _activePartyList.AddItem(displayText);
        }

        // Add empty slots
        for (int i = activeParty.Count; i < 4; i++)
        {
            _activePartyList.AddItem($"{i + 1}. [Empty Slot]");
        }

        // Populate reserve party
        var reserveParty = partyManager.ReserveParty;
        foreach (var character in reserveParty)
        {
            var stats = character.GetComponent<StatsComponent>();
            string displayText = $"{character.Name} - Lv.{stats.Level} | HP:{stats.CurrentHP}/{stats.MaxHP}";
            _reservePartyList.AddItem(displayText);
        }

        if (reserveParty.Count == 0)
        {
            _reservePartyList.AddItem("[No reserve members]");
        }

        // Restore selections
        if (activeSelection >= 0 && activeSelection < _activePartyList.Items.Count)
        {
            _activePartyList.SelectedIndex = activeSelection;
        }
        else if (_activePartyList.Items.Count > 0)
        {
            _activePartyList.SelectedIndex = 0;
        }

        if (reserveSelection >= 0 && reserveSelection < _reservePartyList.Items.Count)
        {
            _reservePartyList.SelectedIndex = reserveSelection;
        }
        else if (_reservePartyList.Items.Count > 0 && reserveParty.Count > 0)
        {
            _reservePartyList.SelectedIndex = 0;
        }

        // Update button states
        UpdateButtonStates();
    }

    /// <summary>
    /// Update button enabled states based on selections
    /// </summary>
    private void UpdateButtonStates()
    {
        var partyManager = GameServices.GameData.Party;
        int activeIndex = _activePartyList.SelectedIndex;
        int reserveIndex = _reservePartyList.SelectedIndex;

        bool hasActiveSelection = activeIndex >= 0 && activeIndex < partyManager.ActivePartyCount;
        bool hasReserveSelection = reserveIndex >= 0 && reserveIndex < partyManager.ReservePartyCount;

        _swapButton.Enabled = hasActiveSelection && hasReserveSelection;
        _moveToReserveButton.Enabled = hasActiveSelection;
        _moveToActiveButton.Enabled = hasReserveSelection && partyManager.ActivePartyCount < 4;
    }

    /// <summary>
    /// Handle active party selection change
    /// </summary>
    private void OnActivePartySelectionChanged(UIListBox sender, int index)
    {
        UpdateButtonStates();
    }

    /// <summary>
    /// Handle reserve party selection change
    /// </summary>
    private void OnReservePartySelectionChanged(UIListBox sender, int index)
    {
        UpdateButtonStates();
    }

    /// <summary>
    /// Swap selected active member with selected reserve member
    /// </summary>
    private void OnSwapButtonClicked(UIButton sender)
    {
        var partyManager = GameServices.GameData.Party;
        int activeIndex = _activePartyList.SelectedIndex;
        int reserveIndex = _reservePartyList.SelectedIndex;

        if (activeIndex >= 0 && activeIndex < partyManager.ActivePartyCount &&
            reserveIndex >= 0 && reserveIndex < partyManager.ReservePartyCount)
        {
            partyManager.SwapActiveWithReserve(activeIndex, reserveIndex);
            RefreshPartyLists();
        }
    }

    /// <summary>
    /// Move selected active member to reserves
    /// </summary>
    private void OnMoveToReserveClicked(UIButton sender)
    {
        var partyManager = GameServices.GameData.Party;
        int activeIndex = _activePartyList.SelectedIndex;

        if (activeIndex >= 0 && activeIndex < partyManager.ActivePartyCount)
        {
            partyManager.MoveToReserve(activeIndex);
            RefreshPartyLists();
        }
    }

    /// <summary>
    /// Move selected reserve member to active party
    /// </summary>
    private void OnMoveToActiveClicked(UIButton sender)
    {
        var partyManager = GameServices.GameData.Party;
        int reserveIndex = _reservePartyList.SelectedIndex;

        if (reserveIndex >= 0 && reserveIndex < partyManager.ReservePartyCount &&
            partyManager.ActivePartyCount < 4)
        {
            partyManager.MoveToActive(reserveIndex);
            RefreshPartyLists();
        }
    }
}
