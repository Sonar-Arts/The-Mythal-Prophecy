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
        // Draw every single row to eliminate scan lines
        int iRadius = (int)MathF.Ceiling(radius);
        float radiusSq = radius * radius;

        for (int y = -iRadius; y <= iRadius; y++)
        {
            float halfWidth = MathF.Sqrt(radiusSq - y * y);
            if (halfWidth < 0.5f) continue;

            int left = (int)(center.X - halfWidth);
            int right = (int)(center.X + halfWidth);
            int width = right - left + 1;

            if (width > 0)
            {
                spriteBatch.Draw(_pixel, new Rectangle(left, (int)center.Y + y, width, 1), color);
            }
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

        // Use integer Y coordinates to prevent scanline gaps
        int yStart = (int)MathF.Floor(p1.Y);
        int yMid = (int)MathF.Ceiling(p2.Y);
        int yEnd = (int)MathF.Ceiling(p3.Y);

        // Fill triangle with horizontal lines (height 2 for overlap to prevent gaps)
        for (int y = yStart; y <= yEnd; y++)
        {
            float x1, x2;

            if (y < yMid)
            {
                float t1 = (y - p1.Y) / (p2.Y - p1.Y + 0.001f);
                float t2 = (y - p1.Y) / (p3.Y - p1.Y + 0.001f);
                x1 = MathHelper.Lerp(p1.X, p2.X, MathHelper.Clamp(t1, 0f, 1f));
                x2 = MathHelper.Lerp(p1.X, p3.X, MathHelper.Clamp(t2, 0f, 1f));
            }
            else
            {
                float t1 = (y - p2.Y) / (p3.Y - p2.Y + 0.001f);
                float t2 = (y - p1.Y) / (p3.Y - p1.Y + 0.001f);
                x1 = MathHelper.Lerp(p2.X, p3.X, MathHelper.Clamp(t1, 0f, 1f));
                x2 = MathHelper.Lerp(p1.X, p3.X, MathHelper.Clamp(t2, 0f, 1f));
            }

            if (x1 > x2) (x1, x2) = (x2, x1);

            int left = (int)MathF.Floor(x1);
            int right = (int)MathF.Ceiling(x2);
            int width = right - left + 1;

            if (width > 0)
            {
                // Draw with height 2 to ensure overlap and prevent gaps between scanlines
                spriteBatch.Draw(_pixel, new Rectangle(left, y, width, 2), color);
            }
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
