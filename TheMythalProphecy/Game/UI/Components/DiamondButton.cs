using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheMythalProphecy.Game.Core;

namespace TheMythalProphecy.Game.UI.Components;

/// <summary>
/// A button rendered as a parallelogram (slanted rectangle)
/// </summary>
public class DiamondButton : UIButton
{
    // Skew amount as fraction of height (0.3 = 30% lean)
    private const float SkewFactor = 0.3f;

    public DiamondButton() : base()
    {
    }

    public DiamondButton(string text, Vector2 position, Vector2 size) : base(text, position, size)
    {
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, UITheme theme)
    {
        Rectangle bounds = Bounds;
        Texture2D pixel = GameServices.UI?.PixelTexture;
        if (pixel == null) return;

        float skewOffset = bounds.Height * SkewFactor;

        // Parallelogram corners (top-left skewed right, bottom-left at base)
        var topLeft = new Vector2(bounds.X + skewOffset, bounds.Y);
        var topRight = new Vector2(bounds.Right + skewOffset, bounds.Y);
        var bottomRight = new Vector2(bounds.Right, bounds.Bottom);
        var bottomLeft = new Vector2(bounds.X, bounds.Bottom);

        Color backgroundColor = GetStateColor();

        // Draw parallelogram as horizontal scan lines
        for (int y = 0; y < bounds.Height; y++)
        {
            float t = y / (float)bounds.Height;
            float leftX = MathHelper.Lerp(topLeft.X, bottomLeft.X, t);
            float rightX = MathHelper.Lerp(topRight.X, bottomRight.X, t);
            int posY = bounds.Y + y;

            spriteBatch.Draw(
                pixel,
                new Rectangle((int)leftX, posY, (int)(rightX - leftX), 1),
                backgroundColor * Alpha
            );
        }

        // Draw parallelogram border
        if (BorderThickness > 0)
        {
            Color borderColor = IsFocused ? theme.HighlightColor : BorderColor;
            DrawLine(spriteBatch, pixel, topLeft, topRight, BorderThickness, borderColor);
            DrawLine(spriteBatch, pixel, topRight, bottomRight, BorderThickness, borderColor);
            DrawLine(spriteBatch, pixel, bottomRight, bottomLeft, BorderThickness, borderColor);
            DrawLine(spriteBatch, pixel, bottomLeft, topLeft, BorderThickness, borderColor);
        }
    }

    private Color GetStateColor()
    {
        if (!Enabled) return DisabledColor;
        if (IsHovered) return HoverColor;
        return NormalColor;
    }

    private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, int thickness, Color color)
    {
        var edge = end - start;
        float length = edge.Length();
        float angle = MathF.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            pixel,
            start,
            null,
            color * Alpha,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f
        );
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseClicked)
    {
        if (!Enabled || !Visible) return false;

        Rectangle bounds = Bounds;
        float skewOffset = bounds.Height * SkewFactor;

        // Check if point is inside parallelogram
        // Transform point to check against skewed shape
        float relY = mousePosition.Y - bounds.Y;
        float t = relY / bounds.Height;

        if (t < 0 || t > 1)
        {
            IsHovered = false;
            return false;
        }

        // Calculate left and right edges at this Y position
        float leftX = bounds.X + skewOffset * (1 - t);
        float rightX = bounds.Right + skewOffset * (1 - t);

        bool isInside = mousePosition.X >= leftX && mousePosition.X <= rightX &&
                        mousePosition.Y >= bounds.Y && mousePosition.Y <= bounds.Bottom;

        IsHovered = isInside;

        if (isInside && mouseClicked)
        {
            InvokeClick();
            return true;
        }

        return false;
    }
}
