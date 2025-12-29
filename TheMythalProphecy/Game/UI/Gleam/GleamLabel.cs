using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Text label with cosmic styling.
/// Supports text scaling and auto-fit to container.
/// </summary>
public class GleamLabel : GleamElement
{
    public string Text { get; set; }
    public SpriteFont Font { get; set; }
    public Color? TextColor { get; set; }
    public bool ShowShadow { get; set; } = true;
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    /// <summary>
    /// Text scale factor. 1.0 = normal, 0.5 = half size, etc.
    /// </summary>
    public float Scale { get; set; } = 1f;

    /// <summary>
    /// If true, automatically scales text to fit within bounds.
    /// </summary>
    public bool AutoFit { get; set; }

    /// <summary>
    /// Padding when using AutoFit (in pixels).
    /// </summary>
    public float AutoFitPadding { get; set; } = 4f;

    public GleamLabel(string text, Vector2 position)
    {
        Text = text;
        Position = position;
        Size = Vector2.Zero; // Auto-size from text
    }

    public GleamLabel(string text, Vector2 position, Vector2 size)
    {
        Text = text;
        Position = position;
        Size = size;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        if (string.IsNullOrEmpty(Text)) return;

        var theme = renderer.Theme;
        var font = Font ?? theme.DefaultFont;
        if (font == null) return;

        Color color = TextColor ?? theme.TextPrimary;
        Vector2 textSize = font.MeasureString(Text);
        Rectangle bounds = Bounds;

        // If size is zero, auto-size
        if (Size == Vector2.Zero)
        {
            bounds = new Rectangle((int)AbsolutePosition.X, (int)AbsolutePosition.Y, (int)textSize.X, (int)textSize.Y);
        }

        // Calculate scale
        float scale = Scale;
        if (AutoFit && Size != Vector2.Zero)
        {
            float availableWidth = bounds.Width - AutoFitPadding * 2;
            float availableHeight = bounds.Height - AutoFitPadding * 2;

            float scaleX = availableWidth / textSize.X;
            float scaleY = availableHeight / textSize.Y;

            // Use the smaller scale to fit both dimensions
            float autoScale = MathHelper.Min(scaleX, scaleY);

            // Don't scale up beyond 1.0, only down
            scale = MathHelper.Min(Scale, MathHelper.Min(autoScale, 1f));
        }

        Vector2 scaledTextSize = textSize * scale;

        Vector2 position;
        switch (Alignment)
        {
            case TextAlignment.Center:
                position = new Vector2(
                    bounds.X + (bounds.Width - scaledTextSize.X) / 2f,
                    bounds.Y + (bounds.Height - scaledTextSize.Y) / 2f
                );
                break;
            case TextAlignment.Right:
                position = new Vector2(
                    bounds.Right - scaledTextSize.X,
                    bounds.Y + (bounds.Height - scaledTextSize.Y) / 2f
                );
                break;
            default: // Left
                position = new Vector2(
                    bounds.X,
                    bounds.Y + (bounds.Height - scaledTextSize.Y) / 2f
                );
                break;
        }

        // Draw with scale
        if (ShowShadow)
        {
            spriteBatch.DrawString(font, Text, position + new Vector2(1, 1), Color.Black * Alpha * 0.5f,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        spriteBatch.DrawString(font, Text, position, color * Alpha,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        // Labels don't handle input
        return false;
    }
}

public enum TextAlignment
{
    Left,
    Center,
    Right
}
