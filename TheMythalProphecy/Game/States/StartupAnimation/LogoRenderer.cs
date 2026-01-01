using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TheMythalProphecy.Game.States.StartupAnimation.StartupAnimationConfig;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Renders Sonar Arts logo using image textures with shader effects
/// </summary>
public static class LogoRenderer
{
    private const float LogoYPercent = 0.18f;
    private static readonly Color ShadowColor = new Color(0, 0, 0, 150);

    public static void DrawRetroLogo(SpriteBatch spriteBatch, Texture2D texture,
        int screenWidth, int screenHeight, float revealAmount)
    {
        if (revealAmount <= 0 || texture == null) return;

        // Get scale factor for the logo
        float logoScale = Scale;
        Vector2 shadowOffset = new Vector2(S(4), S(4));

        // Calculate scaled dimensions
        float scaledWidth = texture.Width * logoScale;
        float scaledHeight = texture.Height * logoScale;

        // Center the logo horizontally, position above the ship
        float logoX = (screenWidth - scaledWidth) / 2f;
        float logoY = screenHeight * LogoYPercent;
        Vector2 position = new Vector2(logoX, logoY);

        // Draw shadow (scaled)
        spriteBatch.Draw(texture, position + shadowOffset, null, ShadowColor * revealAmount,
            0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);

        // Draw logo with reveal opacity (no tint, preserve original colors) (scaled)
        spriteBatch.Draw(texture, position, null, Color.White * revealAmount,
            0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
    }

    public static void DrawFantasyLogo(SpriteBatch spriteBatch, Texture2D texture,
        Effect glowEffect, int screenWidth, int screenHeight)
    {
        if (texture == null) return;

        // Get scale factor for the logo
        float logoScale = Scale;
        Vector2 shadowOffset = new Vector2(S(4), S(4));

        // Calculate scaled dimensions
        float scaledWidth = texture.Width * logoScale;
        float scaledHeight = texture.Height * logoScale;

        // Center the logo horizontally, position above the ship
        float logoX = (screenWidth - scaledWidth) / 2f;
        float logoY = screenHeight * LogoYPercent;
        Vector2 position = new Vector2(logoX, logoY);

        // Draw shadow first (no shader) (scaled)
        spriteBatch.Draw(texture, position + shadowOffset, null, ShadowColor,
            0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);

        // Draw with glow shader if available
        if (glowEffect != null)
        {
            // End current batch to apply shader
            spriteBatch.End();

            // Set shader parameters - subtle godray effect (scaled glow radius)
            glowEffect.Parameters["GlowIntensity"]?.SetValue(0.5f);
            glowEffect.Parameters["GlowRadius"]?.SetValue(S(12.0f));
            glowEffect.Parameters["TextureSize"]?.SetValue(new Vector2(texture.Width, texture.Height));

            // Draw with shader (scaled)
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, effect: glowEffect);
            spriteBatch.Draw(texture, position, null, Color.White,
                0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
            spriteBatch.End();

            // Resume normal batch
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        }
        else
        {
            // Fallback: draw without shader (scaled)
            spriteBatch.Draw(texture, position, null, Color.White,
                0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        }
    }
}
