using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

    public bool IsExpired => Y < -20 || Opacity <= 0;

    public Bubble(float x, float y)
    {
        X = x;
        Y = y;
        Radius = 3 + Random.Shared.NextSingle() * 8;
        Speed = 40 + Random.Shared.NextSingle() * 60;
        Opacity = 0.4f + Random.Shared.NextSingle() * 0.4f;
        Phase = Random.Shared.NextSingle() * MathF.PI * 2;
    }

    public void Update(float deltaTime, float totalElapsed)
    {
        Y -= Speed * deltaTime;
        X += MathF.Sin(totalElapsed * 2f + Phase) * 0.5f;
        Opacity -= deltaTime * 0.3f;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        Color bubbleColor = StartupAnimationConfig.BubbleColor * Opacity;
        renderer.DrawCircleOutline(spriteBatch, new Vector2(X, Y), Radius, bubbleColor, 1);

        // Highlight sparkle
        Color highlight = Color.White * (Opacity * 0.5f);
        float hlX = X - Radius * 0.3f;
        float hlY = Y - Radius * 0.3f;
        renderer.DrawRectangle(spriteBatch, new Rectangle((int)hlX, (int)hlY, 2, 2), highlight);
    }
}
