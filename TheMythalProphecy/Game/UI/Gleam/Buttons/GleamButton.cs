using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Parallelogram-shaped button with cosmic styling and shimmer effect.
/// </summary>
public class GleamButton : GleamElement
{
    private const float SkewFactor = 0.3f;

    public string Text { get; set; }
    public SpriteFont Font { get; set; }

    // Custom colors (null = use theme defaults)
    public Color? NormalColor { get; set; }
    public Color? HoverColor { get; set; }
    public Color? PressedColor { get; set; }
    public Color? DisabledColor { get; set; }
    public Color? BorderColor { get; set; }
    public Color? TextColor { get; set; }

    public int BorderThickness { get; set; } = 2;

    public GleamButton(string text, Vector2 position, Vector2 size)
    {
        Text = text;
        Position = position;
        Size = size;
    }

    public override bool ContainsPoint(Vector2 point)
    {
        // Use parallelogram hit detection (centered within bounds)
        Rectangle bounds = Bounds;
        float skewOffset = bounds.Height * SkewFactor;
        float centerOffset = skewOffset / 2f;
        float relY = point.Y - bounds.Y;
        float t = relY / bounds.Height;

        if (t < 0 || t > 1) return false;

        float leftX = bounds.X + skewOffset * (1 - t) - centerOffset;
        float rightX = bounds.Right + skewOffset * (1 - t) - centerOffset;

        return point.X >= leftX && point.X <= rightX &&
               point.Y >= bounds.Y && point.Y <= bounds.Bottom;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        Rectangle bounds = Bounds;

        // Determine colors
        Color bgNormal = NormalColor ?? theme.DeepPurple;
        Color bgHover = HoverColor ?? theme.MidPurple;
        Color bgPressed = PressedColor ?? theme.DarkPurple;
        Color bgDisabled = DisabledColor ?? theme.MutedPurple;
        Color border = BorderColor ?? theme.Gold;
        Color text = TextColor ?? theme.TextPrimary;

        // Get current background color
        Color bgColor;
        if (!Enabled)
        {
            bgColor = bgDisabled;
        }
        else
        {
            bgColor = GetStateColor(bgNormal, bgHover, bgPressed);
        }

        // Draw parallelogram fill
        renderer.DrawParallelogram(spriteBatch, bounds, bgColor, SkewFactor, Alpha);

        // Draw border
        if (BorderThickness > 0)
        {
            Color borderDraw = IsFocused ? theme.GoldBright : border;
            renderer.DrawParallelogramBorder(spriteBatch, bounds, borderDraw, SkewFactor, BorderThickness, Alpha);
        }

        // Draw text centered
        var font = Font ?? theme.MenuFont ?? theme.DefaultFont;
        if (font != null && !string.IsNullOrEmpty(Text))
        {
            Color textDraw = Enabled ? text : theme.TextDisabled;
            renderer.DrawTextCentered(spriteBatch, font, Text, bounds, textDraw, true, Alpha);
        }
    }
}
