using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Manages game states with a stack-based approach
/// Allows states to be pushed (pausing previous state) or changed (replacing current state)
/// </summary>
public class GameStateManager
{
    private readonly Stack<IGameState> _stateStack;

    public GameStateManager()
    {
        _stateStack = new Stack<IGameState>();
    }

    /// <summary>
    /// Gets the currently active state (top of stack)
    /// </summary>
    public IGameState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : null;

    /// <summary>
    /// Push a new state on top of the current one (pauses the current state)
    /// </summary>
    public void PushState(IGameState state)
    {
        // Pause the current state before pushing
        CurrentState?.Pause();

        _stateStack.Push(state);
        state.Enter();
    }

    /// <summary>
    /// Pop the current state and resume the previous one
    /// </summary>
    public void PopState()
    {
        if (_stateStack.Count > 0)
        {
            var state = _stateStack.Pop();
            state.Exit();

            // Resume the state that's now on top
            CurrentState?.Resume();
        }
    }

    /// <summary>
    /// Replace the current state with a new one
    /// </summary>
    public void ChangeState(IGameState newState)
    {
        if (_stateStack.Count > 0)
        {
            var oldState = _stateStack.Pop();
            oldState.Exit();
        }

        _stateStack.Push(newState);
        newState.Enter();
    }

    /// <summary>
    /// Update the current active state
    /// </summary>
    public void Update(GameTime gameTime)
    {
        CurrentState?.Update(gameTime);
    }

    /// <summary>
    /// Draw the current active state.
    /// If the top state is an overlay, also draws states below it.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_stateStack.Count == 0)
            return;

        // Build list of states to draw (bottom-up for overlays)
        var statesToDraw = new List<IGameState>();
        foreach (var state in _stateStack)
        {
            statesToDraw.Insert(0, state); // Insert at beginning to reverse order
            if (!state.IsOverlay)
                break; // Stop at first non-overlay state
        }

        // Draw from bottom to top
        foreach (var state in statesToDraw)
        {
            state.Draw(spriteBatch, gameTime);
        }
    }
}
