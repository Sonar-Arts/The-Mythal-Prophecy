using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Renders Sonar Arts logo using image textures with shader effects
/// </summary>
public static class LogoRenderer
{
    private const float LogoYPercent = 0.18f;
    private static readonly Vector2 ShadowOffset = new Vector2(4, 4);
    private static readonly Color ShadowColor = new Color(0, 0, 0, 150);

    public static void DrawRetroLogo(SpriteBatch spriteBatch, Texture2D texture,
        int screenWidth, int screenHeight, float revealAmount)
    {
        if (revealAmount <= 0 || texture == null) return;

        // Center the logo horizontally, position above the ship
        float logoX = (screenWidth - texture.Width) / 2f;
        float logoY = screenHeight * LogoYPercent;
        Vector2 position = new Vector2(logoX, logoY);

        // Draw shadow
        spriteBatch.Draw(texture, position + ShadowOffset, ShadowColor * revealAmount);

        // Draw logo with reveal opacity (no tint, preserve original colors)
        spriteBatch.Draw(texture, position, Color.White * revealAmount);
    }

    public static void DrawFantasyLogo(SpriteBatch spriteBatch, Texture2D texture,
        Effect glowEffect, int screenWidth, int screenHeight)
    {
        if (texture == null) return;

        // Center the logo horizontally, position above the ship
        float logoX = (screenWidth - texture.Width) / 2f;
        float logoY = screenHeight * LogoYPercent;
        Vector2 position = new Vector2(logoX, logoY);

        // Draw shadow first (no shader)
        spriteBatch.Draw(texture, position + ShadowOffset, ShadowColor);

        // Draw with glow shader if available
        if (glowEffect != null)
        {
            // End current batch to apply shader
            spriteBatch.End();

            // Set shader parameters - subtle godray effect
            glowEffect.Parameters["GlowIntensity"]?.SetValue(0.5f);
            glowEffect.Parameters["GlowRadius"]?.SetValue(12.0f);
            glowEffect.Parameters["TextureSize"]?.SetValue(new Vector2(texture.Width, texture.Height));

            // Draw with shader
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, effect: glowEffect);
            spriteBatch.Draw(texture, position, Color.White);
            spriteBatch.End();

            // Resume normal batch
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        }
        else
        {
            // Fallback: draw without shader
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
