using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.Entities.Components;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Combat stat display panel with cosmic "constellation band" layout.
/// Groups stats into Physical, Magical, and Fortune categories with gradient-filled parallelogram bars.
/// </summary>
public class GleamStatPanel : GleamElement
{
    // Layout constants
    private const int Padding = 12;
    private const int CategoryLabelHeight = 20;
    private const int StatBarHeight = 28;
    private const int CategorySpacing = 6;
    private const int StatSpacing = 8;
    private const float SkewFactor = 0.15f;

    // Maximum stat value for percentage calculation
    public int MaxStatValue { get; set; } = 100;

    // Stat values
    private int _strength, _defense, _magicPower, _magicDefense, _speed, _luck;

    // Category accent colors - Physical (warm orange-gold)
    private readonly Color _physicalBright = new Color(220, 160, 60);
    private readonly Color _physicalDark = new Color(140, 90, 30);

    // Category accent colors - Magical (cool purple)
    private readonly Color _magicalBright = new Color(160, 100, 220);
    private readonly Color _magicalDark = new Color(90, 50, 140);

    // Category accent colors - Fortune (silver-gold)
    private readonly Color _fortuneBright = new Color(180, 200, 220);
    private readonly Color _fortuneDark = new Color(100, 120, 140);

    // Animation
    private float _shimmerPhase;

    public GleamStatPanel(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    /// <summary>
    /// Set all stats from a StatsComponent.
    /// </summary>
    public void SetStats(StatsComponent stats)
    {
        if (stats == null) return;

        _strength = stats.GetStat(StatType.Strength);
        _defense = stats.GetStat(StatType.Defense);
        _magicPower = stats.GetStat(StatType.MagicPower);
        _magicDefense = stats.GetStat(StatType.MagicDefense);
        _speed = stats.GetStat(StatType.Speed);
        _luck = stats.GetStat(StatType.Luck);
    }

    /// <summary>
    /// Set individual stat values directly.
    /// </summary>
    public void SetStats(int str, int def, int mag, int mdf, int spd, int lck)
    {
        _strength = str;
        _defense = def;
        _magicPower = mag;
        _magicDefense = mdf;
        _speed = spd;
        _luck = lck;
    }

    public override void Update(GameTime gameTime, GleamRenderer renderer)
    {
        base.Update(gameTime, renderer);

        // Animate shimmer effect across bars
        _shimmerPhase += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.8f;
        if (_shimmerPhase > MathF.PI * 2f)
            _shimmerPhase -= MathF.PI * 2f;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        Rectangle bounds = Bounds;

        // Panel background with subtle gradient
        renderer.DrawVerticalGradient(spriteBatch, bounds, theme.DarkPurple, theme.DeepPurple, Alpha * 0.95f);

        // Calculate section layout
        float contentWidth = bounds.Width - Padding * 2;
        float barWidth = (contentWidth - StatSpacing) / 2f;
        float sectionHeight = CategoryLabelHeight + StatBarHeight + CategorySpacing;

        float startY = bounds.Y + Padding;

        // Draw three category sections
        DrawCategorySection(spriteBatch, renderer,
            "PHYSICAL", bounds.X + Padding, startY, contentWidth, barWidth,
            "STR", _strength, "DEF", _defense,
            _physicalBright, _physicalDark);

        startY += sectionHeight;

        DrawCategorySection(spriteBatch, renderer,
            "MAGICAL", bounds.X + Padding, startY, contentWidth, barWidth,
            "MAG", _magicPower, "MDF", _magicDefense,
            _magicalBright, _magicalDark);

        startY += sectionHeight;

        DrawCategorySection(spriteBatch, renderer,
            "FORTUNE", bounds.X + Padding, startY, contentWidth, barWidth,
            "SPD", _speed, "LCK", _luck,
            _fortuneBright, _fortuneDark);

        // Panel border
        renderer.DrawRectBorder(spriteBatch, bounds, theme.Gold, 2, Alpha);

        // Corner accents for cosmic feel
        DrawCornerAccents(spriteBatch, renderer, bounds);
    }

    private void DrawCategorySection(
        SpriteBatch spriteBatch, GleamRenderer renderer,
        string categoryName, float x, float y, float width, float barWidth,
        string stat1Name, int stat1Value, string stat2Name, int stat2Value,
        Color brightColor, Color darkColor)
    {
        var theme = renderer.Theme;
        var font = theme.DefaultFont;

        // Category divider line with label
        if (font != null)
        {
            Vector2 textSize = font.MeasureString(categoryName);
            float textX = x + 4;
            float lineStartX = textX + textSize.X + 8;
            float lineY = y + CategoryLabelHeight / 2f;

            // Draw category label
            renderer.DrawText(spriteBatch, font, categoryName,
                new Vector2(textX, y + 2), theme.TextSecondary, true, Alpha * 0.7f);

            // Draw decorative line extending to the right
            renderer.DrawLine(spriteBatch,
                new Vector2(lineStartX, lineY),
                new Vector2(x + width, lineY),
                1, theme.GoldDim * Alpha * 0.5f);
        }

        // Stat bars below category label
        float barY = y + CategoryLabelHeight;

        // First stat bar (left)
        DrawStatBar(spriteBatch, renderer,
            new Rectangle((int)x, (int)barY, (int)barWidth, StatBarHeight),
            stat1Name, stat1Value, brightColor, darkColor, 0f);

        // Second stat bar (right) - offset shimmer phase for variety
        DrawStatBar(spriteBatch, renderer,
            new Rectangle((int)(x + barWidth + StatSpacing), (int)barY, (int)barWidth, StatBarHeight),
            stat2Name, stat2Value, brightColor, darkColor, MathF.PI);
    }

    private void DrawStatBar(
        SpriteBatch spriteBatch, GleamRenderer renderer,
        Rectangle bounds, string statName, int value,
        Color brightColor, Color darkColor, float shimmerOffset)
    {
        var theme = renderer.Theme;

        // Background (dark parallelogram)
        renderer.DrawParallelogram(spriteBatch, bounds, theme.DarkPurple, SkewFactor, Alpha);

        // Fill based on percentage
        float percentage = MathHelper.Clamp((float)value / MaxStatValue, 0f, 1f);
        int fillWidth = (int)(bounds.Width * percentage);

        if (fillWidth > 4)
        {
            Rectangle fillBounds = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);

            // Draw gradient fill using horizontal lines for the parallelogram shape
            DrawParallelogramGradient(spriteBatch, renderer, fillBounds, darkColor, brightColor, SkewFactor);

            // Inner glow for depth at top of fill
            DrawParallelogramGlow(spriteBatch, renderer, fillBounds, brightColor, SkewFactor);

            // Animated shimmer highlight
            float shimmerPos = (MathF.Sin(_shimmerPhase + shimmerOffset) + 1f) * 0.5f;
            int shimmerX = bounds.X + (int)(fillWidth * shimmerPos * 0.8f);
            int shimmerWidth = Math.Min(16, fillWidth / 3);
            if (shimmerX + shimmerWidth <= bounds.X + fillWidth - 4)
            {
                Rectangle shimmerRect = new Rectangle(shimmerX, bounds.Y + 2, shimmerWidth, bounds.Height - 4);
                renderer.DrawRect(spriteBatch, shimmerRect, Color.White, Alpha * 0.12f);
            }
        }

        // Border
        renderer.DrawParallelogramBorder(spriteBatch, bounds, theme.GoldDim, SkewFactor, 1, Alpha * 0.8f);

        // Text: "STR: 24" centered
        var font = theme.DefaultFont;
        if (font != null)
        {
            string text = $"{statName}: {value}";
            renderer.DrawTextCentered(spriteBatch, font, text, bounds, theme.TextPrimary, true, Alpha);
        }
    }

    private void DrawParallelogramGradient(
        SpriteBatch spriteBatch, GleamRenderer renderer,
        Rectangle bounds, Color bottomColor, Color topColor, float skewFactor)
    {
        float skewOffset = bounds.Height * skewFactor;
        float centerOffset = skewOffset / 2f;

        // Draw gradient using horizontal scan lines within parallelogram
        for (int y = 0; y < bounds.Height; y++)
        {
            float t = y / (float)bounds.Height;
            float leftX = bounds.X + skewOffset * (1 - t) - centerOffset;
            float rightX = bounds.Right + skewOffset * (1 - t) - centerOffset;

            // Clamp to not exceed fill width
            rightX = Math.Min(rightX, bounds.X + bounds.Width + skewOffset * (1 - t) - centerOffset);

            Color lineColor = Color.Lerp(topColor, bottomColor, t);
            int posY = bounds.Y + y;
            int lineWidth = (int)(rightX - leftX);

            if (lineWidth > 0)
            {
                renderer.DrawRect(spriteBatch,
                    new Rectangle((int)leftX, posY, lineWidth, 1),
                    lineColor, Alpha * 0.85f);
            }
        }
    }

    private void DrawParallelogramGlow(
        SpriteBatch spriteBatch, GleamRenderer renderer,
        Rectangle bounds, Color glowColor, float skewFactor)
    {
        int glowHeight = 6;
        float skewOffset = bounds.Height * skewFactor;
        float centerOffset = skewOffset / 2f;

        for (int y = 0; y < glowHeight && y < bounds.Height; y++)
        {
            float t = y / (float)bounds.Height;
            float leftX = bounds.X + skewOffset * (1 - t) - centerOffset;
            float rightX = bounds.Right + skewOffset * (1 - t) - centerOffset;

            float intensity = 1f - (y / (float)glowHeight);
            int posY = bounds.Y + y;
            int lineWidth = (int)(rightX - leftX);

            if (lineWidth > 0)
            {
                renderer.DrawRect(spriteBatch,
                    new Rectangle((int)leftX, posY, lineWidth, 1),
                    glowColor, Alpha * intensity * 0.4f);
            }
        }
    }

    private void DrawCornerAccents(SpriteBatch spriteBatch, GleamRenderer renderer, Rectangle bounds)
    {
        var theme = renderer.Theme;
        int accentSize = 8;
        Color accentColor = theme.GoldBright * Alpha;

        // Top-left corner accent
        renderer.DrawLine(spriteBatch,
            new Vector2(bounds.X, bounds.Y + accentSize),
            new Vector2(bounds.X + accentSize, bounds.Y),
            2, accentColor);

        // Top-right corner accent
        renderer.DrawLine(spriteBatch,
            new Vector2(bounds.Right - accentSize, bounds.Y),
            new Vector2(bounds.Right, bounds.Y + accentSize),
            2, accentColor);

        // Bottom-left corner accent
        renderer.DrawLine(spriteBatch,
            new Vector2(bounds.X, bounds.Bottom - accentSize),
            new Vector2(bounds.X + accentSize, bounds.Bottom),
            2, accentColor);

        // Bottom-right corner accent
        renderer.DrawLine(spriteBatch,
            new Vector2(bounds.Right - accentSize, bounds.Bottom),
            new Vector2(bounds.Right, bounds.Bottom - accentSize),
            2, accentColor);
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        // Stat panels are display-only, no input handling
        return false;
    }
}
