using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// Interface for all game states (title screen, world map, battle, menu, etc.)
/// </summary>
public interface IGameState
{
    /// <summary>
    /// Called when the state becomes active
    /// </summary>
    void Enter();

    /// <summary>
    /// Called when the state becomes inactive
    /// </summary>
    void Exit();

    /// <summary>
    /// Called when another state is pushed on top of this one
    /// </summary>
    void Pause();

    /// <summary>
    /// Called when the state above is popped and this state becomes active again
    /// </summary>
    void Resume();

    /// <summary>
    /// Update the state logic
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draw the state
    /// </summary>
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
