using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        // Hull dimensions
        float hullWidth = 180;
        float hullHeight = 50;

        // Main hull body
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(centerX, centerY), hullWidth, hullHeight,
            StartupAnimationConfig.SubmarineColor);

        // Conning tower
        float towerWidth = 35;
        float towerHeight = 30;
        var towerRect = new Rectangle(
            (int)(centerX - towerWidth / 2),
            (int)(centerY - hullHeight / 2 - towerHeight + 5),
            (int)towerWidth,
            (int)towerHeight
        );
        renderer.DrawRectangle(spriteBatch, towerRect, StartupAnimationConfig.SubmarineColor);

        // Tower top (rounded)
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(centerX, centerY - hullHeight / 2 - towerHeight + 5),
            towerWidth, 15, StartupAnimationConfig.SubmarineColor);

        // Propeller area
        float propX = centerX + hullWidth / 2 - 10;
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(propX, centerY), 20, 35,
            StartupAnimationConfig.SubmarineColor);

        // Periscope
        var periscopeRect = new Rectangle(
            (int)(centerX + 5),
            (int)(centerY - hullHeight / 2 - towerHeight - 15),
            3,
            20
        );
        renderer.DrawRectangle(spriteBatch, periscopeRect, StartupAnimationConfig.SubmarineColor);

        // Highlight on hull
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(centerX - 20, centerY - 10),
            100, 15, StartupAnimationConfig.SubmarineHighlight * 0.3f);
    }
}
