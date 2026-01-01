using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TheMythalProphecy.Game.States.StartupAnimation.StartupAnimationConfig;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Represents a rising bubble particle
/// </summary>
public class Bubble
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Radius { get; }
    public float Speed { get; }
    public float Opacity { get; set; }
    public float Phase { get; }

    public bool IsExpired => Y < -S(20) || Opacity <= 0;

    public Bubble(float x, float y)
    {
        X = x;
        Y = y;
        Radius = S(3) + Random.Shared.NextSingle() * S(8);
        Speed = S(40) + Random.Shared.NextSingle() * S(60);
        Opacity = 0.4f + Random.Shared.NextSingle() * 0.4f;
        Phase = Random.Shared.NextSingle() * MathF.PI * 2;
    }

    public void Update(float deltaTime, float totalElapsed)
    {
        Y -= Speed * deltaTime;
        X += MathF.Sin(totalElapsed * 2f + Phase) * S(0.5f);
        Opacity -= deltaTime * 0.3f;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        Color bubbleColor = BubbleColor * Opacity;
        renderer.DrawCircleOutline(spriteBatch, new Vector2(X, Y), Radius, bubbleColor, SiMin1(1));

        // Highlight sparkle (scaled)
        Color highlight = Color.White * (Opacity * 0.5f);
        float hlX = X - Radius * 0.3f;
        float hlY = Y - Radius * 0.3f;
        renderer.DrawRectangle(spriteBatch, new Rectangle((int)hlX, (int)hlY, SiMin1(2), SiMin1(2)), highlight);
    }
}
