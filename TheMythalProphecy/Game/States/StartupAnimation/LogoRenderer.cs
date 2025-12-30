using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Renders stylized "SONAR ARTS" logo text using fonts
/// </summary>
public static class LogoRenderer
{
    public static void DrawRetroLogo(SpriteBatch spriteBatch, SpriteFont font,
        int screenWidth, int screenHeight, float revealAmount)
    {
        if (revealAmount <= 0) return;

        string text = "SONAR ARTS";
        Vector2 textSize = font.MeasureString(text);
        float scale = 1.5f;
        float textX = (screenWidth - textSize.X * scale) / 2;
        float textY = screenHeight * 0.06f;

        // Retro green colors - terminal/sonar style
        Color glowColor = new Color(100, 255, 150) * (revealAmount * 0.25f);
        Color shadowColor = new Color(0, 60, 30) * revealAmount;
        Color mainColor = new Color(50, 220, 120) * revealAmount;
        Color scanlineColor = new Color(30, 180, 90) * (revealAmount * 0.7f);

        // Outer glow layers
        for (int i = 3; i >= 1; i--)
        {
            float offset = i * 3;
            spriteBatch.DrawString(font, text, new Vector2(textX - offset, textY), glowColor,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, new Vector2(textX + offset, textY), glowColor,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, new Vector2(textX, textY - offset), glowColor,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, new Vector2(textX, textY + offset), glowColor,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        // Drop shadow
        spriteBatch.DrawString(font, text, new Vector2(textX + 4, textY + 4), shadowColor,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        // Main text
        spriteBatch.DrawString(font, text, new Vector2(textX, textY), mainColor,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        // Scanline highlight effect
        spriteBatch.DrawString(font, text, new Vector2(textX, textY - 1), scanlineColor,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public static void DrawFantasyLogo(SpriteBatch spriteBatch, SpriteFont font,
        int screenWidth, int screenHeight)
    {
        string text = "Sonar Arts";
        Vector2 textSize = font.MeasureString(text);
        float scale = 1.4f;
        float textX = (screenWidth - textSize.X * scale) / 2;
        float textY = screenHeight * 0.06f;

        // Fantasy purple/gold colors - elegant, mystical
        Color deepShadow = new Color(20, 10, 40) * 0.9f;
        Color goldGlow = new Color(255, 200, 100) * 0.35f;
        Color mainPurple = new Color(160, 120, 200);
        Color highlight = new Color(220, 190, 255) * 0.5f;
        Color goldAccent = new Color(255, 220, 140) * 0.6f;

        // Deep shadow layers for depth
        for (int i = 4; i >= 1; i--)
        {
            float offset = i * 2;
            Color layerShadow = deepShadow * (0.3f + (4 - i) * 0.1f);
            spriteBatch.DrawString(font, text, new Vector2(textX + offset, textY + offset), layerShadow,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        // Golden glow outline
        for (int i = 2; i >= 1; i--)
        {
            float offset = i * 2;
            spriteBatch.DrawString(font, text, new Vector2(textX - offset, textY), goldGlow,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, new Vector2(textX + offset, textY), goldGlow,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, new Vector2(textX, textY - offset), goldGlow,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        // Main text
        spriteBatch.DrawString(font, text, new Vector2(textX, textY), mainPurple,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        // Top highlight for embossed look
        spriteBatch.DrawString(font, text, new Vector2(textX - 1, textY - 1), highlight,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        // Gold accent on bottom edge
        spriteBatch.DrawString(font, text, new Vector2(textX + 1, textY + 1), goldAccent,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
