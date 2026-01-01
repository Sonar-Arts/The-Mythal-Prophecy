using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TheMythalProphecy.Game.States.StartupAnimation.StartupAnimationConfig;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Submarine entity - static centered drawing
/// </summary>
public class Submarine
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    public float CenterX => _screenWidth * 0.5f;
    public float CenterY => _screenHeight * 0.5f;

    public Submarine(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float centerX = CenterX;
        float centerY = CenterY;

        // Hull dimensions (scaled)
        float hullWidth = S(180);
        float hullHeight = S(50);

        // Main hull body
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(centerX, centerY), hullWidth, hullHeight,
            SubmarineColor);

        // Conning tower
        float towerWidth = S(35);
        float towerHeight = S(30);
        var towerRect = new Rectangle(
            (int)(centerX - towerWidth / 2),
            (int)(centerY - hullHeight / 2 - towerHeight + S(5)),
            (int)towerWidth,
            (int)towerHeight
        );
        renderer.DrawRectangle(spriteBatch, towerRect, SubmarineColor);

        // Tower top (rounded)
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(centerX, centerY - hullHeight / 2 - towerHeight + S(5)),
            towerWidth, S(15), SubmarineColor);

        // Propeller area
        float propX = centerX + hullWidth / 2 - S(10);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(propX, centerY), S(20), S(35),
            SubmarineColor);

        // Periscope
        var periscopeRect = new Rectangle(
            (int)(centerX + S(5)),
            (int)(centerY - hullHeight / 2 - towerHeight - S(15)),
            SiMin1(3),
            Si(20)
        );
        renderer.DrawRectangle(spriteBatch, periscopeRect, SubmarineColor);

        // Highlight on hull
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(centerX - S(20), centerY - S(10)),
            S(100), S(15), SubmarineHighlight * 0.3f);
    }
}
