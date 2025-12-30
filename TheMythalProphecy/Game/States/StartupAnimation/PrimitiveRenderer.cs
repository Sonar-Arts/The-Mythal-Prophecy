using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Provides primitive shape drawing using a 1x1 pixel texture
/// </summary>
public class PrimitiveRenderer : IDisposable
{
    private readonly Texture2D _pixel;

    public PrimitiveRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Dispose()
    {
        _pixel?.Dispose();
    }

    public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_pixel, rect, color);
    }

    public void DrawFilledCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        int steps = Math.Max(8, (int)(radius / 2));
        for (int i = 0; i < steps; i++)
        {
            float y = -radius + (2 * radius * i / steps);
            float halfWidth = MathF.Sqrt(radius * radius - y * y);
            var rect = new Rectangle(
                (int)(center.X - halfWidth),
                (int)(center.Y + y),
                (int)(halfWidth * 2),
                (int)(2 * radius / steps) + 1
            );
            spriteBatch.Draw(_pixel, rect, color);
        }
    }

    public void DrawFilledEllipse(SpriteBatch spriteBatch, Vector2 center, float width, float height, Color color)
    {
        int steps = Math.Max(8, (int)(height / 2));
        float halfHeight = height / 2;
        float halfWidth = width / 2;

        for (int i = 0; i < steps; i++)
        {
            float y = -halfHeight + (height * i / steps);
            float t = y / halfHeight;
            float xWidth = halfWidth * MathF.Sqrt(1 - t * t);

            var rect = new Rectangle(
                (int)(center.X - xWidth),
                (int)(center.Y + y),
                (int)(xWidth * 2),
                (int)(height / steps) + 1
            );
            spriteBatch.Draw(_pixel, rect, color);
        }
    }

    public void DrawCircleOutline(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int thickness = 2)
    {
        int segments = Math.Max(16, (int)(radius / 4));
        for (int i = 0; i < segments; i++)
        {
            float angle1 = MathHelper.TwoPi * i / segments;
            float angle2 = MathHelper.TwoPi * (i + 1) / segments;
            var p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
            var p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;
            DrawLine(spriteBatch, p1, p2, color, thickness);
        }
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        Vector2 edge = end - start;
        float angle = MathF.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(_pixel,
            new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
            null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        // Sort points by Y
        if (p1.Y > p2.Y) (p1, p2) = (p2, p1);
        if (p1.Y > p3.Y) (p1, p3) = (p3, p1);
        if (p2.Y > p3.Y) (p2, p3) = (p3, p2);

        // Fill triangle with horizontal lines
        for (float y = p1.Y; y <= p3.Y; y++)
        {
            float x1, x2;

            if (y < p2.Y)
            {
                float t1 = (y - p1.Y) / (p2.Y - p1.Y + 0.001f);
                float t2 = (y - p1.Y) / (p3.Y - p1.Y + 0.001f);
                x1 = MathHelper.Lerp(p1.X, p2.X, t1);
                x2 = MathHelper.Lerp(p1.X, p3.X, t2);
            }
            else
            {
                float t1 = (y - p2.Y) / (p3.Y - p2.Y + 0.001f);
                float t2 = (y - p1.Y) / (p3.Y - p1.Y + 0.001f);
                x1 = MathHelper.Lerp(p2.X, p3.X, t1);
                x2 = MathHelper.Lerp(p1.X, p3.X, t2);
            }

            if (x1 > x2) (x1, x2) = (x2, x1);

            spriteBatch.Draw(_pixel, new Rectangle((int)x1, (int)y, (int)(x2 - x1) + 1, 1), color);
        }
    }

    public void DrawGradientBackground(SpriteBatch spriteBatch, int screenWidth, int screenHeight,
        Color topColor, Color midColor, Color bottomColor, float midPoint = 0.4f)
    {
        int bands = 60;
        int bandHeight = screenHeight / bands + 1;

        for (int i = 0; i < bands; i++)
        {
            float t = (float)i / bands;
            Color color;

            if (t < midPoint)
            {
                color = Color.Lerp(topColor, midColor, t / midPoint);
            }
            else
            {
                color = Color.Lerp(midColor, bottomColor, (t - midPoint) / (1f - midPoint));
            }

            int y = i * bandHeight;
            spriteBatch.Draw(_pixel, new Rectangle(0, y, screenWidth, bandHeight + 1), color);
        }
    }
}
